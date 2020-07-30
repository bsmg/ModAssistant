using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using static ModAssistant.Http;

namespace ModAssistant
{
    public class Utils
    {
        public static bool IsAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        public static string ExePath = Process.GetCurrentProcess().MainModule.FileName;

        public class Constants
        {
            public const string BeatSaberAPPID = "620980";

            public const string BeatModsAPIUrl_beatmods = "https://beatmods.com/api/v1/";
            public const string TeknikAPIUrl_beatmods = "https://api.teknik.io/v1/";
            public const string BeatModsURL_beatmods = "https://beatmods.com";
            public const string BeatModsVersions_beatmods = "https://versions.beatmods.com/versions.json";
            public const string BeatModsAlias_beatmods = "https://alias.beatmods.com/aliases.json";
            public const string WeebCDNAPIURL_beatmods = "https://pat.assistant.moe/api/v1.0/";
            public const string BeatModsTranslation_beatmods = "https://wgzeyu.github.io/BeatSaberModListTranslationRepo/zh-Hans.json";

            public const string BeatModsAPIUrl_wgzeyu = "https://beatmods.gtxcn.com/api/v1/";
            public const string TeknikAPIUrl_wgzeyu = "https://beatmods.gtxcn.com/teknik/v1/";
            public const string BeatModsURL_wgzeyu = "https://beatmods.gtxcn.com";
            public const string BeatModsVersions_wgzeyu = "https://beatmods.gtxcn.com/bmversions/versions.json";
            public const string BeatModsAlias_wgzeyu = "https://beatmods.gtxcn.com/alias/aliases.json";
            public const string WeebCDNAPIURL_wgzeyu = "https://beatmods.gtxcn.com/assistant/api/v1.0/";
            public const string BeatModsTranslation_wgzeyu = "https://beatmods.gtxcn.com/github/BeatSaberModListTranslationRepo/zh-Hans.json";

            public const string BeatModsAPIUrl_bmtop = "https://api.beatmods.top/api/v1/";
            public const string TeknikAPIUrl_bmtop = "https://teknikapi.beatmods.top/v1/";
            public const string BeatModsURL_bmtop = "https://beatmods.beatmods.top";
            public const string BeatModsVersions_bmtop = "https://versions-beatmods.beatmods.top/versions.json";
            public const string BeatModsAlias_bmtop = "https://alias-beatmods.beatmods.top/aliases.json";
            public const string WeebCDNAPIURL_bmtop = "https://pat-assistant-moe.beatmods.top/api/v1.0/";

            public static string BeatModsAPIUrl;
            public static string TeknikAPIUrl;
            public static string BeatModsURL;
            public static string BeatModsVersions;
            public static string BeatModsAlias;
            public static string WeebCDNAPIURL;

            public const string BeatModsModsOptions = "mod?status=approved";
            public const string MD5Spacer = "                                 ";
            public static readonly char[] IllegalCharacters = new char[]
            {
                '<', '>', ':', '/', '\\', '|', '?', '*', '"',
                '\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007',
                '\u0008', '\u0009', '\u000a', '\u000b', '\u000c', '\u000d', '\u000e', '\u000d',
                '\u000f', '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016',
                '\u0017', '\u0018', '\u0019', '\u001a', '\u001b', '\u001c', '\u001d', '\u001f',
            };

            public static void UpdateDownloadNode() {
                if (ModAssistant.Properties.Settings.Default.StoreType == "Netvios")
                {
                    ModAssistant.Properties.Settings.Default.DownloadServer = "网易版@BeatMods.top";
                }

                if (ModAssistant.Properties.Settings.Default.DownloadServer == "国内源@WGzeyu")
                {
                    Utils.Constants.BeatModsAPIUrl = Utils.Constants.BeatModsAPIUrl_wgzeyu;
                    Utils.Constants.TeknikAPIUrl = Utils.Constants.TeknikAPIUrl_wgzeyu;
                    Utils.Constants.BeatModsURL = Utils.Constants.BeatModsURL_wgzeyu;
                    Utils.Constants.BeatModsVersions = Utils.Constants.BeatModsVersions_wgzeyu;
                    Utils.Constants.BeatModsAlias = Utils.Constants.BeatModsAlias_wgzeyu;
                    Utils.Constants.WeebCDNAPIURL = Utils.Constants.WeebCDNAPIURL_wgzeyu;
                }
                else if (ModAssistant.Properties.Settings.Default.DownloadServer == "网易版@BeatMods.top")
                {
                    Utils.Constants.BeatModsAPIUrl = Utils.Constants.BeatModsAPIUrl_bmtop;
                    Utils.Constants.TeknikAPIUrl = Utils.Constants.TeknikAPIUrl_bmtop;
                    Utils.Constants.BeatModsURL = Utils.Constants.BeatModsURL_bmtop;
                    Utils.Constants.BeatModsVersions = Utils.Constants.BeatModsVersions_bmtop;
                    Utils.Constants.BeatModsAlias = Utils.Constants.BeatModsAlias_bmtop;
                    Utils.Constants.WeebCDNAPIURL = Utils.Constants.WeebCDNAPIURL_bmtop;
                }
                else {
                    Utils.Constants.BeatModsAPIUrl = Utils.Constants.BeatModsAPIUrl_beatmods;
                    Utils.Constants.TeknikAPIUrl = Utils.Constants.TeknikAPIUrl_beatmods;
                    Utils.Constants.BeatModsURL = Utils.Constants.BeatModsURL_beatmods;
                    Utils.Constants.BeatModsVersions = Utils.Constants.BeatModsVersions_beatmods;
                    Utils.Constants.BeatModsAlias = Utils.Constants.BeatModsAlias_beatmods;
                    Utils.Constants.WeebCDNAPIURL = Utils.Constants.WeebCDNAPIURL_beatmods;
                }
            }
        }

        public class TeknikPasteResponse
        {
            public Result result;
            public class Result
            {
                public string id;
                public string url;
                public string title;
                public string syntax;
                public DateTime? expiration;
                public string password;
            }
        }

        public class WeebCDNRandomResponse
        {
            public int index;
            public string url;
            public string ext;
        }

        public static void SendNotify(string message, string title = null)
        {
            string defaultTitle = (string)Application.Current.FindResource("Utils:NotificationTitle");

            var notification = new System.Windows.Forms.NotifyIcon()
            {
                Visible = true,
                Icon = System.Drawing.SystemIcons.Information,
                BalloonTipTitle = title ?? defaultTitle,
                BalloonTipText = message
            };

            notification.ShowBalloonTip(5000);

            notification.Dispose();
        }

        public static void StartAsAdmin(string Arguments, bool Close = false)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;
                process.StartInfo.Arguments = Arguments;
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.Verb = "runas";

                try
                {
                    process.Start();

                    if (!Close)
                    {
                        process.WaitForExit();
                    }
                }
                catch
                {
                    MessageBox.Show((string)Application.Current.FindResource("Utils:RunAsAdmin"));
                }

                if (Close) Application.Current.Shutdown();
            }
        }

        public static string CalculateMD5(string filename)
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
            string InstallDir = Properties.Settings.Default.InstallFolder;

            if (!string.IsNullOrEmpty(InstallDir)
                && Directory.Exists(InstallDir)
                && Directory.Exists(Path.Combine(InstallDir, "Beat Saber_Data", "Plugins"))
                && File.Exists(Path.Combine(InstallDir, "Beat Saber.exe")))
            {
                return InstallDir;
            }

            try
            {
                InstallDir = GetSteamDir();
            }
            catch { }
            if (!string.IsNullOrEmpty(InstallDir))
            {
                return InstallDir;
            }

            try
            {
                InstallDir = GetOculusDir();
            }
            catch { }
            if (!string.IsNullOrEmpty(InstallDir))
            {
                return InstallDir;
            }

            try
            {
                InstallDir = GetNetviosDir();
            }
            catch { }
            if (!string.IsNullOrEmpty(InstallDir))
            {
                return InstallDir;
            }

            MessageBox.Show((string)Application.Current.FindResource("Utils:NoInstallFolder"));

            InstallDir = GetManualDir();
            if (!string.IsNullOrEmpty(InstallDir))
            {
                return InstallDir;
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
            Constants.UpdateDownloadNode();
            return directory;
        }

        public static string GetSteamDir()
        {

            string SteamInstall = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)?.OpenSubKey("SOFTWARE")?.OpenSubKey("WOW6432Node")?.OpenSubKey("Valve")?.OpenSubKey("Steam")?.GetValue("InstallPath").ToString();
            if (string.IsNullOrEmpty(SteamInstall))
            {
                SteamInstall = Registry.LocalMachine.OpenSubKey("SOFTWARE")?.OpenSubKey("WOW6432Node")?.OpenSubKey("Valve")?.OpenSubKey("Steam")?.GetValue("InstallPath").ToString();
            }

            if (string.IsNullOrEmpty(SteamInstall)) return null;

            string vdf = Path.Combine(SteamInstall, @"steamapps\libraryfolders.vdf");
            if (!File.Exists(@vdf)) return null;

            Regex regex = new Regex("\\s\"\\d\"\\s+\"(.+)\"");
            List<string> SteamPaths = new List<string>
            {
                Path.Combine(SteamInstall, @"steamapps")
            };

            using (StreamReader reader = new StreamReader(@vdf))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Match match = regex.Match(line);
                    if (match.Success)
                    {
                        SteamPaths.Add(Path.Combine(match.Groups[1].Value.Replace(@"\\", @"\"), @"steamapps"));
                    }
                }
            }

            regex = new Regex("\\s\"installdir\"\\s+\"(.+)\"");
            foreach (string path in SteamPaths)
            {
                if (File.Exists(Path.Combine(@path, @"appmanifest_" + Constants.BeatSaberAPPID + ".acf")))
                {
                    using (StreamReader reader = new StreamReader(Path.Combine(@path, @"appmanifest_" + Constants.BeatSaberAPPID + ".acf")))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            Match match = regex.Match(line);
                            if (match.Success)
                            {
                                if (File.Exists(Path.Combine(@path, @"common", match.Groups[1].Value, "Beat Saber.exe")))
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

        public static string GetVersion()
        {
            string filename = Path.Combine(App.BeatSaberInstallDirectory, "Beat Saber_Data", "globalgamemanagers");
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                byte[] file = File.ReadAllBytes(filename);
                byte[] bytes = new byte[32];

                fs.Read(file, 0, Convert.ToInt32(fs.Length));
                fs.Close();
                int index = Encoding.UTF8.GetString(file).IndexOf("public.app-category.games") + 136;

                Array.Copy(file, index, bytes, 0, 32);
                string version = Encoding.UTF8.GetString(bytes).Trim(Utils.Constants.IllegalCharacters);

                return version;
            }
        }

        public static string GetOculusDir()
        {
            string OculusInstall = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)?.OpenSubKey("SOFTWARE")?.OpenSubKey("Wow6432Node")?.OpenSubKey("Oculus VR, LLC")?.OpenSubKey("Oculus")?.OpenSubKey("Config")?.GetValue("InitialAppLibrary").ToString();
            if (string.IsNullOrEmpty(OculusInstall)) return null;

            if (!string.IsNullOrEmpty(OculusInstall))
            {
                if (File.Exists(Path.Combine(OculusInstall, "Software", "hyperbolic-magnetism-beat-saber", "Beat Saber.exe")))
                {
                    return SetDir(Path.Combine(OculusInstall, "Software", "hyperbolic-magnetism-beat-saber"), "Oculus");
                }
            }

            // Yoinked this code from Umbranox's Mod Manager. Lot's of thanks and love for Umbra <3
            using (RegistryKey librariesKey = Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey("Oculus VR, LLC")?.OpenSubKey("Oculus")?.OpenSubKey("Libraries"))
            {
                // Oculus libraries uses GUID volume paths like this "\\?\Volume{0fea75bf-8ad6-457c-9c24-cbe2396f1096}\Games\Oculus Apps", we need to transform these to "D:\Game"\Oculus Apps"
                WqlObjectQuery wqlQuery = new WqlObjectQuery("SELECT * FROM Win32_Volume");
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(wqlQuery))
                {
                    Dictionary<string, string> guidLetterVolumes = new Dictionary<string, string>();

                    foreach (ManagementBaseObject disk in searcher.Get())
                    {
                        var diskId = ((string)disk.GetPropertyValue("DeviceID")).Substring(11, 36);
                        var diskLetter = ((string)disk.GetPropertyValue("DriveLetter")) + @"\";

                        if (!string.IsNullOrWhiteSpace(diskLetter))
                        {
                            guidLetterVolumes.Add(diskId, diskLetter);
                        }
                    }

                    // Search among the library folders
                    foreach (string libraryKeyName in librariesKey.GetSubKeyNames())
                    {
                        using (RegistryKey libraryKey = librariesKey.OpenSubKey(libraryKeyName))
                        {
                            string libraryPath = (string)libraryKey.GetValue("Path");
                            // Yoinked this code from Megalon's fix. <3
                            string GUIDLetter = guidLetterVolumes.FirstOrDefault(x => libraryPath.Contains(x.Key)).Value;
                            if (!string.IsNullOrEmpty(GUIDLetter))
                            {
                                string finalPath = Path.Combine(GUIDLetter, libraryPath.Substring(49), @"Software\hyperbolic-magnetism-beat-saber");
                                if (File.Exists(Path.Combine(finalPath, "Beat Saber.exe")))
                                {
                                    return SetDir(finalPath, "Oculus");
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static string GetNetviosDir()
        {
            string NetviosInstall = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64)?.OpenSubKey("Software")?.OpenSubKey("NetVios")?.OpenSubKey("NetViosVR")?.OpenSubKey("Games")?.OpenSubKey("VR000004")?.GetValue("INSTALL").ToString();
            string NetviosStart = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64)?.OpenSubKey("Software")?.OpenSubKey("NetVios")?.OpenSubKey("NetViosVR")?.OpenSubKey("Games")?.OpenSubKey("VR000004")?.GetValue("STARTUP_PATH").ToString();
            if (string.IsNullOrEmpty(NetviosInstall) || string.IsNullOrEmpty(NetviosStart)) return null;

            if (!(string.IsNullOrEmpty(NetviosInstall) && string.IsNullOrEmpty(NetviosStart)))
            {
                if (File.Exists(Path.Combine(NetviosInstall, NetviosStart))) 
                {
                    return SetDir(Path.GetDirectoryName(Path.Combine(NetviosInstall, NetviosStart)), "Netvios");
                }
            }

            return null;
        }

        public static string GetManualDir()
        {
            var dialog = new SaveFileDialog()
            {
                Title = (string)Application.Current.FindResource("Utils:InstallDir:DialogTitle"),
                Filter = "Directory|*.this.directory",
                FileName = "select"
            };

            var old_store = Properties.Settings.Default.StoreType;
            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                path = path.Replace("\\select.this.directory", "");
                path = path.Replace(".this.directory", "");
                path = path.Replace("\\select.directory", "");
                if (File.Exists(Path.Combine(path, "Beat Saber.exe")))
                {
                    string store;
                    if (File.Exists(Path.Combine(path, "NetviosSDK.dll"))) {
                        store = "Netvios";
                    }
                    else if (File.Exists(Path.Combine(path, "Beat Saber_Data", "Plugins", "steam_api64.dll")))
                    {
                        store = "Steam";
                    }
                    else
                    {
                        store = "Oculus";
                    }
                    SetDir(path, store);
                }
            }
            if (!old_store.Equals(Properties.Settings.Default.StoreType)) {
                Process.Start(Utils.ExePath, App.Arguments);
                Application.Current.Dispatcher.Invoke(() => { Application.Current.Shutdown(); });
            }
            return null;
        }

        public static string GetManualFile(string filter = "", string title = "Open File")
        {
            var dialog = new OpenFileDialog()
            {
                Title = title,
                Filter = filter,
                Multiselect = false,
            };

            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }
            return null;
        }

        public static bool IsVoid()
        {
            string directory = App.BeatSaberInstallDirectory;

            if (File.Exists(Path.Combine(directory, "IGG-GAMES.COM.url")) ||
                File.Exists(Path.Combine(directory, "SmartSteamEmu.ini")) ||
                File.Exists(Path.Combine(directory, "GAMESTORRENT.CO.url")) ||
                File.Exists(Path.Combine(directory, "Beat Saber_Data", "Plugins", "BSteam crack.dll")) ||
                File.Exists(Path.Combine(directory, "Beat Saber_Data", "Plugins", "HUHUVR_steam_api64.dll")) ||
                Directory.GetFiles(Path.Combine(directory, "Beat Saber_Data", "Plugins"), "*.ini", SearchOption.TopDirectoryOnly).Length > 0)
                return true;
            return false;
        }

        public static byte[] StreamToArray(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static void OpenFolder(string location)
        {
            if (!location.EndsWith(Path.DirectorySeparatorChar.ToString())) location += Path.DirectorySeparatorChar;
            if (Directory.Exists(location))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = location,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                    return;
                }
                catch { }
            }
            MessageBox.Show($"{string.Format((string)Application.Current.FindResource("Utils:CannotOpenFolder"), location)}.");
        }

        public static void Log(string message, string severity = "LOG")
        {
            string path = Path.GetDirectoryName(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath);
            string logFile = $"{path}{Path.DirectorySeparatorChar}log.log";
            File.AppendAllText(logFile, $"[{DateTime.UtcNow.ToString("yyyy-mm-dd HH:mm:ss.ffffff")}][{severity.ToUpper()}] {message}\n");
        }

        public static async Task Download(string link, string output)
        {
            var resp = await HttpClient.GetAsync(link);
            using (var stream = await resp.Content.ReadAsStreamAsync())
            using (var fs = new FileStream(output, FileMode.OpenOrCreate, FileAccess.Write))
            {
                await stream.CopyToAsync(fs);
            }
        }

        private delegate void ShowMessageBoxDelegate(string Message, string Caption);

        private static void ShowMessageBox(string Message, string Caption)
        {
            MessageBox.Show(Message, Caption);
        }

        public static void ShowMessageBoxAsync(string Message, string Caption)
        {
            ShowMessageBoxDelegate caller = new ShowMessageBoxDelegate(ShowMessageBox);
            caller.BeginInvoke(Message, Caption, null, null);
        }

        public static void ShowMessageBoxAsync(string Message)
        {
            ShowMessageBoxDelegate caller = new ShowMessageBoxDelegate(ShowMessageBox);
            caller.BeginInvoke(Message, null, null, null);
        }

        /// <summary>
        /// Attempts to write the specified string to the <see cref="System.Windows.Clipboard"/>.
        /// </summary>
        /// <param name="text">The string to be written</param>
        public static void SetClipboard(string text)
        {
            bool success = false;
            try
            {
                Clipboard.SetText(text);
                success = true;
            }
            catch (Exception)
            {
                // Swallow exceptions relating to writing data to clipboard.
            }

            // This could be placed in the try/catch block but we don't
            // want to suppress exceptions for non-clipboard operations
            if (success)
            {
                Utils.SendNotify($"Copied text to clipboard");
            }
        }
    }
}
