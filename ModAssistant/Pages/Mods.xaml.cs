﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
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

        public List<string> DefaultMods = new List<string>(){ "SongLoader", "ScoreSaber", "BeatSaverDownloader" };
        public Mod[] ModsList;
        public Mod[] AllModsList;
        public static List<Mod> InstalledMods = new List<Mod>();
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
            if (View != null)
                View.Refresh();
        }

        public async void LoadMods()
        {
            MainWindow.Instance.InstallButton.IsEnabled = false;
            MainWindow.Instance.GameVersionsBox.IsEnabled = false;

            if (ModsList != null)
                Array.Clear(ModsList, 0, ModsList.Length);
            if (AllModsList != null)
                Array.Clear(AllModsList, 0, AllModsList.Length);

            InstalledMods = new List<Mod>();
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
            } else
            {
                InstalledColumn.Width = 0;
                UninstallColumn.Width = 0;
                DescriptionColumn.Width = 800;
            }

            MainWindow.Instance.MainText = "Loading Mods...";
            await Task.Run(() => PopulateModsList());

            ModsListView.ItemsSource = ModList;

            View = (CollectionView)CollectionViewSource.GetDefaultView(ModsListView.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Category");
            View.GroupDescriptions.Add(groupDescription);

            this.DataContext = this;

            RefreshModsList();
            ModsListView.Visibility = Visibility.Visible;
            MainWindow.Instance.MainText = "Finished Loading Mods.";

            MainWindow.Instance.InstallButton.IsEnabled = true;
            MainWindow.Instance.GameVersionsBox.IsEnabled = true;
        }

        private void CheckInstalledMods()
        {
            string json = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Utils.Constants.BeatModsApiUrl + "mod");
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.UserAgent = "ModAssistant/" + App.Version;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                var serializer = new JavaScriptSerializer();
                AllModsList = serializer.Deserialize<Mod[]>(reader.ReadToEnd());
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
            foreach (string file in Directory.GetFileSystemEntries(Path.Combine(App.BeatSaberInstallDirectory, directory)))
            {
                if (File.Exists(file) && Path.GetExtension(file) == ".dll" || Path.GetExtension(file) == ".manifest")
                {
                    Mod mod = GetModFromHash(Utils.CalculateMd5(file));
                    if (mod != null)
                    {
                        AddDetectedMod(mod);
                    }
                }
            }
        }

        private void GetBsipaVersion()
        {
            string injectorPath = Path.Combine(App.BeatSaberInstallDirectory, "Beat Saber_Data", "Managed", "IPA.Injector.dll");
            if (!File.Exists(injectorPath)) return;

            string injectorHash = Utils.CalculateMd5(injectorPath);
            foreach (Mod mod in AllModsList)
            {
                if (mod.Name.ToLower() == "bsipa")
                {
                    foreach (Mod.DownloadLink download in mod.Downloads)
                    {
                        foreach (Mod.FileHashes fileHash in download.HashMd5)
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

        private void AddDetectedMod(Mod mod)
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

        private Mod GetModFromHash(string hash)
        {
            foreach (Mod mod in AllModsList)
            {
                if (mod.Name.ToLower() != "bsipa")
                {
                    foreach (Mod.DownloadLink download in mod.Downloads)
                    {
                        foreach (Mod.FileHashes fileHash in download.HashMd5)
                        {
                            if (fileHash.Hash == hash)
                                return mod;
                        }
                    }
                }
            }

            return null;
        }

        public void PopulateModsList()
        {
            string json = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Utils.Constants.BeatModsApiUrl + Utils.Constants.BeatModsModsOptions + "&gameVersion=" + MainWindow.GameVersion);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.UserAgent = "ModAssistant/" + App.Version;

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    var serializer = new JavaScriptSerializer();
                    ModsList = serializer.Deserialize<Mod[]>(reader.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Could not load mods list.\n\n" + e);
                return;
            }

            foreach (Mod mod in ModsList)
            {
                bool preSelected = mod.Required;
                if (DefaultMods.Contains(mod.Name) || (App.SaveModSelection && App.SavedMods.Contains(mod.Name)))
                {
                    preSelected = true;
                    if(!App.SavedMods.Contains(mod.Name))
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

                foreach (Mod installedMod in InstalledMods)
                {
                    if (mod.Name == installedMod.Name)
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

            foreach (Mod mod in ModsList)
            {
                ResolveDependencies(mod);
            }
        }

        public async void InstallMods ()
        {
            MainWindow.Instance.InstallButton.IsEnabled = false;
            string installDirectory = App.BeatSaberInstallDirectory;

            foreach (Mod mod in ModsList)
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
                else if(mod.ListItem.IsSelected)
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

        private void InstallMod (Mod mod, string directory)
        {
            string downloadLink = null;

            foreach (Mod.DownloadLink link in mod.Downloads)
            {
                if (link.Type == "universal")
                {
                    downloadLink = link.Url;
                    break;
                } else if (link.Type.ToLower() == App.BeatSaberInstallType.ToLower())
                {
                    downloadLink = link.Url;
                    break;
                }
            }

            if (String.IsNullOrEmpty(downloadLink))
            {
                System.Windows.MessageBox.Show($"Could not find download link for {mod.Name}");
                return;
            }

            using (MemoryStream stream = new MemoryStream(DownloadMod(Utils.Constants.BeatModsUrl + downloadLink)))
            {
                using (ZipArchive archive = new ZipArchive(stream))
                {
                    foreach (ZipArchiveEntry file in archive.Entries)
                    {
                        string fileDirectory = Path.GetDirectoryName(Path.Combine(directory, file.FullName));
                        if (!Directory.Exists(fileDirectory))
                            Directory.CreateDirectory(fileDirectory);

                        if(!String.IsNullOrEmpty(file.Name))
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

        private byte[] DownloadMod (string link)
        {
            byte[] zip = new WebClient().DownloadData(link);
            return zip;
        }

        private void RegisterDependencies(Mod dependent)
        {
            if (dependent.Dependencies.Length == 0)
                return;

            foreach (Mod mod in ModsList)
            {
                foreach (Mod.Dependency dep in dependent.Dependencies)
                {
                    
                    if (dep.Name == mod.Name)
                    {
                        dep.Mod = mod;
                        mod.Dependents.Add(dependent);
                        
                    }
                }
            }
        }

        private void ResolveDependencies(Mod dependent)
        {
            if (dependent.ListItem.IsSelected && dependent.Dependencies.Length > 0)
            {
                foreach (Mod.Dependency dependency in dependent.Dependencies)
                {
                    if (dependency.Mod.ListItem.IsEnabled)
                    {
                        dependency.Mod.ListItem.PreviousState = dependency.Mod.ListItem.IsSelected;
                        dependency.Mod.ListItem.IsSelected = true;
                        dependency.Mod.ListItem.IsEnabled = false;
                        ResolveDependencies(dependency.Mod);
                    }
                }
            }
        }

        private void UnresolveDependencies(Mod dependent)
        {
            if (!dependent.ListItem.IsSelected && dependent.Dependencies.Length > 0)
            {
                foreach (Mod.Dependency dependency in dependent.Dependencies)
                {
                    if (!dependency.Mod.ListItem.IsEnabled)
                    {
                        bool needed = false;
                        foreach (Mod dep in dependency.Mod.Dependents)
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
            Mod mod = ((sender as System.Windows.Controls.CheckBox).Tag as Mod);
            mod.ListItem.IsSelected = true;
            ResolveDependencies(mod);
            App.SavedMods.Add(mod.Name);
            Properties.Settings.Default.SavedMods = String.Join(",", App.SavedMods.ToArray());
            Properties.Settings.Default.Save();
            RefreshModsList();
        }

        private void ModCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Mod mod = ((sender as System.Windows.Controls.CheckBox).Tag as Mod);
            mod.ListItem.IsSelected = false;
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
            public Mod ModInfo { get; set; }
            public string Category { get; set; }

            public Mod InstalledModInfo { get; set; }
            public bool IsInstalled { get; set; }
            private string _installedVersion;
            public string InstalledVersion
            {
                get
                {
                    return (String.IsNullOrEmpty(_installedVersion) || !IsInstalled) ? "-" : _installedVersion;
                }
                set
                {
                    _installedVersion = value;
                }
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
                get
                {
                    return (!ModInfo.Required && IsInstalled);
                }
            }

            public string CanSeeDelete
            {
                get
                {
                    if (!ModInfo.Required && IsInstalled)
                        return "Visible";
                    else
                        return "Hidden";
                }
            }

        }

        private void ModsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindow.Instance.InfoButton.IsEnabled = true;
        }

        private void UninstallBsipa(Mod.DownloadLink links)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(App.BeatSaberInstallDirectory, "IPA.exe"),
                WorkingDirectory = App.BeatSaberInstallDirectory,
                Arguments = "--revert -n"
            }).WaitForExit();

            foreach (Mod.FileHashes files in links.HashMd5)
            {
                string file = files.File.Replace("IPA/", "").Replace("Data", "Beat Saber_Data");
                if (File.Exists(Path.Combine(App.BeatSaberInstallDirectory, file)))
                    File.Delete(Path.Combine(App.BeatSaberInstallDirectory, file));
            }
        }

        private void Uninstall_Click(object sender, RoutedEventArgs e)
        {
            Mod mod = ((sender as System.Windows.Controls.Button).Tag as Mod);
            Mod installedMod = mod.ListItem.InstalledModInfo;
            if (System.Windows.Forms.MessageBox.Show($"Are you sure you want to remove {mod.Name}?\nThis could break your other mods.", $"Uninstall {mod.Name}?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Mod.DownloadLink links = null;
                foreach (Mod.DownloadLink link in installedMod.Downloads)
                {
                    if (link.Type.ToLower() == "universal" || link.Type.ToLower() == App.BeatSaberInstallType.ToLower())
                    {
                        links = link;
                        break;
                    }
                }
                if (installedMod.Name.ToLower() == "bsipa")
                    UninstallBsipa(links);
                foreach (Mod.FileHashes files in links.HashMd5)
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