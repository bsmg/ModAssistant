using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Windows;
using Microsoft.Win32;

namespace ModAssistant.Classes
{
    class OneClickInstaller
    {
        private const string ModelSaberUrlPrefix = "https://modelsaber.com/files/";
        private const string BeatSaverUrlPrefix = "https://beatsaver.com/download/";

        private static readonly string BeatSaberPath = App.BeatSaberInstallDirectory;

        private const string CustomAvatarsFolder = "CustomAvatars";
        private const string CustomSabersFolder = "CustomSabers";
        private const string CustomPlatformsFolder = "CustomPlatforms";
        private const string CustomSongsFolder = "CustomSongs";

        private static readonly string[] Protocols = {"modelsaber", "beatsaver"};

        public static void InstallAsset(string link)
        {
            var uri = new Uri(link);

            if (!Protocols.Contains(uri.Scheme)) return;

            switch (uri.Scheme.ToLower())
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
            var id = uri.Host;
            var directory = Path.Combine(BeatSaberPath, CustomSongsFolder, id);

            DownloadAsset(BeatSaverUrlPrefix + id, CustomSongsFolder, $"{id}.zip");

            using (var stream = new FileStream($"{directory}.zip", FileMode.Open))
            using (var archive = new ZipArchive(stream))
            {
                foreach (var file in archive.Entries)
                {
                    var fileDirectory = Path.GetDirectoryName(Path.Combine(directory, file.FullName));

                    if (!Directory.Exists(fileDirectory))
                        Directory.CreateDirectory(fileDirectory);

                    if (!String.IsNullOrEmpty(file.Name))
                        file.ExtractToFile(Path.Combine(directory, file.FullName), true);
                }
            }

            File.Delete(Path.Combine(BeatSaberPath, CustomSongsFolder, $"{id}.zip"));
        }

        private static void ModelSaber(Uri uri)
        {
            var url = ModelSaberUrlPrefix + uri.Host + uri.AbsolutePath;

            switch (uri.Host)
            {
                case "avatar":
                    DownloadAsset(url, CustomAvatarsFolder);
                    break;
                case "saber":
                    DownloadAsset(url, CustomSabersFolder);
                    break;
                case "platform":
                    DownloadAsset(url, CustomPlatformsFolder);
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
                Directory.CreateDirectory(Path.Combine(BeatSaberPath, folder));
                if (string.IsNullOrEmpty(fileName))
                    fileName = WebUtility.UrlDecode(Path.Combine(BeatSaberPath, folder,
                        new Uri(link).Segments.Last()));
                else
                    fileName = WebUtility.UrlDecode(Path.Combine(BeatSaberPath, folder, fileName));

                Utils.Download(link, fileName);
                Utils.SendNotify($"Installed: {Path.GetFileNameWithoutExtension(fileName)}");
            }
            catch
            {
                Utils.SendNotify("Failed to install.");
            }
        }

        public static void Register(string protocol, bool background = false)
        {
            if (IsRegistered(protocol))
                return;
            try
            {
                if (Utils.IsAdmin)
                {
                    var protocolKey = Registry.ClassesRoot.OpenSubKey(protocol, true) ??
                                      Registry.ClassesRoot.CreateSubKey(protocol, true);

                    var commandKey = protocolKey.CreateSubKey(@"shell\open\command", true) ??
                                     Registry.ClassesRoot.CreateSubKey(@"shell\open\command", true);

                    if (protocolKey.GetValue("OneClick-Provider", string.Empty).ToString() != "ModAssistant")
                    {
                        protocolKey.SetValue("URL Protocol", string.Empty, RegistryValueKind.String);
                        protocolKey.SetValue("OneClick-Provider", "ModAssistant", RegistryValueKind.String);

                        commandKey.SetValue(string.Empty, $"\"{Utils.ExePath}\" \"--install\" \"%1\"");
                    }

                    Utils.SendNotify($"{protocol} One Click Install handlers registered!");
                }
                else
                {
                    Utils.StartAsAdmin($"\"--register\" \"{protocol}\"");
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

        public static void Unregister(string protocol, bool background = false)
        {
            if (!IsRegistered(protocol))
                return;
            try
            {
                if (Utils.IsAdmin)
                {
                    using (var protocolKey = Registry.ClassesRoot.OpenSubKey(protocol, true))
                    {
                        if (protocolKey != null &&
                            protocolKey.GetValue("OneClick-Provider", string.Empty).ToString().Equals("ModAssistant"))
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
            var protocolKey = Registry.ClassesRoot.OpenSubKey(protocol);
            return !(protocolKey is null) &&
                   protocolKey.GetValue("OneClick-Provider", string.Empty).ToString().Equals("ModAssistant");
        }
    }
}