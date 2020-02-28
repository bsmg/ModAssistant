using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace ModAssistant
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string BeatSaberInstallDirectory;
        public static string BeatSaberInstallType;
        public static bool SaveModSelection;
        public static bool CheckInstalledMods;
        public static bool SelectInstalledMods;
        public static string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static List<string> SavedMods = ModAssistant.Properties.Settings.Default.SavedMods.Split(',').ToList();


        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            // Load localisation languages
            LoadLanguage(CultureInfo.CurrentCulture.Name);

            // Uncomment the next line to debug localisation
            // LoadLanguage("en-DEBUG");

            if (ModAssistant.Properties.Settings.Default.UpgradeRequired)
            {
                ModAssistant.Properties.Settings.Default.Upgrade();
                ModAssistant.Properties.Settings.Default.UpgradeRequired = false;
                ModAssistant.Properties.Settings.Default.Save();
            }

            Version = Version.Substring(0, Version.Length - 2);
            BeatSaberInstallDirectory = Utils.GetInstallDir();

            while (string.IsNullOrEmpty(App.BeatSaberInstallDirectory))
            {
                string title = (string)Current.FindResource("App:InstallDirDialog:Title");
                string body = (string)Current.FindResource("App:InstallDirDialog:OkCancel");

                if (System.Windows.Forms.MessageBox.Show(body, title, System.Windows.Forms.MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
                {
                    App.BeatSaberInstallDirectory = Utils.GetManualDir();
                }
                else
                {
                    Environment.Exit(0);
                }
            }

            BeatSaberInstallType = ModAssistant.Properties.Settings.Default.StoreType;
            SaveModSelection = ModAssistant.Properties.Settings.Default.SaveSelected;
            CheckInstalledMods = ModAssistant.Properties.Settings.Default.CheckInstalled;
            SelectInstalledMods = ModAssistant.Properties.Settings.Default.SelectInstalled;

            if (e.Args.Length == 0)
            {
                await Task.Run(async () => await Updater.Run());

                MainWindow window = new MainWindow();
                window.Show();
            }
            else
            {
                await ArgumentHandler(e.Args);
            }
        }

        private async Task ArgumentHandler(string[] args)
        {
            switch (args[0])
            {
                case "--install":
                    if (args.Length < 2 || string.IsNullOrEmpty(args[1]))
                    {
                        Utils.SendNotify(string.Format((string)Current.FindResource("App:InvalidArgument"), "--install"));
                    }
                    else
                    {
                        await OneClickInstaller.InstallAsset(args[1]);
                    }
                    break;

                case "--no-update":
                    MainWindow window = new MainWindow();
                    window.Show();
                    break;

                case "--register":
                    if (args.Length < 2 || string.IsNullOrEmpty(args[1]))
                    {
                        Utils.SendNotify(string.Format((string)Current.FindResource("App:InvalidArgument"), "--register"));
                    }
                    else
                    {
                        OneClickInstaller.Register(args[1], true);
                    }

                    break;

                case "--unregister":
                    if (args.Length < 2 || string.IsNullOrEmpty(args[1]))
                    {
                        Utils.SendNotify(string.Format((string)Current.FindResource("App:InvalidArgument"), "--unregister"));
                    }
                    else
                    {
                        OneClickInstaller.Unregister(args[1], true);
                    }
                    break;

                default:
                    Utils.SendNotify((string)Current.FindResource("App:UnrecognizedArgument"));
                    break;
            }

            Current.Shutdown();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string title = (string)Current.FindResource("App:Exception");
            string body = (string)Current.FindResource("App:UnhandledException");
            MessageBox.Show($"{body}: {e.Exception}", "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);

            e.Handled = true;
            Application.Current.Shutdown();
        }

        private ResourceDictionary LanguagesDict
        {
            get
            {
                return Resources.MergedDictionaries[1];
            }
        }

        private void LoadLanguage(string culture)
        {
            try
            {
                LanguagesDict.Source = new Uri($"Localisation/{culture}.xaml", UriKind.Relative);
            }
            catch (IOException)
            {
                if (culture.Contains("-"))
                {
                    LoadLanguage(culture.Split('-').First());
                }
                // Can't load language file
            }
        }
    }
}
