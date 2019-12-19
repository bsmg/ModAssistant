using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;

namespace ModAssistant.Pages
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    /// 
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
    }
}
