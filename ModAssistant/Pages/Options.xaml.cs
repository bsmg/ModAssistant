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

            if (OneClickInstaller.IsRegistered())
            {
                ProtocolHandler.IsChecked = true;
            }
            else
            {
                ProtocolHandler.IsChecked = false;
            }

            ProtocolHandler.IsEnabled = false;

            this.DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Utils.GetManualDir();
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
        }

        private void CheckInstalled_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.CheckInstalled = false;
            App.CheckInstalledMods = false;
            CheckInstalledMods = false;
            Properties.Settings.Default.Save();
            SelectInstalled.IsEnabled = false;
        }

        private void ProtocolHandler_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ProtocolHandler_Unchecked(object sender, RoutedEventArgs e)
        {

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
