using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;

namespace ModAssistant.Pages
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Page
    {
        public static Options Instance = new Options();

        public string InstallDirectory { get; set; }
        public string InstallType { get; set; }
        public bool SaveSelection { get; set; }
        public bool CheckInstalledMods { get; set; }
        public bool SelectInstalledMods { get; set; }
        public bool ReinstallInstalledMods { get; set; }
        public bool ModelSaberProtocolHandlerEnabled { get; set; }
        public bool BeatSaverProtocolHandlerEnabled { get; set; }
        public bool PlaylistsProtocolHandlerEnabled { get; set; }
        public bool CloseWindowOnFinish { get; set; }
        public string LogURL { get; private set; }
        public string OCIWindow { get; set; }

        public Options()
        {
            InitializeComponent();

            OCIWindow = App.OCIWindow;
            if (!string.IsNullOrEmpty(OCIWindow))
            {
                UpdateOCIWindow(OCIWindow);
            }
            if (!CheckInstalledMods)
            {
                SelectInstalled.IsEnabled = false;
                ReinstallInstalled.IsEnabled = false;
            }

            UpdateHandlerStatus();
            this.DataContext = this;
        }

        public void UpdateHandlerStatus()
        {
            ModelSaberProtocolHandlerEnabled = OneClickInstaller.IsRegistered("modelsaber");
            BeatSaverProtocolHandlerEnabled = OneClickInstaller.IsRegistered("beatsaver");
            PlaylistsProtocolHandlerEnabled = OneClickInstaller.IsRegistered("bsplaylist");
        }

        private void SelectDirButton_Click(object sender, RoutedEventArgs e)
        {
            Utils.GetManualDir();
            DirectoryTextBlock.Text = InstallDirectory;
            GameTypeTextBlock.Text = InstallType;
        }

        private void OpenDirButton_Click(object sender, RoutedEventArgs e)
        {
            Utils.OpenFolder(InstallDirectory);
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(Utils.GetSteamDir());
        }

        private void SaveSelected_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SaveSelected = true;
            App.SaveModSelection = true;
            Properties.Settings.Default.Save();
        }

        private void SaveSelected_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SaveSelected = false;
            App.SaveModSelection = false;
            Properties.Settings.Default.Save();
        }

        private void CheckInstalled_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.CheckInstalled = true;
            App.CheckInstalledMods = true;
            CheckInstalledMods = true;
            Properties.Settings.Default.Save();
            SelectInstalled.IsEnabled = true;
            ReinstallInstalled.IsEnabled = true;

            if (MainWindow.ModsOpened)
            {
                Mods.Instance.PendingChanges = true;
            }
        }

        private void CheckInstalled_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.CheckInstalled = false;
            App.CheckInstalledMods = false;
            CheckInstalledMods = false;
            Properties.Settings.Default.Save();
            SelectInstalled.IsEnabled = false;
            ReinstallInstalled.IsEnabled = false;

            if (MainWindow.ModsOpened)
            {
                Mods.Instance.PendingChanges = true;
            }
        }

        private void CloseWindowOnFinish_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.CloseWindowOnFinish = true;
            App.CloseWindowOnFinish = true;
            CloseWindowOnFinish = true;
            Properties.Settings.Default.Save();
        }

        private void CloseWindowOnFinish_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.CloseWindowOnFinish = false;
            App.CloseWindowOnFinish = false;
            CloseWindowOnFinish = false;
            Properties.Settings.Default.Save();
        }

        public void ModelSaberProtocolHandler_Checked(object sender, RoutedEventArgs e)
        {
            OneClickInstaller.Register("modelsaber", Description: "URL:ModelSaber OneClick Install");
        }

        public void ModelSaberProtocolHandler_Unchecked(object sender, RoutedEventArgs e)
        {
            OneClickInstaller.Unregister("modelsaber");
        }

        public void BeatSaverProtocolHandler_Checked(object sender, RoutedEventArgs e)
        {
            OneClickInstaller.Register("beatsaver", Description: "URL:BeatSaver OneClick Install");
        }

        public void BeatSaverProtocolHandler_Unchecked(object sender, RoutedEventArgs e)
        {
            OneClickInstaller.Unregister("beatsaver");
        }
        public void PlaylistsProtocolHandler_Checked(object sender, RoutedEventArgs e)
        {
            OneClickInstaller.Register("bsplaylist", Description: "URL:BeatSaver Playlist OneClick Install");
        }

        public void PlaylistsProtocolHandler_Unchecked(object sender, RoutedEventArgs e)
        {
            OneClickInstaller.Unregister("bsplaylist");
        }

        private void SelectInstalled_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SelectInstalled = true;
            App.SelectInstalledMods = true;
            SelectInstalledMods = true;
            Properties.Settings.Default.Save();
        }

        private void SelectInstalled_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SelectInstalled = false;
            App.SelectInstalledMods = false;
            SelectInstalledMods = false;
            Properties.Settings.Default.Save();
        }

        private void ReinstallInstalled_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ReinstallInstalled = true;
            App.ReinstallInstalledMods = true;
            ReinstallInstalledMods = true;
            Properties.Settings.Default.Save();
        }

        private void ReinstallInstalled_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ReinstallInstalled = false;
            App.ReinstallInstalledMods = false;
            ReinstallInstalledMods = false;
            Properties.Settings.Default.Save();
        }

        private async void OpenLogsDirButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow.Instance.MainText = $"{Application.Current.FindResource("Options:UploadingLog")}...";
                await Task.Run(async () => await UploadLog());

                Process.Start(LogURL);
                Utils.SetClipboard(LogURL);
                MainWindow.Instance.MainText = (string)Application.Current.FindResource("Options:LogUrlCopied");
            }
            catch (Exception exception)
            {
                MainWindow.Instance.MainText = $"{Application.Current.FindResource("Options:LogUploadFailed")}.";

                string title = (string)Application.Current.FindResource("Options:LogUploadFailed:Title");
                string body = (string)Application.Current.FindResource("Options:LogUploadFailed:Body");
                MessageBox.Show($"{body}\n ================= \n" + exception, title);
                Utils.OpenFolder(Path.Combine(InstallDirectory, "Logs"));
            }
        }

        private async Task UploadLog()
        {
            const string DateFormat = "yyyy-mm-dd HH:mm:ss";
            DateTime now = DateTime.Now;
            string logPath = Path.GetDirectoryName(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath);
            string Log = Path.Combine(logPath, "log.log");
            string GameLog = File.ReadAllText(Path.Combine(InstallDirectory, "Logs", "_latest.log"));
            string Separator = File.Exists(Log) ? $"\n\n=============================================\n============= Mod Assistant Log =============\n=============================================\n\n" : string.Empty;
            string ModAssistantLog = File.Exists(Log) ? File.ReadAllText(Log) : string.Empty;

            var nvc = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("title", $"_latest.log ({now.ToString(DateFormat)})"),
                new KeyValuePair<string, string>("expireUnit", "hour"),
                new KeyValuePair<string, string>("expireLength", "5"),
                new KeyValuePair<string, string>("code", $"{GameLog}{Separator}{ModAssistantLog}"),
            };

            string[] items = new string[nvc.Count];

            for (int i = 0; i < nvc.Count; i++)
            {
                KeyValuePair<string, string> item = nvc[i];
                items[i] = WebUtility.UrlEncode(item.Key) + "=" + WebUtility.UrlEncode(item.Value);
            }

            StringContent content = new StringContent(string.Join("&", items), null, "application/x-www-form-urlencoded");
            HttpResponseMessage resp = await Http.HttpClient.PostAsync(Utils.Constants.TeknikAPIUrl + "Paste", content);
            string body = await resp.Content.ReadAsStringAsync();

            Utils.TeknikPasteResponse TeknikResponse = Http.JsonSerializer.Deserialize<Utils.TeknikPasteResponse>(body);
            LogURL = TeknikResponse.result.url;
        }

        private void OpenAppDataButton_Click(object sender, RoutedEventArgs e)
        {
            string location = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "AppData", "LocalLow", "Hyperbolic Magnetism");
            if (Directory.Exists(location))
            {
                Utils.OpenFolder(location);
            }
            else
            {
                MessageBox.Show((string)Application.Current.FindResource("Options:AppDataNotFound"));
            }
        }

        private async void YeetBSIPAButton_Click(object sender, RoutedEventArgs e)
        {
            if (Mods.Instance.AllModsList == null)
            {
                MainWindow.Instance.MainText = $"{Application.Current.FindResource("Options:GettingModList")}...";
                await Task.Run(async () => await Mods.Instance.GetAllMods());
                MainWindow.Instance.MainText = $"{Application.Current.FindResource("Options:FindingBSIPAVersion")}...";
                await Task.Run(() => Mods.Instance.GetBSIPAVersion());
            }
            foreach (Mod mod in Mods.InstalledMods)
            {
                if (mod.name.ToLowerInvariant() == "bsipa")
                {
                    Mods.Instance.UninstallMod(mod);
                    break;
                }
            }

            MainWindow.Instance.MainText = $"{Application.Current.FindResource("Options:BSIPAUninstalled")}...";
        }
        private async void YeetModsButton_Click(object sender, RoutedEventArgs e)
        {
            string title = (string)Application.Current.FindResource("Options:YeetModsBox:Title");
            string line1 = (string)Application.Current.FindResource("Options:YeetModsBox:RemoveAllMods");
            string line2 = (string)Application.Current.FindResource("Options:YeetModsBox:CannotBeUndone");

            var resp = System.Windows.Forms.MessageBox.Show($"{line1}\n{line2}", title, System.Windows.Forms.MessageBoxButtons.YesNo);
            if (resp == System.Windows.Forms.DialogResult.Yes)
            {

                if (Mods.Instance.AllModsList == null)
                {
                    MainWindow.Instance.MainText = $"{Application.Current.FindResource("Options:GettingModList")}...";
                    await Task.Run(async () => await Mods.Instance.CheckInstalledMods());
                }
                foreach (Mod mod in Mods.InstalledMods)
                {
                    Mods.Instance.UninstallMod(mod);
                }
                if (Directory.Exists(Path.Combine(App.BeatSaberInstallDirectory, "Plugins")))
                    Directory.Delete(Path.Combine(App.BeatSaberInstallDirectory, "Plugins"), true);
                if (Directory.Exists(Path.Combine(App.BeatSaberInstallDirectory, "Libs")))
                    Directory.Delete(Path.Combine(App.BeatSaberInstallDirectory, "Libs"), true);
                if (Directory.Exists(Path.Combine(App.BeatSaberInstallDirectory, "IPA")))
                    Directory.Delete(Path.Combine(App.BeatSaberInstallDirectory, "IPA"), true);

                MainWindow.Instance.MainText = $"{Application.Current.FindResource("Options:AllModsUninstalled")}...";
            }
        }

        private void ApplicationThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedItem == null)
            {
                Themes.ApplyWindowsTheme();
                MainWindow.Instance.MainText = (string)Application.Current.FindResource("Options:CurrentThemeRemoved");
            }
            else
            {
                Themes.ApplyTheme((sender as ComboBox).SelectedItem.ToString());
            }
        }

        public void LanguageSelectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedItem == null)
            {
                // Apply default language
                Console.WriteLine("Applying default language");
                Languages.LoadLanguage("en");
            }
            else
            {
                // Get the matching language from the LoadedLanguages array, then try and use it
                var languageName = (sender as ComboBox).SelectedItem.ToString();
                var selectedLanguage = Languages.LoadedLanguages.Find(language => language.NativeName.CompareTo(languageName) == 0);
                if (Languages.LoadLanguage(selectedLanguage.Name))
                {
                    Properties.Settings.Default.LanguageCode = selectedLanguage.Name;
                    Properties.Settings.Default.Save();
                    if (Languages.FirstRun)
                    {
                        Languages.FirstRun = false;
                    }
                    else
                    {
                        Process.Start(Utils.ExePath, App.Arguments);
                        Application.Current.Dispatcher.Invoke(() => { Application.Current.Shutdown(); });
                    }
                }
            }
        }

        private void ApplicationThemeExportTemplate_Click(object sender, RoutedEventArgs e)
        {
            Themes.WriteThemeToDisk("Ugly Kulu-Ya-Ku");
            Themes.LoadThemes();
        }

        private void ApplicationThemeOpenThemesFolder_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(Themes.ThemeDirectory))
            {
                Utils.OpenFolder(Themes.ThemeDirectory);
            }
            else
            {
                MessageBox.Show((string)Application.Current.FindResource("Options:ThemeFolderNotFound"));
            }
        }

        private void InstallPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            string playlistFile = Utils.GetManualFile();
            if (File.Exists(playlistFile))
            {
                Task.Run(() => { API.Playlists.DownloadFrom(playlistFile).Wait(); });
            }
        }

        private void ShowOCIWindowComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox.SelectedItem != null)
            {
                ComboBoxItem comboBoxItem = (ComboBoxItem)comboBox.SelectedItem;
                UpdateOCIWindow(comboBoxItem.Tag.ToString());
            }
        }

        public void UpdateOCIWindow(string state)
        {
            ComboBox comboBox = ShowOCIWindowComboBox;
            if (comboBox != null)
            {
                if (state == "Yes") comboBox.SelectedIndex = 0;
                else if (state == "Close") comboBox.SelectedIndex = 1;
                else if (state == "No") comboBox.SelectedIndex = 2;
                else return;
            }
            if (!string.IsNullOrEmpty(state))
            {
                OCIWindow = App.OCIWindow = Properties.Settings.Default.OCIWindow = state;
                Properties.Settings.Default.Save();
            }
        }
    }
}
