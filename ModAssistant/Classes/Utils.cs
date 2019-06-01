using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;

namespace ModAssistant.Classes
{
    public static partial class Utils
    {
        public static readonly bool IsAdmin =
            new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        public static readonly string ExePath = Process.GetCurrentProcess().MainModule?.FileName;

        public static class GameVersions
        {
            public static Dictionary<string, string> SteamVersions = new Dictionary<string, string>
            {
                {"3708884", "0.13.2"},
                {"3844832", "1.0.0"},
                {"3861357", "1.0.1"}
            };
        }

        public static void SendNotify(string message, string title = "Mod Assistant")
        {
            var notification = new System.Windows.Forms.NotifyIcon()
            {
                Visible = true,
                Icon = System.Drawing.SystemIcons.Information,
                BalloonTipTitle = title,
                BalloonTipText = message
            };

            notification.ShowBalloonTip(5000);

            notification.Dispose();
        }

        public static void StartAsAdmin(string arguments, bool close = false)
        {
            using (var process = new Process())
            {
                var processModule = Process.GetCurrentProcess().MainModule;
                if (processModule != null)
                    process.StartInfo.FileName = processModule.FileName;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.Verb = "runas";

                try
                {
                    process.Start();
                    if (!close)
                        process.WaitForExit();
                }
                catch
                {
                    MessageBox.Show("Mod Assistant needs to run this task as Admin. Please try again.");
                }

                if (close)
                    Application.Current.Shutdown();
            }
        }

        public static string CalculateMd5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public static string GetInstallDir()
        {
            string installDir = Properties.Settings.Default.InstallFolder;

            if (!string.IsNullOrEmpty(installDir) && Directory.Exists(installDir))
            {
                return installDir;
            }

            try
            {
                installDir = GetSteamDir();
            }
            catch { }

            if (!string.IsNullOrEmpty(installDir))
            {
                return installDir;
            }

            try
            {
                installDir = GetOculusDir();
            }
            catch { }

            if (!string.IsNullOrEmpty(installDir))
            {
                return installDir;
            }

            MessageBox.Show("Could not detect your Beat Saber install folder. Please select it manually.");

            installDir = GetManualDir();
            if (!string.IsNullOrEmpty(installDir))
            {
                return installDir;
            }

            return null;
        }

        public static string SetDir(string directory, string store)
        {
            App.BeatSaberInstallDirectory = directory;
            App.BeatSaberInstallType = store;

            Pages.Options.Instance.InstallDirectory = directory;
            Pages.Options.Instance.InstallType = store;

            Properties.Settings.Default.InstallFolder = directory;
            Properties.Settings.Default.StoreType = store;
            Properties.Settings.Default.Save();

            return directory;
        }

        public static string GetSteamDir()
        {
            var steamInstall = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                ?.OpenSubKey("SOFTWARE")?.OpenSubKey("WOW6432Node")?.OpenSubKey("Valve")?.OpenSubKey("Steam")
                ?.GetValue("InstallPath").ToString();

            if (string.IsNullOrEmpty(steamInstall))
            {
                steamInstall = Registry.LocalMachine.OpenSubKey("SOFTWARE")?.OpenSubKey("WOW6432Node")
                    ?.OpenSubKey("Valve")?.OpenSubKey("Steam")?.GetValue("InstallPath").ToString();
            }

            if (string.IsNullOrEmpty(steamInstall)) return null;

            var vdf = Path.Combine(steamInstall, @"steamapps\libraryfolders.vdf");
            if (!File.Exists(@vdf)) return null;

            var regex = new Regex("\\s\"\\d\"\\s+\"(.+)\"");

            var steamPaths = new List<string>();
            steamPaths.Add(Path.Combine(steamInstall, @"steamapps"));

            using (var reader = new StreamReader(@vdf))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var match = regex.Match(line);
                    if (match.Success)
                    {
                        steamPaths.Add(Path.Combine(match.Groups[1].Value.Replace(@"\\", @"\"), @"steamapps"));
                    }
                }
            }

            regex = new Regex("\\s\"installdir\"\\s+\"(.+)\"");
            foreach (var path in steamPaths)
            {
                if (File.Exists(Path.Combine(@path, @"appmanifest_" + Utils.Constants.BeatSaberAppid + ".acf")))
                {
                    using (var reader =
                        new StreamReader(Path.Combine(@path,
                            @"appmanifest_" + Utils.Constants.BeatSaberAppid + ".acf")))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var match = regex.Match(line);
                            if (match.Success)
                            {
                                if (File.Exists(Path.Combine(@path, @"common", match.Groups[1].Value,
                                    "Beat Saber.exe")))
                                {
                                    return SetDir(Path.Combine(@path, @"common", match.Groups[1].Value), "Steam");
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static string GetSteamVersion()
        {
            var steamInstall = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                ?.OpenSubKey("SOFTWARE")?.OpenSubKey("WOW6432Node")?.OpenSubKey("Valve")?.OpenSubKey("Steam")
                ?.GetValue("InstallPath").ToString();

            if (string.IsNullOrEmpty(steamInstall))
            {
                steamInstall = Registry.LocalMachine.OpenSubKey("SOFTWARE")?.OpenSubKey("WOW6432Node")
                    ?.OpenSubKey("Valve")?.OpenSubKey("Steam")?.GetValue("InstallPath").ToString();
            }

            if (string.IsNullOrEmpty(steamInstall)) return null;

            var vdf = Path.Combine(steamInstall, @"steamapps\libraryfolders.vdf");
            if (!File.Exists(@vdf)) return null;

            var regex = new Regex("\\s\"\\d\"\\s+\"(.+)\"");
            var steamPaths = new List<string>();
            steamPaths.Add(Path.Combine(steamInstall, @"steamapps"));

            using (var reader = new StreamReader(@vdf))
            {
                string line;
                while (!string.IsNullOrWhiteSpace((line = reader.ReadLine())))
                {
                    var match = regex.Match(line);
                    if (match.Success)
                    {
                        steamPaths.Add(Path.Combine(match.Groups[1].Value.Replace(@"\\", @"\"), @"steamapps"));
                    }
                }
            }

            regex = new Regex("\\s\"buildid\"\\s+\"(.+)\"");
            foreach (var path in steamPaths)
            {
                if (File.Exists(Path.Combine(@path, @"appmanifest_" + Utils.Constants.BeatSaberAppid + ".acf")))
                {
                    using (var reader =
                        new StreamReader(Path.Combine(@path,
                            @"appmanifest_" + Utils.Constants.BeatSaberAppid + ".acf")))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var match = regex.Match(line);
                            if (match.Success)
                            {
                                GameVersions.SteamVersions.TryGetValue(match.Groups[1].Value, out var version);
                                return version ?? string.Empty;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static string GetOculusDir()
        {
            var oculusInstall = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                ?.OpenSubKey("SOFTWARE")?.OpenSubKey("Wow6432Node")?.OpenSubKey("Oculus VR, LLC")?.OpenSubKey("Oculus")
                ?.OpenSubKey("Config")?.GetValue("InitialAppLibrary").ToString();
            if (string.IsNullOrEmpty(oculusInstall)) return null;

            if (!string.IsNullOrEmpty(oculusInstall))
            {
                if (File.Exists(Path.Combine(oculusInstall, "Software", "hyperbolic-magnetism-beat-saber",
                    "Beat Saber.exe")))
                {
                    return SetDir(Path.Combine(oculusInstall, "Software", "hyperbolic-magnetism-beat-saber"), "Oculus");
                }
            }

            // Yoinked this code from Umbranox's Mod Manager. Lot's of thanks and love for Umbra <3
            using (var librariesKey = Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey("Oculus VR, LLC")
                ?.OpenSubKey("Oculus")?.OpenSubKey("Libraries"))
            {
                // Oculus libraries uses GUID volume paths like this "\\?\Volume{0fea75bf-8ad6-457c-9c24-cbe2396f1096}\Games\Oculus Apps", we need to transform these to "D:\Game"\Oculus Apps"
                var wqlQuery = new WqlObjectQuery("SELECT * FROM Win32_Volume");
                var searcher = new ManagementObjectSearcher(wqlQuery);
                var guidLetterVolumes = new Dictionary<string, string>();

                foreach (var disk in searcher.Get())
                {
                    var diskId = ((string) disk.GetPropertyValue("DeviceID")).Substring(11, 36);
                    var diskLetter = ((string) disk.GetPropertyValue("DriveLetter")) + @"\";

                    if (!string.IsNullOrWhiteSpace(diskLetter))
                    {
                        guidLetterVolumes.Add(diskId, diskLetter);
                    }
                }

                // Search among the library folders
                foreach (var libraryKeyName in librariesKey.GetSubKeyNames())
                {
                    using (var libraryKey = librariesKey.OpenSubKey(libraryKeyName))
                    {
                        var libraryPath = (string) libraryKey.GetValue("Path");
                        // Yoinked this code from Megalon's fix. <3
                        var guidLetter = guidLetterVolumes.FirstOrDefault(x => libraryPath.Contains(x.Key)).Value;
                        if (!string.IsNullOrEmpty(guidLetter))
                        {
                            var finalPath = Path.Combine(guidLetter, libraryPath.Substring(49),
                                @"Software\hyperbolic-magnetism-beat-saber");
                            if (File.Exists(Path.Combine(finalPath, "Beat Saber.exe")))
                            {
                                return SetDir(finalPath, "Oculus");
                            }
                        }
                    }
                }
            }

            return null;
        }
        /*
        public static string GetManualDir()
        {

            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Multiselect = false,
                Title = "Select your Beat Saber installation folder"
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return dialog.FileName;
            }

            return null;
        }*/

        public static string GetManualDir()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog()
            {
                Title = "Select your Beat Saber install folder",
                Filter = "Directory|*.this.directory",
                FileName = "select"
            };

            if (dialog.ShowDialog() == true)
            {
                var path = dialog.FileName;

                path = path.Replace("\\select.this.directory", "");
                path = path.Replace(".this.directory", "");
                path = path.Replace("\\select.directory", "");

                if (File.Exists(Path.Combine(path, "Beat Saber.exe")))
                {
                    string store;
                    if (File.Exists(Path.Combine(path, "Beat Saber_Data", "Plugins", "steam_api64.dll")))
                    {
                        store = "Steam";
                    }
                    else
                    {
                        store = "Oculus";
                    }

                    return SetDir(path, store);
                }
            }

            return null;
        }

        public static bool IsVoid()
        {
            var directory = App.BeatSaberInstallDirectory;

            if (File.Exists(Path.Combine(directory, "IGG-GAMES.COM.url")) ||
                File.Exists(Path.Combine(directory, "SmartSteamEmu.ini")) ||
                File.Exists(Path.Combine(directory, "GAMESTORRENT.CO.url")) ||
                File.Exists(Path.Combine(directory, "Beat Saber_Data", "Plugins", "BSteam crack.dll")) ||
                File.Exists(Path.Combine(directory, "Beat Saber_Data", "Plugins", "HUHUVR_steam_api64.dll")) ||
                Directory.GetFiles(Path.Combine(directory, "Beat Saber_Data", "Plugins"), "*.ini",
                    SearchOption.TopDirectoryOnly).Length >
                0)
                return true;
            return false;
        }

        public static void Download(string link, string output)
        {
            var webClient = new WebClient();
            webClient.Headers.Add("user-agent", "ModAssistant/" + App.Version);

            var file = webClient.DownloadData(link);
            File.WriteAllBytes(output, file);
        }

        private delegate void ShowMessageBoxDelegate(string message, string caption);

        private static void ShowMessageBox(string message, string caption)
        {
            MessageBox.Show(message, caption);
        }

        public static void ShowMessageBoxAsync(string message, string caption)
        {
            var caller = new ShowMessageBoxDelegate(ShowMessageBox);
            caller.BeginInvoke(message, caption, null, null);
        }

        public static void ShowMessageBoxAsync(string message)
        {
            var caller = new ShowMessageBoxDelegate(ShowMessageBox);
            caller.BeginInvoke(message, null, null, null);
        }
    }
}