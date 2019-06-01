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
        private const string ModelSaberUrlPrefix = "https://modelsaber.com/files/";
        private const string BeatSaverUrlPrefix = "https://beatsaver.com/download/";

        private static string _beatSaberPath = App.BeatSaberInstallDirectory;

        private const string CustomAvatarsFolder = "CustomAvatars";
        private const string CustomSabersFolder = "CustomSabers";
        private const string CustomPlatformsFolder = "CustomPlatforms";
        private const string CustomSongsFolder = "CustomSongs";

        private static readonly string[] Protocols = new[] { "modelsaber", "beatsaver" };

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
            }
        }

        private static void BeatSaver(Uri uri)
        {
            string id = uri.Host;
            DownloadAsset(BeatSaverUrlPrefix + id, CustomSongsFolder, id + ".zip");
            string directory = Path.Combine(_beatSaberPath, CustomSongsFolder, id);

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

            File.Delete(Path.Combine(_beatSaberPath, CustomSongsFolder, id + ".zip"));
        }

        private static void ModelSaber(Uri uri)
        {
            switch (uri.Host)
            {
                case "avatar":
                    DownloadAsset(ModelSaberUrlPrefix + uri.Host + uri.AbsolutePath, CustomAvatarsFolder);
                    break;
                case "saber":
                    DownloadAsset(ModelSaberUrlPrefix + uri.Host + uri.AbsolutePath, CustomSabersFolder);
                    break;
                case "platform":
                    DownloadAsset(ModelSaberUrlPrefix + uri.Host + uri.AbsolutePath, CustomPlatformsFolder);
                    break;
            }
        }

        private static void DownloadAsset(string link, string folder, string fileName = null)
        {
            if (string.IsNullOrEmpty(_beatSaberPath))
            {
                Utils.SendNotify("Beat Saber installation path not found.");
            }
            try
            {
                Directory.CreateDirectory(Path.Combine(_beatSaberPath, folder));
                if (String.IsNullOrEmpty(fileName))
                    fileName = WebUtility.UrlDecode(Path.Combine(_beatSaberPath, folder, new Uri(link).Segments.Last()));
                else
                    fileName = WebUtility.UrlDecode(Path.Combine(_beatSaberPath, folder, fileName));

                Utils.Download(link, fileName);
                Utils.SendNotify("Installed: " + Path.GetFileNameWithoutExtension(fileName));

            }
            catch
            {
                Utils.SendNotify("Failed to install.");
            }
        }

        public static void Register(string protocol, bool background = false)
        {
            if (IsRegistered(protocol) == true)
                return;
            try
            {
                if (Utils.IsAdmin)
                {
                    RegistryKey protocolKey = Registry.ClassesRoot.OpenSubKey(protocol, true);
                    if (protocolKey == null)
                        protocolKey = Registry.ClassesRoot.CreateSubKey(protocol, true);
                    RegistryKey commandKey = protocolKey.CreateSubKey(@"shell\open\command", true);
                    if (commandKey == null)
                        commandKey = Registry.ClassesRoot.CreateSubKey(@"shell\open\command", true);

                    if (protocolKey.GetValue("OneClick-Provider", "").ToString() != "ModAssistant")
                    {
                        protocolKey.SetValue("URL Protocol", "", RegistryValueKind.String);
                        protocolKey.SetValue("OneClick-Provider", "ModAssistant", RegistryValueKind.String);
                        commandKey.SetValue("", $"\"{Utils.ExePath}\" \"--install\" \"%1\"");
                    }

                    Utils.SendNotify($"{protocol} One Click Install handlers registered!");
                }
                else
                {
                    Utils.StartAsAdmin($"\"--register\" \"{protocol}\"");
                }
            } catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            if (background)
                Application.Current.Shutdown();
            else
                Pages.Options.Instance.UpdateHandlerStatus();
        }

        public static void Unregister(string protocol, bool background = false)
        {
            if (IsRegistered(protocol) == false)
                return;
            try
            {
                if (Utils.IsAdmin)
                {
                    using (RegistryKey protocolKey = Registry.ClassesRoot.OpenSubKey(protocol, true))
                    {
                        if (protocolKey != null
                            && protocolKey.GetValue("OneClick-Provider", "").ToString() == "ModAssistant")
                        {
                            Registry.ClassesRoot.DeleteSubKeyTree(protocol);
                        }
                    }
                    Utils.SendNotify($"{protocol} One Click Install handlers unregistered!");
                }
                else
                {
                    Utils.StartAsAdmin($"\"--unregister\" \"{protocol}\"");
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            if (background)
                Application.Current.Shutdown();
            else
                Pages.Options.Instance.UpdateHandlerStatus();
        }

        public static bool IsRegistered(string protocol)
        {
            RegistryKey protocolKey = Registry.ClassesRoot.OpenSubKey(protocol);
            if (protocolKey != null
                && protocolKey.GetValue("OneClick-Provider", "").ToString() == "ModAssistant")
                return true;
            else
                return false;
        }
    }
}
