using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Navigation;
using static ModAssistant.Http;
using ModAssistant.Libs;
using System.Windows.Media.Animation;
using TextBox = System.Windows.Controls.TextBox;
using static ModAssistant.Mod;

namespace ModAssistant.Pages
{
    /// <summary>
    /// Interaction logic for Mods.xaml
    /// </summary>
    public sealed partial class Mods : Page
    {
        public static Mods Instance = new Mods();

        public List<string> DefaultMods = new List<string>() { "SongCore", "ScoreSaber", "BeatSaverDownloader", "BeatSaverVoting", "PlaylistCore", "Survey" };
        public List<string> DefaultModsNetvios = new List<string>() { "Base-Netvios" };
        /*        public List<string> DefaultModsNetvios = new List<string>() { "Base-Netvios", "SongCore-Netvios", "BeatSaberMarkupLanguage-Netvios", "PlayerDataPlugin-Netvios", "NetviosSdkPlugin-Netvios", "NetviosHelperPlugin-Netvios", "LocalizationPlugin-Netvios", "BeatSaverSharp-Netvios", "BS_Utils-Netvios", "BeatSaverDownloader-Netvios", "CommonPlugin-Netvios" };*/
        public List<string> NetviosNotCompatiblePluginList = new List<string>() { "BSIPA", "Chroma", "Versus" };
        public Mod[] ModsList;
        public Mod[] AllModsList;
        public TranslationWGzeyu[] ModsTranslationWGzeyu;
        public static List<Mod> InstalledMods = new List<Mod>();
        public List<string> CategoryNames = new List<string>();
        public CollectionView view;
        public bool PendingChanges;
        public Dictionary<String, String> CategoryTranslation = new Dictionary<string, string>();
        public Dictionary<string, SemVersion> inListMods = new Dictionary<string, SemVersion>();
        public string BSIPAVersion = "Base-Netvios";
        public string BSIPAVersionUncheck = "BSIPA-NetviosSpecial";
        public string lastBSIPA = "";

        private readonly SemaphoreSlim _modsLoadSem = new SemaphoreSlim(1, 1);

        public List<ModListItem> ModList { get; set; }

        public Mods()
        {
            InitializeComponent();
        }

        private void RefreshModsList()
        {
            if (view != null)
            {
                view.Refresh();
            }
        }

        public void RefreshColumns()
        {
            if (MainWindow.Instance.Main.Content != Mods.Instance) return;
            double viewWidth = ModsListView.ActualWidth;
            double totalSize = 0;
            GridViewColumn description = null;
            GridView grid = ModsListView.View as GridView;
            if (grid != null)
            {
                foreach (var column in grid.Columns)
                {
                    if (column.Header?.ToString() == FindResource("Mods:Header:Description").ToString())
                    {
                        description = column;
                    }
                    else
                    {
                        totalSize += column.ActualWidth;
                    }
                    if (double.IsNaN(column.Width))
                    {
                        column.Width = column.ActualWidth;
                        column.Width = double.NaN;
                    }
                }
                double descriptionNewWidth = viewWidth - totalSize - 35;
                description.Width = descriptionNewWidth > 200 ? descriptionNewWidth : 200;
            }
        }

        public async Task LoadMods()
        {
            var versionLoadSuccess = await MainWindow.Instance.VersionLoadStatus.Task;
            if (versionLoadSuccess == false) return;

            await _modsLoadSem.WaitAsync();

            try
            {
                MainWindow.Instance.InstallButton.IsEnabled = false;
                MainWindow.Instance.GameVersionsBox.IsEnabled = false;
                MainWindow.Instance.InfoButton.IsEnabled = false;

                if (ModsList != null)
                {
                    Array.Clear(ModsList, 0, ModsList.Length);
                }

                if (AllModsList != null)
                {
                    Array.Clear(AllModsList, 0, AllModsList.Length);
                }

                InstalledMods = new List<Mod>();
                CategoryNames = new List<string>();
                ModList = new List<ModListItem>();
                CategoryTranslationInit();

                ModsListView.Visibility = Visibility.Hidden;

                if (App.CheckInstalledMods)
                {
                    MainWindow.Instance.MainText = $"{FindResource("Mods:CheckingInstalledMods")}...";
                    await Task.Run(async () => await CheckInstalledMods());
                    InstalledColumn.Width = double.NaN;
                    UninstallColumn.Width = 70;
                    DescriptionColumn.Width = 750;
                }
                else
                {
                    InstalledColumn.Width = 0;
                    UninstallColumn.Width = 0;
                    DescriptionColumn.Width = 800;
                }

                MainWindow.Instance.MainText = $"{FindResource("Mods:LoadingMods")}{(Properties.Settings.Default.LanguageCode == "zh" ? "(从" + Properties.Settings.Default.DownloadServer + ")。" : "from " + Properties.Settings.Default.DownloadServer + "...")}";
                await Task.Run(async () => await PopulateModsList());

                ModsListView.ItemsSource = ModList;

                view = (CollectionView)CollectionViewSource.GetDefaultView(ModsListView.ItemsSource);
                PropertyGroupDescription groupDescription = new PropertyGroupDescription("Category");
                view.GroupDescriptions.Add(groupDescription);

                this.DataContext = this;

                RefreshModsList();
                ModsListView.Visibility = Visibility.Visible;
                MainWindow.Instance.MainText = $"{FindResource("Mods:FinishedLoadingMods")}{(Properties.Settings.Default.LanguageCode == "zh" ? "。" : ".")}";

                MainWindow.Instance.InstallButton.IsEnabled = true;
                MainWindow.Instance.GameVersionsBox.IsEnabled = true;
            }
            finally
            {
                _modsLoadSem.Release();
            }
        }

        public async Task CheckInstalledMods()
        {
            await GetAllMods();
            List<string> empty = new List<string>();
            GetBSIPAVersion();
            CheckInstallDir("IPA/Pending/Plugins", empty);
            CheckInstallDir("IPA/Pending/Libs", empty);
            CheckInstallDir("Plugins", empty);
            CheckInstallDir("Libs", empty);
        }

        public async Task GetAllMods()
        {
            var resp = await HttpClient.GetAsync(Utils.Constants.BeatModsAPIUrl + "mod");
            var body = await resp.Content.ReadAsStringAsync();
            if (Properties.Settings.Default.LanguageCode == "zh" && Properties.Settings.Default.DownloadServer != "网易版@BeatMods.top")
            {
                string BeatModsTranslation_now = Utils.Constants.BeatModsTranslation_wgzeyu;
                if (Properties.Settings.Default.DownloadServer != "国内源@WGzeyu")
                {
                     BeatModsTranslation_now = Utils.Constants.BeatModsTranslation_beatmods;
                }
                var resp_WGzeyu = await HttpClient.GetAsync(BeatModsTranslation_now);
                var body_WGzeyu = await resp_WGzeyu.Content.ReadAsStringAsync();
                MainWindow.Instance.MainText = $"{(Properties.Settings.Default.LanguageCode == "zh" ? "正在获取Mod翻译@WGzeyu" : "Fetching additional translation form WGzuyu.")}";
                ModsTranslationWGzeyu = JsonSerializer.Deserialize<TranslationWGzeyu[]>(body_WGzeyu);
            }
            AllModsList = JsonSerializer.Deserialize<Mod[]>(body);
        }

        private void CheckInstallDir(string directory, List<string> blacklist)
        {
            if (!Directory.Exists(Path.Combine(App.BeatSaberInstallDirectory, directory)))
            {
                return;
            }

            foreach (string file in Directory.GetFileSystemEntries(Path.Combine(App.BeatSaberInstallDirectory, directory)))
            {
                if (File.Exists(file) && Path.GetExtension(file) == ".dll" || Path.GetExtension(file) == ".manifest")
                {
                    Mod mod = GetModFromHash(Utils.CalculateMD5(file));
                    if (mod != null)
                    {
                        AddDetectedMod(mod);
                    }
                    else
                    {
                        /*Console.WriteLine("3rd party plugins: " + file + ": " + Utils.CalculateMD5(file));*/
                    }
                }
            }
        }

        public void GetBSIPAVersion()
        {
            string InjectorPath = Path.Combine(App.BeatSaberInstallDirectory, "Beat Saber_Data", "Managed", "IPA.Injector.dll");
            if (!File.Exists(InjectorPath)) return;

            string InjectorHash = Utils.CalculateMD5(InjectorPath);
            foreach (Mod mod in AllModsList)
            {
                if (mod.name.ToLower() == "bsipa" || mod.name.ToLower() == "base-netvios" || mod.name.ToLower() == "bsipa-netviosspecial")
                {
                    foreach (Mod.DownloadLink download in mod.downloads)
                    {
                        foreach (Mod.FileHashes fileHash in download.hashMd5)
                        {
                            if (fileHash.hash == InjectorHash)
                            {
                                AddDetectedMod(mod);
                                lastBSIPA = mod.name;
                                DefaultModsNetvios.Clear();
                                DefaultModsNetvios.Add(mod.name);
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
                /*Console.WriteLine($"Add {mod.name}-{mod.version}({mod._id})");*/
                InstalledMods.Add(mod);
                if (App.SelectInstalledMods && !DefaultMods.Contains(mod.name))
                {
                    DefaultMods.Add(mod.name);
                }
            }
            else {
                /*Console.WriteLine($"Abandon {mod.name}-{mod.version}({mod._id})");*/
            }
        }

        private Mod GetModFromHash(string hash)
        {
            foreach (Mod mod in AllModsList)
            {
                if ((mod.name.ToLower() != "bsipa" && mod.name.ToLower() != "base-netvios" && mod.name.ToLower() != "bsipa-netviosspecial") && mod.status != "declined")
                {
                    foreach (Mod.DownloadLink download in mod.downloads)
                    {
                        foreach (Mod.FileHashes fileHash in download.hashMd5)
                        {
                            if (fileHash.hash == hash) {
                                /*Console.WriteLine($"File Hash: {hash}, find {mod.name}-{mod.version}({mod._id}) has hash {fileHash.hash}({fileHash.file})");*/
                                return mod;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public async Task PopulateModsList()
        {
            try
            {
                var resp = await HttpClient.GetAsync((Properties.Settings.Default.StoreType == "Netvios" && ModAssistant.Properties.Settings.Default.DownloadServer == "网易版@BeatMods.top") ? (Utils.Constants.BeatModsAPIUrl + Utils.Constants.BeatModsModsOptions + "&gameVersion=" + MainWindow.GameVersion + "&withNetvios=true") : (Utils.Constants.BeatModsAPIUrl + Utils.Constants.BeatModsModsOptions + "&gameVersion=" + MainWindow.GameVersion));
                var body = await resp.Content.ReadAsStringAsync();
                ModsList = JsonSerializer.Deserialize<Mod[]>(body);

                foreach (Mod mod in ModsList)
                {
                    // duplicate detection
                    var versions = mod.version.Split('.');
                    if (versions.Length > 3)
                    {
                        mod.trueGameVersion = mod.version;
                        mod.version = versions[versions.Length - 3] + "." + versions[versions.Length - 2] + "." + versions[versions.Length - 1];
                    }
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show($"{FindResource("Mods:LoadFailed")}.\n\n" + e);
                return;
            }

            foreach (Mod mod in ModsList)
            {
                if (!(mod.translations is null || mod.translations.Length == 0))
                {
                    foreach (Translation singleTranslation in mod.translations)
                    {
                        if (singleTranslation.language == Properties.Settings.Default.LanguageCode)
                        {
                            mod.nameWithTranslation = (singleTranslation.name == "") ? singleTranslation.name : (singleTranslation.name == "!NOTRANSLATION!") ? mod.name : singleTranslation.name + " (" + mod.name + ")";
                            mod.descriptionWithTranslation = singleTranslation.description;
                        }
                    }
                }
                else if (Properties.Settings.Default.LanguageCode == "zh")
                {
                    foreach (TranslationWGzeyu singleTranslationWGzeyu in ModsTranslationWGzeyu)
                    {
                        if (mod.name.Equals(singleTranslationWGzeyu.name))
                        {
                            mod.nameWithTranslation = singleTranslationWGzeyu.newname;
                            mod.descriptionWithTranslation = singleTranslationWGzeyu.newdescription;
                        }
                    }
                }

                bool preSelected = mod.required;

                if (Properties.Settings.Default.StoreType == "Netvios" && Properties.Settings.Default.DownloadServer == "网易版@BeatMods.top") {
                    /*mod.required = false;*/
                    preSelected = false;
                }

                if (((Properties.Settings.Default.StoreType == "Netvios" && Properties.Settings.Default.DownloadServer == "网易版@BeatMods.top") ? DefaultModsNetvios.Contains(mod.name) : DefaultMods.Contains(mod.name)) || (App.SaveModSelection && App.SavedMods.Contains(mod.name)))
                {
                    preSelected = true;
                    if (!App.SavedMods.Contains(mod.name))
                    {
                        App.SavedMods.Add(mod.name);
                    }
                }

                RegisterDependencies(mod);

                ModListItem ListItem = new ModListItem()
                {
                    IsSelected = (Properties.Settings.Default.StoreType == "Netvios" && Properties.Settings.Default.DownloadServer == "网易版@BeatMods.top" && NetviosNotCompatiblePluginList.Contains(mod.name)) ? false : preSelected,
                    IsEnabled = (Properties.Settings.Default.StoreType == "Netvios" && Properties.Settings.Default.DownloadServer == "网易版@BeatMods.top" && NetviosNotCompatiblePluginList.Contains(mod.name)) ? false : !mod.required,
                    ModName = (mod.nameWithTranslation is null || mod.nameWithTranslation == "") ? mod.name : mod.nameWithTranslation,
                    ModVersion = mod.version,
                    ModDescription = ((mod.descriptionWithTranslation is null || mod.descriptionWithTranslation == "") ? mod.description : mod.descriptionWithTranslation).Replace("\r\n", " ").Replace("\n", " "),
                    ModInfo = mod,
                    Category = getCategoryTranslation(mod.category)
                };

                foreach (Promotion promo in Promotions.ActivePromotions)
                {
                    if (mod.name == promo.ModName)
                    {
                        ListItem.PromotionText = promo.Text;
                        ListItem.PromotionLink = promo.Link;
                    }
                }

                string maxVersion = "";
                foreach (Mod installedMod in InstalledMods)
                {
                    if (mod.name == installedMod.name && (maxVersion == "" || SemVersion.Compare(mod.version, maxVersion) == 1))
                    {
                        maxVersion = mod.version;
                        ListItem.InstalledModInfo = installedMod;
                        ListItem.IsInstalled = true;
                        ListItem.InstalledVersion = maxVersion;
                        if (Properties.Settings.Default.StoreType != "Netvios" || Properties.Settings.Default.DownloadServer != "网易版@BeatMods.top") {
                            break;
                        }
                    }
                }

                mod.ListItem = ListItem;

                ModList.Add(ListItem);
            }

            foreach (Mod mod in ModsList)
            {
                ResolveDependencies(mod);
            }
        }

        public async void InstallMods()
        {
            MainWindow.Instance.InstallButton.IsEnabled = false;
            string installDirectory = App.BeatSaberInstallDirectory;

            foreach (Mod mod in ModsList)
            {
                // Ignore mods that are newer than installed version
                if (mod.ListItem.GetVersionComparison > 0) continue;

                // Ignore mods that are on current version if we aren't reinstalling mods
                if (mod.ListItem.GetVersionComparison == 0 && !App.ReinstallInstalledMods) continue;

                if (mod.name.ToLower() == "bsipa" || mod.name.ToLower() == "bsipa-netvios" || mod.name.ToLower() == "bsipa-netviosspecial")
                {
                    if (mod.name.ToLower() == "bsipa" && Properties.Settings.Default.StoreType == "Netvios" && Properties.Settings.Default.DownloadServer == "网易版@BeatMods.top") continue;
                    MainWindow.Instance.MainText = $"{string.Format((string)FindResource("Mods:InstallingMod"), mod.name)} {(Properties.Settings.Default.LanguageCode == "zh" ? "(从" : "from ")} {Properties.Settings.Default.DownloadServer} {(Properties.Settings.Default.LanguageCode == "zh" ? ")" : "...")}";
                    await Task.Run(async () => await InstallMod(mod, installDirectory));
                    MainWindow.Instance.MainText = $"{string.Format((string)FindResource("Mods:InstalledMod"), mod.name)}{(Properties.Settings.Default.LanguageCode == "zh" ? "。" : ".")}";
                    if (!File.Exists(Path.Combine(installDirectory, "winhttp.dll")) || mod.name != lastBSIPA)
                    {
                        await Task.Run(() =>
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = Path.Combine(installDirectory, "IPA.exe"),
                                WorkingDirectory = installDirectory,
                                Arguments = "-n"
                            }).WaitForExit()
                        );
                        lastBSIPA = mod.name;
                    }

                    Pages.Options.Instance.YeetBSIPA.IsEnabled = true;
                }
                else if (mod.ListItem.IsSelected)
                {
                    MainWindow.Instance.MainText = $"{string.Format((string)FindResource("Mods:InstallingMod"), mod.name)} {(Properties.Settings.Default.LanguageCode == "zh" ? "(从" : "from ")} {Properties.Settings.Default.DownloadServer} {(Properties.Settings.Default.LanguageCode == "zh" ? ")" : "...")}";
                    await Task.Run(async () => await InstallMod(mod, Path.Combine(installDirectory, @"IPA\Pending")));
                    MainWindow.Instance.MainText = $"{string.Format((string)FindResource("Mods:InstalledMod"), mod.name)}{(Properties.Settings.Default.LanguageCode == "zh" ? "。" : ".")}";
                }
            }

            MainWindow.Instance.MainText = $"{FindResource("Mods:FinishedInstallingMods")}{(Properties.Settings.Default.LanguageCode == "zh" ? "。" : ".")}";
            MainWindow.Instance.InstallButton.IsEnabled = true;
            RefreshModsList();
        }

        private async Task InstallMod(Mod mod, string directory)
        {
            string downloadLink = null;

            foreach (Mod.DownloadLink link in mod.downloads)
            {
                if (link.type == "universal")
                {
                    downloadLink = link.url;
                    break;
                }
                else if (link.type.ToLower() == App.BeatSaberInstallType.ToLower())
                {
                    downloadLink = link.url;
                    break;
                }
            }

            if (string.IsNullOrEmpty(downloadLink))
            {
                System.Windows.MessageBox.Show(string.Format((string)FindResource("Mods:ModDownloadLinkMissing"), mod.name));
                return;
            }

            MainWindow.Instance.MainText = $"{string.Format((string)FindResource("Mods:DownloadingMod"), mod.name,Properties.Settings.Default.DownloadServer)}";

            using (Stream stream = await DownloadMod(Utils.Constants.BeatModsURL + downloadLink))
            using (ZipArchive archive = new ZipArchive(stream))
            {
                MainWindow.Instance.MainText = $"{string.Format((string)FindResource("Mods:InstallingMod"), mod.name)}...";
                foreach (ZipArchiveEntry file in archive.Entries)
                {
                    string fileDirectory = Path.GetDirectoryName(Path.Combine(directory, file.FullName));
                    if (!Directory.Exists(fileDirectory))
                    {
                        Directory.CreateDirectory(fileDirectory);
                    }

                    if (!string.IsNullOrEmpty(file.Name))
                    {
                        await ExtractFile(file, Path.Combine(directory, file.FullName), 3.0, mod.name, 10);
                    }
                }
            }

            if (App.CheckInstalledMods)
            {
                mod.ListItem.IsInstalled = true;
                mod.ListItem.InstalledVersion = mod.version;
                mod.ListItem.InstalledModInfo = mod;
            }
        }

        private async Task ExtractFile(ZipArchiveEntry file, string path, double seconds, string name, int maxTries, int tryNumber = 0)
        {
            if (tryNumber < maxTries)
            {
                try
                {
                    file.ExtractToFile(path, true);
                }
                catch
                {
                    MainWindow.Instance.MainText = $"{string.Format((string)FindResource("Mods:FailedExtract"), name, seconds, tryNumber + 1, maxTries)}";
                    await Task.Delay((int)(seconds * 1000));
                    await ExtractFile(file, path, seconds, name, maxTries, tryNumber + 1);
                }
            }
            else
            {
                System.Windows.MessageBox.Show($"{string.Format((string)FindResource("Mods:FailedExtractMaxReached"), name, maxTries)}.", "Failed to install " + name);
            }
        }

        private async Task<Stream> DownloadMod(string link)
        {
            var resp = await HttpClient.GetAsync(link);
            return await resp.Content.ReadAsStreamAsync();
        }

        private void RegisterDependencies(Mod dependent)
        {
            if (dependent.dependencies.Length == 0)
                return;

            foreach (Mod mod in ModsList)
            {
                foreach (Mod.Dependency dep in dependent.dependencies)
                {

                    if (dep.name == mod.name)
                    {
                        dep.Mod = mod;
                        mod.Dependents.Add(dependent);

                    }
                }
            }
        }

        private void ResolveDependencies(Mod dependent)
        {
            if (dependent.ListItem.IsSelected && Properties.Settings.Default.StoreType == "Netvios" && Properties.Settings.Default.DownloadServer == "网易版@BeatMods.top") {
                if (!dependent.name.Contains("Netvios") || dependent.name == "BSIPA-NetviosSpecial")
                {
                    BSIPAVersion = "BSIPA-NetviosSpecial";
                    BSIPAVersionUncheck = "Base-Netvios";
                }
                else if (dependent.name == "Base-Netvios")
                {
                    BSIPAVersion = "Base-Netvios";
                    BSIPAVersionUncheck = "BSIPA-NetviosSpecial";
                }

                foreach (ModListItem modItem in ModList)
                {
                    if (modItem.ModInfo.name == BSIPAVersion)
                    {
                        modItem.IsEnabled = false;
                        modItem.IsSelected = true;
                    }
                    else if (modItem.ModInfo.name == BSIPAVersionUncheck)
                    {
                        modItem.IsEnabled = true;
                        modItem.IsSelected = false;
                    } else if (modItem.ModInfo.name.ToLower() == "BSIPA".ToLower()) {
                        modItem.IsEnabled = false;
                        modItem.IsSelected = false;
                    }
                }
            }

            if (dependent.ListItem.IsSelected && dependent.dependencies.Length > 0)
            {
                foreach (Mod.Dependency dependency in dependent.dependencies)
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
            if (!dependent.ListItem.IsSelected && dependent.dependencies.Length > 0)
            {
                foreach (Mod.Dependency dependency in dependent.dependencies)
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
                        if (!needed && !dependency.Mod.required)
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
            App.SavedMods.Add(mod.name);
            Properties.Settings.Default.SavedMods = string.Join(",", App.SavedMods.ToArray());
            Properties.Settings.Default.Save();
            RefreshModsList();
        }

        private void ModCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Mod mod = ((sender as System.Windows.Controls.CheckBox).Tag as Mod);
            mod.ListItem.IsSelected = false;
            UnresolveDependencies(mod);
            App.SavedMods.Remove(mod.name);
            Properties.Settings.Default.SavedMods = string.Join(",", App.SavedMods.ToArray());
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
            private SemVersion _installedVersion { get; set; }
            public string InstalledVersion
            {
                get
                {
                    if (!IsInstalled || _installedVersion == null) return "-";
                    return _installedVersion.ToString();
                }
                set
                {
                    if (SemVersion.TryParse(value, out SemVersion tempInstalledVersion))
                    {
                        _installedVersion = tempInstalledVersion;
                    } else
                    {
                        _installedVersion = null;
                    }
                }
            }

            public string GetVersionColor
            {
                get
                {
                    if (!IsInstalled || _installedVersion is null) return "Black";
                    return _installedVersion >= ModVersion ? "Green" : "Red";
                }
            }

            public string GetVersionDecoration
            {
                get
                {
                    if (!IsInstalled || _installedVersion is null) return "None";
                    return _installedVersion >= ModVersion ? "None" : "Strikethrough";
                }
            }

            public int GetVersionComparison
            {
                get
                {
                    if (!IsInstalled || _installedVersion is null || _installedVersion < ModVersion) return -1;
                    if (_installedVersion > ModVersion) return 1;
                    return 0;
                }
            }

            public bool CanDelete
            {
                get
                {
                    return (!ModInfo.required && IsInstalled);
                }
            }

            public string CanSeeDelete
            {
                get
                {
                    if (!ModInfo.required && IsInstalled)
                        return "Visible";
                    else
                        return "Hidden";
                }
            }

            public string PromotionText { get; set; }
            public string PromotionLink { get; set; }
            public string PromotionMargin
            {
                get
                {
                    if (string.IsNullOrEmpty(PromotionText)) return "0";
                    return "0,0,5,0";
                }
            }
        }

        private void ModsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((Mods.ModListItem)Mods.Instance.ModsListView.SelectedItem == null)
            {
                MainWindow.Instance.InfoButton.IsEnabled = false;
            }
            else
            {
                MainWindow.Instance.InfoButton.IsEnabled = true;
            }
        }

        public void UninstallBSIPA(Mod.DownloadLink links)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(App.BeatSaberInstallDirectory, "IPA.exe"),
                WorkingDirectory = App.BeatSaberInstallDirectory,
                Arguments = "--revert -n"
            }).WaitForExit();

            foreach (Mod.FileHashes files in links.hashMd5)
            {
                string file = files.file.Replace("IPA/", "").Replace("Data", "Beat Saber_Data");
                if (File.Exists(Path.Combine(App.BeatSaberInstallDirectory, file)))
                    File.Delete(Path.Combine(App.BeatSaberInstallDirectory, file));
            }
            Pages.Options.Instance.YeetBSIPA.IsEnabled = false;
        }

        private void Uninstall_Click(object sender, RoutedEventArgs e)
        {
            Mod mod = ((sender as System.Windows.Controls.Button).Tag as Mod);

            string title = string.Format((string)FindResource("Mods:UninstallBox:Title"), mod.name);
            string body1 = string.Format((string)FindResource("Mods:UninstallBox:Body1"), mod.name);
            string body2 = string.Format((string)FindResource("Mods:UninstallBox:Body2"), mod.name);
            var result = System.Windows.Forms.MessageBox.Show($"{body1}\n{body2}", title, MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                UninstallModFromList(mod);
            }
        }

        private void UninstallModFromList(Mod mod)
        {
            UninstallMod(mod.ListItem.InstalledModInfo);
            mod.ListItem.IsInstalled = false;
            mod.ListItem.InstalledVersion = null;
            if (App.SelectInstalledMods)
            {
                mod.ListItem.IsSelected = false;
                UnresolveDependencies(mod);
                App.SavedMods.Remove(mod.name);
                Properties.Settings.Default.SavedMods = string.Join(",", App.SavedMods.ToArray());
                Properties.Settings.Default.Save();
                RefreshModsList();
            }
            view.Refresh();
        }

        public void UninstallMod(Mod mod)
        {
            Mod.DownloadLink links = null;
            foreach (Mod.DownloadLink link in mod.downloads)
            {
                if (link.type.ToLower() == "universal" || link.type.ToLower() == App.BeatSaberInstallType.ToLower())
                {
                    links = link;
                    break;
                }
            }
            if (mod.name.ToLower() == "bsipa" || mod.name.ToLower() == "base-netvios" || mod.name.ToLower() == "bsipa-netviosspecial")
                UninstallBSIPA(links);
            foreach (Mod.FileHashes files in links.hashMd5)
            {
                if (File.Exists(Path.Combine(App.BeatSaberInstallDirectory, files.file)))
                    File.Delete(Path.Combine(App.BeatSaberInstallDirectory, files.file));
                if (File.Exists(Path.Combine(App.BeatSaberInstallDirectory, "IPA", "Pending", files.file)))
                    File.Delete(Path.Combine(App.BeatSaberInstallDirectory, "IPA", "Pending", files.file));
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshColumns();
        }

        private void CopyText(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var textBlock = sender as TextBlock;
            if (textBlock == null) { return; }
            var text = textBlock.Text;

            // Ensure there's text to be copied
            if (string.IsNullOrWhiteSpace(text)) { return; }

            Utils.SetClipboard(text);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (SearchBar.Height == 0)
            {
                SearchBar.Focus();
                Animate(SearchBar, 0, 16, new TimeSpan(0, 0, 0, 0, 300));
                Animate(SearchText, 0, 16, new TimeSpan(0, 0, 0, 0, 300));
                ModsListView.Items.Filter = new Predicate<object>(SearchFilter);
            }
            else
            {
                Animate(SearchBar, 16, 0, new TimeSpan(0, 0, 0, 0, 300));
                Animate(SearchText, 16, 0, new TimeSpan(0, 0, 0, 0, 300));
                ModsListView.Items.Filter = null;
            }
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            ModsListView.Items.Filter = new Predicate<object>(SearchFilter);
            if (SearchBar.Text.Length > 0)
            {
                SearchText.Text = null;
            }
            else
            {
                SearchText.Text = (string)FindResource("Mods:SearchLabel");
            }
        }

        private bool SearchFilter(object mod)
        {
            ModListItem item = mod as ModListItem;
            if (item.ModName.ToLower().Contains(SearchBar.Text.ToLower())) return true;
            if (item.ModDescription.ToLower().Contains(SearchBar.Text.ToLower())) return true;
            if (item.ModName.ToLower().Replace(" ", string.Empty).Contains(SearchBar.Text.ToLower().Replace(" ", string.Empty))) return true;
            if (item.ModDescription.ToLower().Replace(" ", string.Empty).Contains(SearchBar.Text.ToLower().Replace(" ", string.Empty))) return true;
            return false;
        }

        private void Animate(TextBlock target, double oldHeight, double newHeight, TimeSpan duration)
        {
            target.Height = oldHeight;
            DoubleAnimation animation = new DoubleAnimation(newHeight, duration);
            target.BeginAnimation(TextBlock.HeightProperty, animation);
        }

        private void Animate(TextBox target, double oldHeight, double newHeight, TimeSpan duration)
        {
            target.Height = oldHeight;
            DoubleAnimation animation = new DoubleAnimation(newHeight, duration);
            target.BeginAnimation(TextBox.HeightProperty, animation);
        }

        private void CategoryTranslationInit()
        {
            if (Properties.Settings.Default.LanguageCode == "zh")
            {
                CategoryTranslation.Add("Core", "核心");
                CategoryTranslation.Add("Cosmetic", "美化");
                CategoryTranslation.Add("Gameplay", "游戏性");
                CategoryTranslation.Add("Libraries", "支持库");
                CategoryTranslation.Add("Lighting", "灯光");
                CategoryTranslation.Add("Netvios", "影核版专用");
                CategoryTranslation.Add("Other", "其它");
                CategoryTranslation.Add("Practice / Training", "练习 / 训练");
                CategoryTranslation.Add("Stream Tools", "直播工具");
                CategoryTranslation.Add("Text Changes", "自定义文字");
                CategoryTranslation.Add("Tweaks / Tools", "调整 / 工具");
                CategoryTranslation.Add("UI Enhancements", "UI增强");
                CategoryTranslation.Add("Uncategorized", "未分类");
            }
        }

        private string getCategoryTranslation(string name)
        {
            if (CategoryTranslation.ContainsKey(name))
            {
                return CategoryTranslation[name];
            }
            else
            {
                return name;
            }
        }
    }
}
