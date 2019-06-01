using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.IO.Compression;
using System.Diagnostics;
using System.Windows.Forms;

namespace ModAssistant.Pages
{
    /// <summary>
    /// Interaction logic for Mods.xaml
    /// </summary>
    public sealed partial class Mods : Page
    {
        public static Mods Instance = new Mods();

        public List<string> DefaultMods = new List<string>() {"SongLoader", "ScoreSaber", "BeatSaverDownloader"};
        public Classes.Mod[] ModsList;
        public Classes.Mod[] AllModsList;
        public static List<Classes.Mod> InstalledMods = new List<Classes.Mod>();
        public List<string> CategoryNames = new List<string>();
        public CollectionView View;
        public bool PendingChanges;

        public List<ModListItem> ModList { get; set; }

        public Mods()
        {
            InitializeComponent();
        }

        private void RefreshModsList()
        {
            View?.Refresh();
        }

        public async void LoadMods()
        {
            MainWindow.Instance.InstallButton.IsEnabled = false;
            MainWindow.Instance.GameVersionsBox.IsEnabled = false;

            if (ModsList != null)
                Array.Clear(ModsList, 0, ModsList.Length);
            if (AllModsList != null)
                Array.Clear(AllModsList, 0, AllModsList.Length);

            InstalledMods = new List<Classes.Mod>();
            CategoryNames = new List<string>();
            ModList = new List<ModListItem>();

            ModsListView.Visibility = Visibility.Hidden;

            if (App.CheckInstalledMods)
            {
                MainWindow.Instance.MainText = "Checking Installed Mods...";
                await Task.Run(() => CheckInstalledMods());
                InstalledColumn.Width = Double.NaN;
                UninstallColumn.Width = 70;
                DescriptionColumn.Width = 750;
            }
            else
            {
                InstalledColumn.Width = 0;
                UninstallColumn.Width = 0;
                DescriptionColumn.Width = 800;
            }

            MainWindow.Instance.MainText = "Loading Mods...";
            await Task.Run(() => PopulateModsList());

            ModsListView.ItemsSource = ModList;

            View = (CollectionView) CollectionViewSource.GetDefaultView(ModsListView.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Category");
            View.GroupDescriptions.Add(groupDescription);

            DataContext = this;

            RefreshModsList();
            ModsListView.Visibility = Visibility.Visible;
            MainWindow.Instance.MainText = "Finished Loading Mods.";

            MainWindow.Instance.InstallButton.IsEnabled = true;
            MainWindow.Instance.GameVersionsBox.IsEnabled = true;
        }

        private void CheckInstalledMods()
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(Classes.Utils.Constants.BeatModsApiUrl + "mod");
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.UserAgent = "ModAssistant/" + App.Version;

            using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                var serializer = new JavaScriptSerializer();
                AllModsList = serializer.Deserialize<Classes.Mod[]>(reader.ReadToEnd());
            }

            List<string> empty = new List<string>();
            GetBsipaVersion();
            CheckInstallDir("IPA/Pending/Plugins", empty);
            CheckInstallDir("IPA/Pending/Libs", empty);
            CheckInstallDir("Plugins", empty);
            CheckInstallDir("Libs", empty);
        }

        private void CheckInstallDir(string directory, List<string> blacklist)
        {
            if (!Directory.Exists(Path.Combine(App.BeatSaberInstallDirectory, directory)))
                return;
            foreach (string file in Directory.GetFileSystemEntries(Path.Combine(App.BeatSaberInstallDirectory,
                directory)))
            {
                if (File.Exists(file) && Path.GetExtension(file) == ".dll" || Path.GetExtension(file) == ".manifest")
                {
                    Classes.Mod mod = GetModFromHash(Classes.Utils.CalculateMd5(file));
                    if (mod != null)
                    {
                        AddDetectedMod(mod);
                    }
                }
            }
        }

        private void GetBsipaVersion()
        {
            string injectorPath = Path.Combine(App.BeatSaberInstallDirectory, "Beat Saber_Data", "Managed",
                "IPA.Injector.dll");
            if (!File.Exists(injectorPath)) return;

            string injectorHash = Classes.Utils.CalculateMd5(injectorPath);
            foreach (Classes.Mod mod in AllModsList)
            {
                if (mod.Name.ToLower().Equals("bsipa"))
                {
                    foreach (Classes.Mod.DownloadLink download in mod.Downloads)
                    {
                        foreach (Classes.Mod.FileHashes fileHash in download.HashMd5)
                        {
                            if (fileHash.Hash == injectorHash)
                            {
                                AddDetectedMod(mod);
                            }
                        }
                    }
                }
            }
        }

        private void AddDetectedMod(Classes.Mod mod)
        {
            if (!InstalledMods.Contains(mod))
            {
                InstalledMods.Add(mod);
                if (App.SelectInstalledMods && !DefaultMods.Contains(mod.Name))
                {
                    DefaultMods.Add(mod.Name);
                }
            }
        }

        private Classes.Mod GetModFromHash(string hash)
        {
            foreach (Classes.Mod mod in AllModsList)
            {
                if (!mod.Name.ToLower().Equals("bsipa"))
                {
                    foreach (Classes.Mod.DownloadLink download in mod.Downloads)
                    {
                        foreach (Classes.Mod.FileHashes fileHash in download.HashMd5)
                        {
                            if (fileHash.Hash.Equals(hash))
                                return mod;
                        }
                    }
                }
            }

            return null;
        }

        public void PopulateModsList()
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(
                Classes.Utils.Constants.BeatModsApiUrl +
                Classes.Utils.Constants.BeatModsModsOptions +
                "&gameVersion=" +
                MainWindow.GameVersion);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.UserAgent = "ModAssistant/" + App.Version;

            try
            {
                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    var serializer = new JavaScriptSerializer();
                    ModsList = serializer.Deserialize<Classes.Mod[]>(reader.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Could not load mods list.\n\n" + e);
                return;
            }

            foreach (Classes.Mod mod in ModsList)
            {
                bool preSelected = mod.Required;
                if (DefaultMods.Contains(mod.Name) || (App.SaveModSelection && App.SavedMods.Contains(mod.Name)))
                {
                    preSelected = true;
                    if (!App.SavedMods.Contains(mod.Name))
                    {
                        App.SavedMods.Add(mod.Name);
                    }
                }

                RegisterDependencies(mod);

                ModListItem listItem = new ModListItem()
                {
                    IsSelected = preSelected,
                    IsEnabled = !mod.Required,
                    ModName = mod.Name,
                    ModVersion = mod.Version,
                    ModDescription = mod.Description.Replace("\r\n", " ").Replace("\n", " "),
                    ModInfo = mod,
                    Category = mod.Category
                };

                foreach (Classes.Mod installedMod in InstalledMods)
                {
                    if (mod.Name.Equals(installedMod.Name))
                    {
                        listItem.InstalledModInfo = installedMod;
                        listItem.IsInstalled = true;
                        listItem.InstalledVersion = installedMod.Version;
                        break;
                    }
                }

                mod.ListItem = listItem;
                ModList.Add(listItem);
            }

            foreach (Classes.Mod mod in ModsList)
            {
                ResolveDependencies(mod);
            }
        }

        public async void InstallMods()
        {
            MainWindow.Instance.InstallButton.IsEnabled = false;
            string installDirectory = App.BeatSaberInstallDirectory;

            foreach (Classes.Mod mod in ModsList)
            {
                if (mod.Name.ToLower() == "bsipa")
                {
                    MainWindow.Instance.MainText = $"Installing {mod.Name}...";
                    await Task.Run(() => InstallMod(mod, installDirectory));
                    MainWindow.Instance.MainText = $"Installed {mod.Name}.";
                    if (!File.Exists(Path.Combine(installDirectory, "winhttp.dll")))
                    {
                        await Task.Run(() =>
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = Path.Combine(installDirectory, "IPA.exe"),
                                WorkingDirectory = installDirectory,
                                Arguments = "-n"
                            }).WaitForExit()
                        );
                    }
                }
                else if (mod.ListItem.IsSelected)
                {
                    MainWindow.Instance.MainText = $"Installing {mod.Name}...";
                    await Task.Run(() => InstallMod(mod, Path.Combine(installDirectory, @"IPA\Pending")));
                    MainWindow.Instance.MainText = $"Installed {mod.Name}.";
                }
            }

            MainWindow.Instance.MainText = "Finished installing mods.";
            MainWindow.Instance.InstallButton.IsEnabled = true;
            RefreshModsList();
        }

        private void InstallMod(Classes.Mod mod, string directory)
        {
            string downloadLink = string.Empty;

            foreach (Classes.Mod.DownloadLink link in mod.Downloads)
            {
                if (link.Type.ToLower() == "universal" ||
                    link.Type.ToLower() == App.BeatSaberInstallType.ToLower())
                {
                    downloadLink = link.Url;
                    break;
                }
            }

            if (string.IsNullOrEmpty(downloadLink))
            {
                System.Windows.MessageBox.Show($"Could not find download link for {mod.Name}");
                return;
            }

            using (MemoryStream stream =
                new MemoryStream(DownloadMod(Classes.Utils.Constants.BeatModsUrl + downloadLink)))
            {
                using (ZipArchive archive = new ZipArchive(stream))
                {
                    foreach (ZipArchiveEntry file in archive.Entries)
                    {
                        string fileDirectory = Path.GetDirectoryName(Path.Combine(directory, file.FullName));
                        if (!Directory.Exists(fileDirectory))
                            Directory.CreateDirectory(fileDirectory);

                        if (!String.IsNullOrEmpty(file.Name))
                            file.ExtractToFile(Path.Combine(directory, file.FullName), true);
                    }
                }
            }

            if (App.CheckInstalledMods)
            {
                mod.ListItem.IsInstalled = true;
                mod.ListItem.InstalledVersion = mod.Version;
                mod.ListItem.InstalledModInfo = mod;
            }
        }

        private byte[] DownloadMod(string link)
        {
            using (var client = new WebClient())
            {
                return client.DownloadData(link);
            }
        }

        private void RegisterDependencies(Classes.Mod dependent)
        {
            if (dependent.Dependencies.Length == 0)
                return;

            foreach (Classes.Mod mod in ModsList)
            {
                foreach (Classes.Mod.Dependency dep in dependent.Dependencies)
                {
                    if (dep.Name.Equals(mod.Name))
                    {
                        dep.Mod = mod;
                        mod.Dependents.Add(dependent);
                    }
                }
            }
        }

        private void ResolveDependencies(Classes.Mod dependent)
        {
            if (dependent.ListItem.IsSelected && dependent.Dependencies.Length > 0)
            {
                foreach (Classes.Mod.Dependency dependency in dependent.Dependencies)
                {
                    if (dependency.Mod.ListItem.IsEnabled)
                    {
                        dependency.Mod.ListItem.PreviousState = dependency.Mod.ListItem.IsSelected;
                        dependency.Mod.ListItem.IsSelected = true;
                        dependency.Mod.ListItem.IsEnabled = !dependency.Mod.ListItem.IsEnabled;
                        ResolveDependencies(dependency.Mod);
                    }
                }
            }
        }

        private void UnresolveDependencies(Classes.Mod dependent)
        {
            if (!dependent.ListItem.IsSelected && dependent.Dependencies.Length > 0)
            {
                foreach (Classes.Mod.Dependency dependency in dependent.Dependencies)
                {
                    if (!dependency.Mod.ListItem.IsEnabled)
                    {
                        bool needed = false;
                        foreach (Classes.Mod dep in dependency.Mod.Dependents)
                        {
                            if (dep.ListItem.IsSelected)
                            {
                                needed = true;
                                break;
                            }
                        }

                        if (!needed && !dependency.Mod.Required)
                        {
                            dependency.Mod.ListItem.IsSelected = dependency.Mod.ListItem.PreviousState;
                            dependency.Mod.ListItem.IsEnabled = true;
                            UnresolveDependencies(dependency.Mod);
                        }
                    }
                }
            }
        }

        private void ModCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBoxCheckOrUnCheck(Converter(sender), true);
        }

        private void ModCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBoxCheckOrUnCheck(Converter(sender), false);
        }

        private Classes.Mod Converter(object sender)
        {
            return (sender as System.Windows.Controls.CheckBox).Tag as Classes.Mod;
        }

        private void CheckBoxCheckOrUnCheck(Classes.Mod mod, bool isSelected)
        {
            mod.ListItem.IsSelected = isSelected;

            UnresolveDependencies(mod);
            App.SavedMods.Remove(mod.Name);

            Properties.Settings.Default.SavedMods = String.Join(",", App.SavedMods.ToArray());
            Properties.Settings.Default.Save();

            RefreshModsList();
        }

        public class Category
        {
            public string CategoryName { get; set; }
            public List<ModListItem> Mods = new List<ModListItem>();
        }

        public class ModListItem
        {
            public string ModName { get; set; }
            public string ModVersion { get; set; }
            public string ModDescription { get; set; }
            public bool PreviousState { get; set; }

            public bool IsEnabled { get; set; }
            public bool IsSelected { get; set; }
            public Classes.Mod ModInfo { get; set; }
            public string Category { get; set; }

            public Classes.Mod InstalledModInfo { get; set; }
            public bool IsInstalled { get; set; }
            private string _installedVersion;

            public string InstalledVersion
            {
                get { return (String.IsNullOrEmpty(_installedVersion) || !IsInstalled) ? "-" : _installedVersion; }
                set { _installedVersion = value; }
            }

            public string GetVersionColor
            {
                get
                {
                    if (!IsInstalled) return "Black";
                    return InstalledVersion == ModVersion ? "Green" : "Red";
                }
            }

            public bool CanDelete
            {
                get { return (!ModInfo.Required && IsInstalled); }
            }

            public string CanSeeDelete
            {
                get
                {
                    if (!ModInfo.Required && IsInstalled)
                        return "Visible";
                    return "Hidden";
                }
            }
        }

        private void ModsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindow.Instance.InfoButton.IsEnabled = true;
        }

        private void UninstallBsipa(Classes.Mod.DownloadLink links)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(App.BeatSaberInstallDirectory, "IPA.exe"),
                WorkingDirectory = App.BeatSaberInstallDirectory,
                Arguments = "--revert -n"
            }).WaitForExit();

            foreach (Classes.Mod.FileHashes files in links.HashMd5)
            {
                string file = files.File.Replace("IPA/", "").Replace("Data", "Beat Saber_Data");
                if (File.Exists(Path.Combine(App.BeatSaberInstallDirectory, file)))
                    File.Delete(Path.Combine(App.BeatSaberInstallDirectory, file));
            }
        }

        private void Uninstall_Click(object sender, RoutedEventArgs e)
        {
            Classes.Mod mod = ((sender as System.Windows.Controls.Button).Tag as Classes.Mod);
            Classes.Mod installedMod = mod.ListItem.InstalledModInfo;
            if (System.Windows.Forms.MessageBox.Show(
                    $"Are you sure you want to remove {mod.Name}?\nThis could break your other mods.",
                    $"Uninstall {mod.Name}?", MessageBoxButtons.YesNo) ==
                DialogResult.Yes)
            {
                Classes.Mod.DownloadLink links = null;
                foreach (Classes.Mod.DownloadLink link in installedMod.Downloads)
                {
                    if (link.Type.ToLower() == "universal" || link.Type.ToLower() == App.BeatSaberInstallType.ToLower())
                    {
                        links = link;
                        break;
                    }
                }

                if (installedMod.Name.ToLower().Equals("bsipa"))
                    UninstallBsipa(links);
                foreach (Classes.Mod.FileHashes files in links.HashMd5)
                {
                    if (File.Exists(Path.Combine(App.BeatSaberInstallDirectory, files.File)))
                        File.Delete(Path.Combine(App.BeatSaberInstallDirectory, files.File));
                    if (File.Exists(Path.Combine(App.BeatSaberInstallDirectory, "IPA", "Pending", files.File)))
                        File.Delete(Path.Combine(App.BeatSaberInstallDirectory, "IPA", "Pending", files.File));
                }

                mod.ListItem.IsInstalled = false;
                mod.ListItem.InstalledVersion = null;
                if (App.SelectInstalledMods)
                {
                    mod.ListItem.IsSelected = false;
                    UnresolveDependencies(mod);
                    App.SavedMods.Remove(mod.Name);
                    Properties.Settings.Default.SavedMods = String.Join(",", App.SavedMods.ToArray());
                    Properties.Settings.Default.Save();
                    RefreshModsList();
                }

                View.Refresh();
            }
        }
    }
}