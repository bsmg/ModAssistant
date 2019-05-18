using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Runtime.Serialization;
using ModAssistant.Pages;
using System.Reflection;

namespace ModAssistant
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;
        public static bool ModsOpened = false;
        public static string GameVersion;

        public string MainText
        {
            get
            {
                return MainTextBlock.Text;
            }
            set
            {
                Dispatcher.Invoke(new Action(() => { MainWindow.Instance.MainTextBlock.Text = value; }));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;

            VersionText.Text = App.Version;

            Main.Content = Intro.Instance;

            List<string> versions;
            string json = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Utils.Constants.BeatModsAPIUrl + "version");
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.UserAgent = "ModAssistant/" + App.Version;

            versions = null;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    var serializer = new JavaScriptSerializer();
                    versions = serializer.Deserialize<string[]>(reader.ReadToEnd()).ToList();
                }

                if (!String.IsNullOrEmpty(Properties.Settings.Default.GameVersion) && versions.Contains(Properties.Settings.Default.GameVersion))
                    GameVersion = Properties.Settings.Default.GameVersion;
                else
                    GameVersion = versions[versions.Count - 1];

                GameVersionsBox.ItemsSource = versions;
                GameVersionsBox.SelectedValue = GameVersion;
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not load game versions, Mods tab will be unavailable.\n" + e);
            }

            if (!String.IsNullOrEmpty(GameVersion) && Properties.Settings.Default.Agreed)
            {
                MainWindow.Instance.ModsButton.IsEnabled = true;
            }
        }

        private void ModsButton_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = Mods.Instance;

            if (!ModsOpened)
            {
                Mods.Instance.LoadMods();
                ModsOpened = true;
                return;
            }

            if (Mods.Instance.PendingChanges)
            {
                Mods.Instance.LoadMods();
                Mods.Instance.PendingChanges = false;
            }
        }

        private void IntroButton_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = Intro.Instance;
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = About.Instance;
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = Options.Instance;
        }

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            Mods.Instance.InstallMods();
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            Mods.ModListItem mod = ((Mods.ModListItem)Mods.Instance.ModsListView.SelectedItem);
            string infoUrl = mod.ModInfo.link;
            if (String.IsNullOrEmpty(infoUrl))
            {
                MessageBox.Show(mod.ModName + " does not have an info page");
            }
            else
            {
                System.Diagnostics.Process.Start(infoUrl);
            }
        }

        private void GameVersionsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string oldGameVersion = GameVersion;
            
            GameVersion = (sender as ComboBox).SelectedItem.ToString();
            
            if (String.IsNullOrEmpty(oldGameVersion)) return;

            Properties.Settings.Default.GameVersion = GameVersion;
            Properties.Settings.Default.Save();

            if (ModsOpened)
            {
                Mods.Instance.LoadMods();
            }
        }
    }
}
