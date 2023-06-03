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
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using ModAssistant.Libs;
using static ModAssistant.Http;
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

        public List<string> DefaultMods = new List<string>() { "SongCore", "ScoreSaber", "BeatSaverDownloader", "BeatSaverVoting", "PlaylistManager", "ModelDownloader", "MappingExtensions", "BetterSongList", "BetterSongSearch", "BSIPA" , "Noodle Extensions" , "NoodleExtensions" , "Chroma" , "SiraUtil" , "SiraLocalizer" , "BeatLeader" , "Beat Leader" };
        public Mod[] ModsList;
        public Mod[] AllModsList;
        public TranslationWGzeyu[] ModsTranslationWGzeyu;
        public static List<Mod> InstalledMods = new List<Mod>();
        public static List<Mod> ManifestsToMatch = new List<Mod>();
        public List<string> CategoryNames = new List<string>();
        public CollectionView view;
        public bool PendingChanges;
        public Dictionary<String, String> CategoryTranslation = new Dictionary<string, string>();
        public Dictionary<string, SemVersion> inListMods = new Dictionary<string, SemVersion>();
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
            if (MainWindow.Instance.Main.Content != Instance) return;
            double viewWidth = ModsListView.ActualWidth;
            double totalSize = 0;
            GridViewColumn description = null;

            if (ModsListView.View is GridView grid)
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

                MainWindow.Instance.MainText = $"{FindResource("Mods:LoadingMods")}{(Properties.Settings.Default.LanguageCode == "zh" ? "（从" + Properties.Settings.Default.DownloadServer + "）" : " from " + Properties.Settings.Default.DownloadServer + "...")}";
                await Task.Run(async () => await PopulateModsList());

                ModsListView.ItemsSource = ModList;

                try
                {
                    var manualCategories = new string[] { "Core", "Leaderboards" };

                    ModList.Sort((a, b) =>
                    {
                        foreach (var category in manualCategories)
                        {
                            if (a.Category == category && b.Category == category) return 0;
                            if (a.Category == category) return -1;
                            if (b.Category == category) return 1;
                        }

                        var categoryCompare = a.Category.CompareTo(b.Category);
                        if (categoryCompare != 0) return categoryCompare;

                        var aRequired = !a.IsEnabled;
                        var bRequired = !b.IsEnabled;

                        if (a.ModRequired && !b.ModRequired) return -1;
                        if (b.ModRequired && !a.ModRequired) return 1;

                        return a.ModName.CompareTo(b.ModName);
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                view = (CollectionView)CollectionViewSource.GetDefaultView(ModsListView.ItemsSource);
                PropertyGroupDescription groupDescription = new PropertyGroupDescription("Category");
                view.GroupDescriptions.Add(groupDescription);

                this.DataContext = this;

                RefreshModsList();
                ModsListView.Visibility = ModList.Count == 0 ? Visibility.Hidden : Visibility.Visible;
                NoModsGrid.Visibility = ModList.Count == 0 ? Visibility.Visible : Visibility.Hidden;

                MainWindow.Instance.MainText = $"{FindResource("Mods:FinishedLoadingMods")}{(Properties.Settings.Default.LanguageCode == "zh" ? "。" : ".")}";
                if (Properties.Settings.Default.LanguageCode == "zh" && Properties.Settings.Default.DownloadServer != Server.BeatModsTop)
                {
                    MainWindow.Instance.MainText = MainWindow.Instance.MainText + "（翻译来自@WGzeyu）";
                }

                MainWindow.Instance.InstallButton.IsEnabled = ModList.Count != 0;
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

            GetBSIPAVersion();
            CheckInstallDir("IPA/Pending/Plugins");
            CheckInstallDir("IPA/Pending/Libs");
            CheckInstallDir("Plugins");
            CheckInstallDir("Libs");
        }

        public async Task GetAllMods()
        {
            var resp = await HttpClient.GetAsync(Utils.Constants.BeatModsAPIUrl + "mod");
            var body = await resp.Content.ReadAsStringAsync();

            try
            {
                AllModsList = JsonSerializer.Deserialize<Mod[]>(body);
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show($"{FindResource("Mods:LoadFailed")}.\n\n" + e);
                AllModsList = new Mod[] { };
            }
        }

        private void CheckInstallDir(string directory)
        {
            if (!Directory.Exists(Path.Combine(App.BeatSaberInstallDirectory, directory)))
            {
                return;
            }

            foreach (string file in Directory.GetFileSystemEntries(Path.Combine(App.BeatSaberInstallDirectory, directory)))
            {
                string fileExtension = Path.GetExtension(file);

                if (File.Exists(file) && (fileExtension == ".dll" || fileExtension == ".exe" || fileExtension == ".manifest"))
                {
                    Mod mod = GetModFromHash(Utils.CalculateMD5(file));
                    if (mod != null)
                    {
                        if (fileExtension == ".manifest")
                        {
                            ManifestsToMatch.Add(mod);
                        }
                        else
                        {
                            if (directory.Contains("Libs"))
                            {
                                if (!ManifestsToMatch.Contains(mod))
                                {
                                    continue;
                                }

                                ManifestsToMatch.Remove(mod);
                            }

                            AddDetectedMod(mod);
                        }
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
                if (mod.name.ToLowerInvariant() == "bsipa")
                {
                    foreach (Mod.DownloadLink download in mod.downloads)
                    {
                        foreach (Mod.FileHashes fileHash in download.hashMd5)
                        {
                            if (fileHash.hash == InjectorHash)
                            {
                                AddDetectedMod(mod);
                                lastBSIPA = mod.name;
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
                if (mod.name.ToLowerInvariant() != "bsipa" && mod.status != "declined")
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
                Console.WriteLine(Utils.Constants.BeatModsAPIUrl + Utils.Constants.BeatModsModsOptions + "&gameVersion=" + MainWindow.GameVersion);
                var resp = await HttpClient.GetAsync(Utils.Constants.BeatModsAPIUrl + Utils.Constants.BeatModsModsOptions + "&gameVersion=" + MainWindow.GameVersion);
                var body = await resp.Content.ReadAsStringAsync();
                ModsList = JsonSerializer.Deserialize<Mod[]>(body);

                if (new Version(MainWindow.GameVersion) >= new Version("1.16.3") && (Environment.OSVersion.Version.Major < 6 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1)))
                {
                    string notice = "";
                    string caption = "";
                    switch (Properties.Settings.Default.LanguageCode)
                    {
                        case "zh":
                            caption = "提示";
                            notice = "您的系统版本过低，可能会有部分Mod不兼容，导致出现黑屏等问题，建议升级至Windows10再安装Mod。";
                            break;
                        default:
                            caption = "Notice";
                            notice = "Your Windows version is old. Some Mods may be incompatible with BeatSaber, a possible issue like black screen. ModAssistant recommends upgrading your OS to Windows 10.";
                            break;
                    }
                    System.Windows.Forms.MessageBox.Show(notice, caption);
                }

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

            bool ExceptionShown = false;
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
                else if (Properties.Settings.Default.LanguageCode == "zh" && Properties.Settings.Default.DownloadServer != Server.BeatModsTop)
                {
                    try
                    {
                        if (ModsTranslationWGzeyu is null) {
                            MainWindow.Instance.MainText = $"{(Properties.Settings.Default.LanguageCode == "zh" ? "正在获取Mod翻译（翻译来自@WGzeyu）" : "Fetching additional translation form WGzuyu.")}";
                            var resp_WGzeyu = await HttpClient.GetAsync(Utils.Constants.BeatModsTranslation);
                            var body_WGzeyu = await resp_WGzeyu.Content.ReadAsStringAsync();
                            ModsTranslationWGzeyu = JsonSerializer.Deserialize<TranslationWGzeyu[]>(body_WGzeyu);
                            Console.WriteLine("Finished");
                        }

                        foreach (TranslationWGzeyu singleTranslationWGzeyu in ModsTranslationWGzeyu)
                        {
                            if (mod.name.Equals(singleTranslationWGzeyu.name))
                            {
                                mod.nameWithTranslation = singleTranslationWGzeyu.newname;
                                if (mod.description.Equals(singleTranslationWGzeyu.description))
                                {
                                    mod.descriptionWithTranslation = singleTranslationWGzeyu.newdescription;
                                }
                                else
                                {
                                    mod.descriptionWithTranslation = mod.description + singleTranslationWGzeyu.newdescription;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (!ExceptionShown)
                        {
                           string caption = "";
                           string notice = "";
                           switch (Properties.Settings.Default.LanguageCode)
                           {
                               case "zh":
                                    caption = "翻译加载错误";
                                    notice = "如有疑问可以报告给开发者，该错误只影响翻译载入，并不影响其余功能！";
                                    break;
                               default:
                                   caption = "Fetching Translation Exception";
                                   notice = "Please report to developer if you want. The Exception only effect translation not other functions!";
                                   break;
                           }
                           System.Windows.MessageBox.Show($"{notice}\n{e}", caption);
                           ExceptionShown = true;
                        }
                    }
                }

                bool preSelected = mod.required;

                if ((DefaultMods.Contains(mod.name)) || (App.SaveModSelection && App.SavedMods.Contains(mod.name)))
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
                    IsSelected = preSelected,
                    IsEnabled = !mod.required,
                    ModName = (mod.nameWithTranslation is null || mod.nameWithTranslation == "") ? mod.name : mod.nameWithTranslation,
                    ModVersion = mod.version,
                    ModDescription = ((mod.descriptionWithTranslation is null || mod.descriptionWithTranslation == "") ? mod.description : mod.descriptionWithTranslation).Replace("\r\n", " ").Replace("\n", " "),
                    ModRequired = mod.required,
                    ModInfo = mod,
                    Category = getCategoryTranslation(mod.category)
                };

                foreach (Promotion promo in Promotions.List)
                {
                    if (promo.Active && mod.name == promo.ModName)
                    {
                        ListItem.PromotionTexts = new string[promo.Links.Count];
                        ListItem.PromotionLinks = new string[promo.Links.Count];
                        ListItem.PromotionTextAfterLinks = new string[promo.Links.Count];

                        for (int i = 0; i < promo.Links.Count; ++i)
                        {
                            PromotionLink link = promo.Links[i];
                            ListItem.PromotionTexts[i] = link.Text;
                            ListItem.PromotionLinks[i] = link.Link;
                            ListItem.PromotionTextAfterLinks[i] = link.TextAfterLink;
                        }
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
                        //ListItem.InstalledVersion = maxVersion;
                        ListItem.InstalledVersion = installedMod.version;
                        break;
                    }
                }

                mod.ListItem = ListItem;

                ModList.Add(ListItem);
            }

            foreach (Mod mod in ModsList)
            {
                // Console.WriteLine(mod.name + "(" + mod._id + ")");
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

                if (mod.name.ToLowerInvariant() == "bsipa")
                {
                    MainWindow.Instance.MainText = $"{string.Format((string)FindResource("Mods:InstallingMod"), mod.name)}...";
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

                    Options.Instance.YeetBSIPA.IsEnabled = true;
                }
                else if (mod.ListItem.IsSelected)
                {
                    MainWindow.Instance.MainText = $"{string.Format((string)FindResource("Mods:InstallingMod"), mod.name)} {(Properties.Settings.Default.LanguageCode == "zh" ? "（从" : " from ")} {Properties.Settings.Default.DownloadServer} {(Properties.Settings.Default.LanguageCode == "zh" ? "��" : "...")}";
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
            int filesCount = 0;
            string downloadLink = null;

            foreach (Mod.DownloadLink link in mod.downloads)
            {
                filesCount = link.hashMd5.Length;

                if (link.type == "universal")
                {
                    downloadLink = link.url;
                    break;
                }
                else if (link.type.ToLowerInvariant() == App.BeatSaberInstallType.ToLowerInvariant())
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

            // Console.WriteLine(Utils.Constants.BeatModsURL + downloadLink);
			MainWindow.Instance.MainText = $"{string.Format((string)FindResource("Mods:DownloadingMod"), mod.name, Properties.Settings.Default.DownloadServer)}";

            while (true)
            {
                List<ZipArchiveEntry> files = new List<ZipArchiveEntry>(filesCount);
                
                using (Stream stream = await DownloadMod(Utils.Constants.BeatModsURL + downloadLink))
                using (ZipArchive archive = new ZipArchive(stream))
                {
                	MainWindow.Instance.MainText = $"{string.Format((string)FindResource("Mods:InstallingMod"), mod.name)}...";

                    try
                    {
                        foreach (ZipArchiveEntry file in archive.Entries)
                        {
                            string fileDirectory = Path.GetDirectoryName(Path.Combine(directory, file.FullName));
                            if (!Directory.Exists(fileDirectory))
                            {
                                Directory.CreateDirectory(fileDirectory);
                            }

                            if (!string.IsNullOrEmpty(file.Name))
                            {
                                foreach (Mod.DownloadLink download in mod.downloads)
                                {
                                    foreach (Mod.FileHashes fileHash in download.hashMd5)
                                    {
                                        using (Stream fileStream = file.Open())
                                        {
                                            if (fileHash.hash == Utils.CalculateMD5FromStream(fileStream))
                                            {
                                                files.Add(file);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (files.Count == filesCount)
                        {
                            foreach (ZipArchiveEntry file in files)
                            {
                                await ExtractFile(file, Path.Combine(directory, file.FullName), 3.0, mod.name, 10);
                            }

                            break;
                        }
                    }
                    catch (InvalidDataException e)
                    {
                        string msg = "";
                        string caption = "";
                        switch (Properties.Settings.Default.LanguageCode)
                        {
                            case "zh":
                                msg = "ZIP文件已损坏或下载失败";
                                caption = "安装失败";
                                break;
                            default:
                                msg = "ZIP file broken or file download failed!";
                                caption = "Install mod failed!";
                                break;
                        }
                        System.Windows.MessageBox.Show($"{msg}\n{e}", caption);
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
            try
            {
                var resp = await HttpClient.GetAsync(link);
                return await resp.Content.ReadAsStreamAsync();
            }
            catch (Exception e) {
                Console.WriteLine(e);
                var resp = await HttpClient.GetAsync(link);
                return await resp.Content.ReadAsStreamAsync();
            }
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
            if (dependent.ListItem.IsSelected) {
                foreach (ModListItem modItem in ModList)
                {
                    if (modItem.ModInfo.name.ToLowerInvariant() == "BSIPA".ToLowerInvariant()) {
                        modItem.IsEnabled = false;
                        modItem.IsSelected = false;
                    }
                }
            }

            if (dependent.ListItem.IsSelected && dependent.dependencies.Length > 0)
            {
                foreach (Mod.Dependency dependency in dependent.dependencies)
                {
                    // Console.WriteLine("\t" + dependency.name + "(" + dependency._id + ")");
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
            Mod mod = (sender as System.Windows.Controls.CheckBox).Tag as Mod;
            if (mod.name.ToLowerInvariant() == "catcore") {
                string notice = "";
                string caption = "";
                switch (Properties.Settings.Default.LanguageCode) {
                    case "zh":
                        caption = "提示";
                        notice = "您勾选安装CatCore(猫猫核心)!\n\n该mod与由baoziii维护的ChatCore(聊天核心)、EnhancedStreamChat-v3(增强直播聊天V3)以及SongRequestManager-v2(点歌管理器V2)互不兼容。\n\n如果您想使用ChatCore(聊天核心)所支持的Bilibili直播弹幕功能，请取消勾选该mod。";
                        break;
                    default:
                        caption = "Notice";
                        notice = "You selected CatCore! \n\nCatCore is conflicted with the following mods maintained by baoziii: ChatCore, EnhancedStreamChat-v3, and SongRequestManager-v2.\n\nIf you need to check Bilibili Live Danmuku, please uncheck this box.";
                        break;
                }

                System.Windows.Forms.MessageBox.Show(notice, caption);
            }
            mod.ListItem.IsSelected = true;
            // Console.WriteLine(mod.name + "(" + mod._id + ")");
            ResolveDependencies(mod);
            App.SavedMods.Add(mod.name);
            Properties.Settings.Default.SavedMods = string.Join(",", App.SavedMods.ToArray());
            Properties.Settings.Default.Save();

            RefreshModsList();
        }

        private void ModCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Mod mod = (sender as System.Windows.Controls.CheckBox).Tag as Mod;
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
            public bool ModRequired { get; set; }
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
                    }
                    else
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

            public string[] PromotionTexts { get; set; }
            public string[] PromotionLinks { get; set; }
            public string[] PromotionTextAfterLinks { get; set; }
            public string PromotionMargin
            {
                get
                {
                    if (PromotionTexts == null || string.IsNullOrEmpty(PromotionTexts[0])) return "-15,0,0,0";
                    return "0,0,5,0";
                }
            }
        }

        private void ModsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((Mods.ModListItem)Instance.ModsListView.SelectedItem == null)
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
            Options.Instance.YeetBSIPA.IsEnabled = false;
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
                if (link.type.ToLowerInvariant() == "universal" || link.type.ToLowerInvariant() == App.BeatSaberInstallType.ToLowerInvariant())
                {
                    links = link;
                    break;
                }
            }
            if (mod.name.ToLowerInvariant() == "bsipa")
            {
                var hasIPAExe = File.Exists(Path.Combine(App.BeatSaberInstallDirectory, "IPA.exe"));
                var hasIPADir = Directory.Exists(Path.Combine(App.BeatSaberInstallDirectory, "IPA"));

                if (hasIPADir && hasIPAExe)
                {
                    UninstallBSIPA(links);
                }
                else
                {
                    var title = (string)FindResource("Mods:UninstallBSIPANotFound:Title");
                    var body = (string)FindResource("Mods:UninstallBSIPANotFound:Body");

                    System.Windows.Forms.MessageBox.Show(body, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
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
            if (!(sender is TextBlock textBlock)) return;
            var text = textBlock.Text;

            // Ensure there's text to be copied
            if (string.IsNullOrWhiteSpace(text)) return;

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
            if (item.ModName.ToLowerInvariant().Contains(SearchBar.Text.ToLowerInvariant())) return true;
            if (item.ModDescription.ToLowerInvariant().Contains(SearchBar.Text.ToLowerInvariant())) return true;
            if (item.ModName.ToLowerInvariant().Replace(" ", string.Empty).Contains(SearchBar.Text.ToLowerInvariant().Replace(" ", string.Empty))) return true;
            if (item.ModDescription.ToLowerInvariant().Replace(" ", string.Empty).Contains(SearchBar.Text.ToLowerInvariant().Replace(" ", string.Empty))) return true;
            return false;
        }

        private void Animate(TextBlock target, double oldHeight, double newHeight, TimeSpan duration)
        {
            target.Height = oldHeight;
            DoubleAnimation animation = new DoubleAnimation(newHeight, duration);
            target.BeginAnimation(HeightProperty, animation);
        }

        private void Animate(TextBox target, double oldHeight, double newHeight, TimeSpan duration)
        {
            target.Height = oldHeight;
            DoubleAnimation animation = new DoubleAnimation(newHeight, duration);
            target.BeginAnimation(HeightProperty, animation);
        }

        private void CategoryTranslationInit()
        {
            if (CategoryTranslation.Count != 0) {
                CategoryTranslation.Clear();
            }
            switch (Properties.Settings.Default.LanguageCode) {
                case "zh":
                    CategoryTranslation.Add("core", "核心");
                    CategoryTranslation.Add("cosmetic", "美化");
                    CategoryTranslation.Add("for modders", "给Mod制作者");
                    CategoryTranslation.Add("gameplay", "游戏性");
                    CategoryTranslation.Add("libraries", "支持库");
                    CategoryTranslation.Add("lighting", "灯光");
                    CategoryTranslation.Add("multiplayer", "多人联机");
                    CategoryTranslation.Add("other", "其它");
                    CategoryTranslation.Add("practice / training", "练习 / 训练");
                    CategoryTranslation.Add("stream tools", "直播工具");
                    CategoryTranslation.Add("streaming tools", "直播工具");
                    CategoryTranslation.Add("text changes", "自定义文字");
                    CategoryTranslation.Add("tweaks / tools", "调整 / 工具");
                    CategoryTranslation.Add("ui enhancements", "UI增强");
                    CategoryTranslation.Add("uncategorized", "未分类");
                    break;
            }
        }

        private string getCategoryTranslation(string name)
        {
            if (CategoryTranslation.ContainsKey(name.ToLowerInvariant()))
            {
                return CategoryTranslation[name.ToLowerInvariant()];
            }
            else
            {
                return name;
            }
        }

        private string compareString(string str1, string str2) {
            Console.WriteLine("str1: " + str1 + " str2: " + str2);
            if (str1.Length != str2.Length)
            {
                return "Length not match: " + str1 + (str1.Length > str2.Length ? " > " : " < ") + str2;
            }
            else {
                for (int i = 0; i < str1.Length; i++) {
                    if (str1[i] != str2[i]) {
                        return "Index" + i + " not match, " + str1[i] + " vs " + str2[i];
                    }
                }
            }
            return "Match!";
        }
    }
}
