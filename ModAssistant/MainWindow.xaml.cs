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

            if (Utils.isVoid())
            {
                Main.Content = Invalid.Instance;
                MainWindow.Instance.ModsButton.IsEnabled = false;
                MainWindow.Instance.OptionsButton.IsEnabled = false;
                MainWindow.Instance.IntroButton.IsEnabled = false;
                MainWindow.Instance.AboutButton.IsEnabled = false;
                MainWindow.Instance.GameVersionsBox.IsEnabled = false;
                return;
            }

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
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    versions = serializer.Deserialize<string[]>(reader.ReadToEnd()).ToList();
                }

                GameVersion = GetGameVersion(versions);

                GameVersionsBox.ItemsSource = versions;
                GameVersionsBox.SelectedValue = GameVersion;
            }
            catch (Exception e)
            {
                GameVersionsBox.IsEnabled = false;
                MessageBox.Show("Could not load game versions, Mods tab will be unavailable.\n" + e);
            }

            if (!String.IsNullOrEmpty(GameVersion) && Properties.Settings.Default.Agreed)
            {
                MainWindow.Instance.ModsButton.IsEnabled = true;
            }

            if (!Properties.Settings.Default.Agreed || String.IsNullOrEmpty(Properties.Settings.Default.LastTab))
            {
                Main.Content = Intro.Instance;
            }
            else
            {
                switch (Properties.Settings.Default.LastTab)
                {
                    case "Intro":
                        Main.Content = Intro.Instance;
                        break;
                    case "Mods":
                        Mods.Instance.LoadMods();
                        ModsOpened = true;
                        Main.Content = Mods.Instance;
                        break;
                    case "About":
                        Main.Content = About.Instance;
                        break;
                    case "Options":
                        Main.Content = Options.Instance;
                        break;
                    default:
                        Main.Content = Intro.Instance;
                        break;
                }
            }
        }

        private string GetGameVersion(List<string> versions)
        {
            string version = Utils.GetVersion();
            if (!String.IsNullOrEmpty(version) && versions.Contains(version))
            {
                return version;
            }

            string versionsString = String.Join(",", versions.ToArray());
            if (Properties.Settings.Default.AllGameVersions != versionsString)
            {
                Properties.Settings.Default.AllGameVersions = versionsString;
                Properties.Settings.Default.Save();
                Utils.ShowMessageBoxAsync("It looks like there's been a game update.\n\nPlease double check that the correct version is selected at the bottom left corner!", "New Game Version Detected!");
                return versions[0];
            }

            if (!String.IsNullOrEmpty(Properties.Settings.Default.GameVersion) && versions.Contains(Properties.Settings.Default.GameVersion))
                return Properties.Settings.Default.GameVersion;
            return versions[0];
        }

        private void ModsButton_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = Mods.Instance;
            Properties.Settings.Default.LastTab = "Mods";
            Properties.Settings.Default.Save();

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
            Properties.Settings.Default.LastTab = "Intro";
            Properties.Settings.Default.Save();
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = About.Instance;
            Properties.Settings.Default.LastTab = "About";
            Properties.Settings.Default.Save();
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = Options.Instance;
            Properties.Settings.Default.LastTab = "Options";
            Properties.Settings.Default.Save();
        }

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            Mods.Instance.InstallMods();
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            if ((Mods.ModListItem)Mods.Instance.ModsListView.SelectedItem == null)
            {
                MessageBox.Show("No mod selected");
                return;
            }
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

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            About.Instance.PatUp.IsOpen = false;
            About.Instance.PatButton.IsEnabled = true;
        }
    }
}
