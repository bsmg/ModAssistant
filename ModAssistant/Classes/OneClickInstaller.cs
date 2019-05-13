using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Security.Principal;
using Microsoft.Win32;
using System.IO.Compression;

namespace ModAssistant
{
    class OneClickInstaller
    {
        private const string ModelSaberURLPrefix = "https://modelsaber.com/files/";
        private const string BeatSaverURLPrefix = "https://beatsaver.com/download/";

        private static string BeatSaberPath = App.BeatSaberInstallDirectory;

        private const string CustomAvatarsFolder = "CustomAvatars";
        private const string CustomSabersFolder = "CustomSabers";
        private const string CustomPlatformsFolder = "CustomPlatforms";
        private const string CustomSongsFolder = "CustomSongs";

        private static readonly string[] Protocols = new[] { "modelsaber", "beatsaver", "modsaber" };

        public static void InstallAsset(string link)
        {
            Uri uri = new Uri(link);
            if (!Protocols.Contains(uri.Scheme)) return;

            switch (uri.Scheme)
            {
                case "modelsaber":
                    ModelSaber(uri);
                    break;
                case "beatsaver":
                    BeatSaver(uri);
                    break;
                case "modsaber":
                    ModSaber(uri);
                    break;
            }
        }

        private static void BeatSaver(Uri uri)
        {
            string ID = uri.Segments.Last<string>();
            DownloadAsset(BeatSaverURLPrefix + ID, CustomSongsFolder, ID + ".zip");
            string directory = Path.Combine(BeatSaberPath, CustomSongsFolder, ID);

            using (FileStream stream = new FileStream(directory + ".zip", FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(stream))
                {
                    foreach (ZipArchiveEntry file in archive.Entries)
                    {
                        string fileDirectory = Path.GetDirectoryName(Path.Combine(directory, file.FullName));
                        if (!Directory.Exists(fileDirectory))
                            Directory.CreateDirectory(fileDirectory);

                        if (!String.IsNullOrEmpty(file.Name))
                            file.ExtractToFile(Path.Combine(directory, file.FullName), true);
                    }
                }
            }

            File.Delete(Path.Combine(BeatSaberPath, CustomSongsFolder, ID + ".zip"));
        }

        private static void ModSaber(Uri uri)
        {
            switch (uri.Host)
            {
                case "song":
                    string ID = uri.Segments.Last<string>();
                    DownloadAsset(BeatSaverURLPrefix + ID, CustomSongsFolder, ID + ".zip");
                    string directory = Path.Combine(BeatSaberPath, CustomSongsFolder, ID);

                    using (FileStream stream = new FileStream(directory + ".zip", FileMode.Open))
                    {
                        using (ZipArchive archive = new ZipArchive(stream))
                        {
                            foreach (ZipArchiveEntry file in archive.Entries)
                            {
                                string fileDirectory = Path.GetDirectoryName(Path.Combine(directory, file.FullName));
                                if (!Directory.Exists(fileDirectory))
                                    Directory.CreateDirectory(fileDirectory);

                                if (!String.IsNullOrEmpty(file.Name))
                                    file.ExtractToFile(Path.Combine(directory, file.FullName), true);
                            }
                        }
                    }

                    File.Delete(Path.Combine(BeatSaberPath, CustomSongsFolder, ID + ".zip"));
                    break;
            }
        }

        private static void ModelSaber(Uri uri)
        {
            switch (uri.Host)
            {
                case "avatar":
                    DownloadAsset(ModelSaberURLPrefix + uri.Host + uri.AbsolutePath, CustomAvatarsFolder);
                    break;
                case "saber":
                    DownloadAsset(ModelSaberURLPrefix + uri.Host + uri.AbsolutePath, CustomSabersFolder);
                    break;
                case "platform":
                    DownloadAsset(ModelSaberURLPrefix + uri.Host + uri.AbsolutePath, CustomPlatformsFolder);
                    break;
            }
        }

        private static void DownloadAsset(string link, string folder, string fileName = null)
        {
            if (string.IsNullOrEmpty(BeatSaberPath))
            {
                Utils.SendNotify("Beat Saber installation path not found.");
            }
            try
            {
                if (String.IsNullOrEmpty(fileName))
                    fileName = WebUtility.UrlDecode(Path.Combine(BeatSaberPath, folder, new Uri(link).Segments.Last()));
                else
                    fileName = WebUtility.UrlDecode(Path.Combine(BeatSaberPath, folder, fileName));

                Utils.Download(link, fileName);
                Utils.SendNotify("Installed: " + Path.GetFileNameWithoutExtension(fileName));

            }
            catch
            {
                Utils.SendNotify("Failed to install.");
            }
        }

        public static void Register(string Protocol, bool Background = false)
        {
            if (IsRegistered(Protocol) == true)
                return;
            try
            {
                if (Utils.IsAdmin)
                {
                    RegistryKey ProtocolKey = Registry.ClassesRoot.OpenSubKey(Protocol, true);
                    if (ProtocolKey == null)
                        ProtocolKey = Registry.ClassesRoot.CreateSubKey(Protocol, true);
                    RegistryKey CommandKey = ProtocolKey.CreateSubKey(@"shell\open\command", true);
                    if (CommandKey == null)
                        CommandKey = Registry.ClassesRoot.CreateSubKey(@"shell\open\command", true);

                    if (ProtocolKey.GetValue("OneClick-Provider", "").ToString() != "ModAssistant")
                    {
                        ProtocolKey.SetValue("URL Protocol", "", RegistryValueKind.String);
                        ProtocolKey.SetValue("OneClick-Provider", "ModAssistant", RegistryValueKind.String);
                        CommandKey.SetValue("", $"\"{Utils.ExePath}\" \"--install\" \"%1\"");
                    }

                    Utils.SendNotify($"{Protocol} One Click Install handlers registered!");
                }
                else
                {
                    Utils.StartAsAdmin($"\"--register\" \"{Protocol}\"");
                }
            } catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            if (Background)
                Application.Current.Shutdown();
            else
                Pages.Options.Instance.UpdateHandlerStatus();
        }

        public static void Unregister(string Protocol, bool Background = false)
        {
            if (IsRegistered(Protocol) == false)
                return;
            try
            {
                if (Utils.IsAdmin)
                {
                    using (RegistryKey ProtocolKey = Registry.ClassesRoot.OpenSubKey(Protocol, true))
                    {
                        if (ProtocolKey != null
                            && ProtocolKey.GetValue("OneClick-Provider", "").ToString() == "ModAssistant")
                        {
                            Registry.ClassesRoot.DeleteSubKeyTree(Protocol);
                        }
                    }
                    Utils.SendNotify($"{Protocol} One Click Install handlers unregistered!");
                }
                else
                {
                    Utils.StartAsAdmin($"\"--unregister\" \"{Protocol}\"");
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            if (Background)
                Application.Current.Shutdown();
            else
                Pages.Options.Instance.UpdateHandlerStatus();
        }

        public static bool IsRegistered(string Protocol)
        {
            RegistryKey ProtocolKey = Registry.ClassesRoot.OpenSubKey(Protocol);
            if (ProtocolKey != null
                && ProtocolKey.GetValue("OneClick-Provider", "").ToString() == "ModAssistant")
                return true;
            else
                return false;
        }
    }
}
