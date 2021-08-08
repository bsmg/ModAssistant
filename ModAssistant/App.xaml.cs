using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public static bool ReinstallInstalledMods;
        public static bool CloseWindowOnFinish;
        public static string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static List<string> SavedMods = ModAssistant.Properties.Settings.Default.SavedMods.Split(',').ToList();
        public static MainWindow window;
        public static string Arguments;
        public static bool Update = true;
        public static bool GUI = true;
        public static string OCIWindow;

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            // Set SecurityProtocol to prevent crash with TLS
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            if (ModAssistant.Properties.Settings.Default.UpgradeRequired)
            {
                ModAssistant.Properties.Settings.Default.Upgrade();
                ModAssistant.Properties.Settings.Default.UpgradeRequired = false;
                ModAssistant.Properties.Settings.Default.Save();
            }

            Version = Version.Substring(0, Version.Length - 2);
            OCIWindow = ModAssistant.Properties.Settings.Default.OCIWindow;
            if (string.IsNullOrEmpty(OCIWindow))
            {
                OCIWindow = "Yes";
            }
            Pages.Options options = Pages.Options.Instance;
            options.InstallDirectory =
                BeatSaberInstallDirectory = Utils.GetInstallDir();

            Languages.LoadLanguages();

            while (string.IsNullOrEmpty(BeatSaberInstallDirectory))
            {
                string title = (string)Current.FindResource("App:InstallDirDialog:Title");
                string body = (string)Current.FindResource("App:InstallDirDialog:OkCancel");

                if (System.Windows.Forms.MessageBox.Show(body, title, System.Windows.Forms.MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
                {
                    BeatSaberInstallDirectory = Utils.GetManualDir();
                }
                else
                {
                    Environment.Exit(0);
                }
            }

            options.InstallType =
                BeatSaberInstallType = ModAssistant.Properties.Settings.Default.StoreType;
            options.SaveSelection =
                SaveModSelection = ModAssistant.Properties.Settings.Default.SaveSelected;
            options.CheckInstalledMods =
                CheckInstalledMods = ModAssistant.Properties.Settings.Default.CheckInstalled;
            options.SelectInstalledMods =
                SelectInstalledMods = ModAssistant.Properties.Settings.Default.SelectInstalled;
            options.ReinstallInstalledMods =
                ReinstallInstalledMods = ModAssistant.Properties.Settings.Default.ReinstallInstalled;
            options.CloseWindowOnFinish =
                CloseWindowOnFinish = ModAssistant.Properties.Settings.Default.CloseWindowOnFinish;

            await ArgumentHandler(e.Args);
            await Init();
            options.UpdateOCIWindow(OCIWindow);
        }

        private async Task Init()
        {
            if (Update)
            {
                try
                {
                    await Task.Run(async () => await Updater.Run());
                }
                catch (UnauthorizedAccessException)
                {
                    Utils.StartAsAdmin(Arguments, true);
                }
            }

            if (GUI)
            {
                window = new MainWindow();
                window.Show();
            }
            else
            {
                //Application.Current.Shutdown();
            }
        }

        private async Task ArgumentHandler(string[] args)
        {
            Arguments = string.Join(" ", args);
            while (args.Length > 0)
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

                        if (CloseWindowOnFinish)
                        {
                            await Task.Delay(5 * 1000);
                            Current.Shutdown();
                        }

                        Update = false;
                        GUI = false;
                        args = Shift(args, 2);
                        break;

                    case "--no-update":
                        Update = false;
                        args = Shift(args);
                        break;

                    case "--language":
                        if (args.Length < 2 || string.IsNullOrEmpty(args[1]))
                        {
                            Utils.SendNotify(string.Format((string)Current.FindResource("App:InvalidArgument"), "--language"));
                        }
                        else
                        {
                            if (Languages.LoadLanguage(args[1]))
                            {
                                ModAssistant.Properties.Settings.Default.LanguageCode = args[1];
                                ModAssistant.Properties.Settings.Default.Save();
                                Languages.UpdateUI(args[1]);
                            }
                        }

                        args = Shift(args, 2);
                        break;

                    case "--register":
                        if (args.Length < 3 || string.IsNullOrEmpty(args[1]))
                        {
                            Utils.SendNotify(string.Format((string)Current.FindResource("App:InvalidArgument"), "--register"));
                        }
                        else
                        {
                            OneClickInstaller.Register(args[1], true, args[2]);
                        }

                        Update = false;
                        GUI = false;
                        args = Shift(args, 3);
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

                        Update = false;
                        GUI = false;
                        args = Shift(args, 2);
                        break;

                    case "--runforever":
                        while (true)
                        {

                        }

                    default:
                        Utils.SendNotify((string)Current.FindResource("App:UnrecognizedArgument"));
                        args = Shift(args);
                        break;
                }
            }
        }

        private static string[] Shift(string[] array, int places = 1)
        {
            if (places >= array.Length) return Array.Empty<string>();
            string[] newArray = new string[array.Length - places];
            for (int i = places; i < array.Length; i++)
            {
                newArray[i - places] = array[i];
            }

            return newArray;
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string title = (string)Current.FindResource("App:Exception");
            string body = (string)Current.FindResource("App:UnhandledException");
            MessageBox.Show($"{body}: {e.Exception}", title, MessageBoxButton.OK, MessageBoxImage.Warning);

            e.Handled = true;
            Current.Shutdown();
        }
    }
}
