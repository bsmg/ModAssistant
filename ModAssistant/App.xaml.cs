using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ModAssistant;

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
        public static List<string> SavedMods = ModAssistant.Properties.Settings.Default.SavedMods.Split(',').ToList();


        private void Application_Startup(object sender, StartupEventArgs e)
        {
            BeatSaberInstallDirectory = Utils.GetInstallDir();
            BeatSaberInstallType = ModAssistant.Properties.Settings.Default.StoreType;
            SaveModSelection = ModAssistant.Properties.Settings.Default.SaveSelected;
            CheckInstalledMods = ModAssistant.Properties.Settings.Default.CheckInstalled;

            Update.Run();

            if (e.Args.Length == 0)
            {
                MainWindow window = new MainWindow();
                window.Show();
            }
            else
            {
                ArgumentHandler(e.Args);
            }
        }

        private void ArgumentHandler(string[] Args)
        {
            Utils.SendNotify(Args.Count().ToString());
            switch (Args[0])
            {
                case "--install":
                    if (!String.IsNullOrEmpty(Args[1]))
                        OneClickInstaller.InstallAsset(Args[1]);
                    else
                        Utils.SendNotify("Invalid argument! '--install' requires an option.");
                    break;

                case "--update":
                    // Update
                    break;

                default:
                    Utils.SendNotify("Unrecognized argument. Closing Mod Assistant.");
                    break;
            }
            Current.Shutdown();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("An unhandled exception just occurred: " + e.Exception.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            e.Handled = true;
        }

        public void RegisterOneClickInstalls ()
        {

        }
    }
}
