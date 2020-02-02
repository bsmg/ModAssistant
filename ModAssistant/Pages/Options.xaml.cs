using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Path = System.IO.Path;
using System.Net.Http;

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
        public bool ModSaberProtocolHandlerEnabled { get; set; }
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
            ModSaberProtocolHandlerEnabled = OneClickInstaller.IsRegistered("modsaber");
        }

        private void SelectDirButton_Click(object sender, RoutedEventArgs e)
        {
            Utils.GetManualDir();
            DirectoryTextBlock.Text = InstallDirectory;
            GameTypeTextBlock.Text = InstallType;
        }

        private void OpenDirButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(InstallDirectory);
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
                Mods.Instance.PendingChanges = true;
        }

        private void CheckInstalled_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.CheckInstalled = false;
            App.CheckInstalledMods = false;
            CheckInstalledMods = false;
            Properties.Settings.Default.Save();
            SelectInstalled.IsEnabled = false;
            if (MainWindow.ModsOpened)
                Mods.Instance.PendingChanges = true;
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
                MainWindow.Instance.MainText = "Uploading Log...";
                await Task.Run(async () => await UploadLog());

                System.Diagnostics.Process.Start(LogURL);
                Clipboard.SetText(LogURL);
                MainWindow.Instance.MainText = "Log URL Copied To Clipboard!";
            }
            catch (Exception exception)
            {
                MainWindow.Instance.MainText = "Uploading Log Failed.";
                MessageBox.Show("Could not upload log file to Teknik, please try again or send the file manually.\n ================= \n" + exception, "Uploading log failed!");
                System.Diagnostics.Process.Start(Path.Combine(InstallDirectory, "Logs"));
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

        private async void YeetBSIPAButton_Click(object sender, RoutedEventArgs e)
        {
            if (Mods.Instance.AllModsList == null)
            {
                MainWindow.Instance.MainText = "Getting Mod List...";
                await Mods.Instance.GetAllMods();
                MainWindow.Instance.MainText = "Finding BSIPA Version...";
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
            MainWindow.Instance.MainText = "BSIPA Uninstalled...";
        }
        private async void YeetModsButton_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show($"Are you sure you want to remove ALL mods?\nThis cannot be undone.", $"Uninstall All Mods?", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {

                if (Mods.Instance.AllModsList == null)
                {
                    MainWindow.Instance.MainText = "Getting Mod List...";
                    await Mods.Instance.CheckInstalledMods();
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
                MainWindow.Instance.MainText = "All Mods Uninstalled...";
            }
        }
    }
}
