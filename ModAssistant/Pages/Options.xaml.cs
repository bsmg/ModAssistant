using System;
using System.Collections.Generic;
using System.IO;
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
        public bool ModelSaberProtocolHandlerEnabled { get; set; }
        public bool BeatSaverProtocolHandlerEnabled { get; set; }
        public string LogURL { get; private set; }

        public Options()
        {
            InitializeComponent();
            InstallDirectory = App.BeatSaberInstallDirectory;
            InstallType = App.BeatSaberInstallType;
            SaveSelection = App.SaveModSelection;
            CheckInstalledMods = App.CheckInstalledMods;
            SelectInstalledMods = App.SelectInstalledMods;
            if (!CheckInstalledMods)
                SelectInstalled.IsEnabled = false;

            UpdateHandlerStatus();

            this.DataContext = this;
        }

        public void UpdateHandlerStatus()
        {
            ModelSaberProtocolHandlerEnabled = OneClickInstaller.IsRegistered("modelsaber");
            BeatSaverProtocolHandlerEnabled = OneClickInstaller.IsRegistered("beatsaver");
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

            if (MainWindow.ModsOpened)
            {
                Mods.Instance.PendingChanges = true;
            }
        }

        public void ModelSaberProtocolHandler_Checked(object sender, RoutedEventArgs e)
        {
            OneClickInstaller.Register("modelsaber");
        }

        public void ModelSaberProtocolHandler_Unchecked(object sender, RoutedEventArgs e)
        {
            OneClickInstaller.Unregister("modelsaber");
        }

        public void BeatSaverProtocolHandler_Checked(object sender, RoutedEventArgs e)
        {
            OneClickInstaller.Register("beatsaver");
        }

        public void BeatSaverProtocolHandler_Unchecked(object sender, RoutedEventArgs e)
        {
            OneClickInstaller.Unregister("beatsaver");
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

        private async void OpenLogsDirButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow.Instance.MainText = $"{Application.Current.FindResource("Options:UploadingLog")}...";
                await Task.Run(async () => await UploadLog());

                System.Diagnostics.Process.Start(LogURL);
                Clipboard.SetText(LogURL);
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

            var nvc = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("title", $"_latest.log ({now.ToString(DateFormat)})"),
                new KeyValuePair<string, string>("expireUnit", "hour"),
                new KeyValuePair<string, string>("expireLength", "5"),
                new KeyValuePair<string, string>("code", File.ReadAllText(Path.Combine(InstallDirectory, "Logs", "_latest.log"))),
            };

            var req = new HttpRequestMessage(HttpMethod.Post, Utils.Constants.TeknikAPIUrl + "Paste")
            {
                Content = new FormUrlEncodedContent(nvc),
            };

            var resp = await Http.HttpClient.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();

            var TeknikResponse = Http.JsonSerializer.Deserialize<Utils.TeknikPasteResponse>(body);
            LogURL = TeknikResponse.result.url;
        }

        private void OpenAppDataButton_Click(object sender, RoutedEventArgs e)
        {
            string location = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "AppData", "LocalLow", "Hyperbolic Magnetism");
            Utils.OpenFolder(location);
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
                if (mod.name.ToLower() == "bsipa")
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
                    MainWindow.Instance.MainText = $"{Application.Current.FindResource("Options: GettingModList")}...";
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
    }
}
