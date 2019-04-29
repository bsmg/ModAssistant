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

namespace ModAssistant
{
    class OneClickInstaller
    {
        private const string ModelSaberURLPrefix = "https://modelsaber.com/files/";

        private const string CustomAvatarsFolder = "CustomAvatars";
        private const string CustomSabersFolder = "CustomSabers";
        private const string CustomPlatformsFolder = "CustomPlatforms";

        private static readonly string[] Protocols = new[] { "modelsaber" };

        private static bool IsElevated => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        public static void InstallAsset(string link)
        {
            Uri uri = new Uri(link);
            if (!Protocols.Contains(uri.Scheme)) return;

            switch (uri.Scheme)
            {
                case "modelsaber":
                    ModelSaber(uri);
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

        private static void DownloadAsset(string link, string folder)
        {
            string BeatSaberPath = App.BeatSaberInstallDirectory;
            if (string.IsNullOrEmpty(BeatSaberPath))
            {
                Utils.SendNotify("Beat Saber installation path not found.");
            }
            try
            {
                string fileName = WebUtility.UrlDecode(Path.Combine(BeatSaberPath, folder, new Uri(link).Segments.Last()));
                byte[] file = new WebClient().DownloadData(link);
                File.WriteAllBytes(fileName, file);
                Utils.SendNotify("Installed: " + Path.GetFileNameWithoutExtension(fileName));

            }
            catch
            {
                Utils.SendNotify("Failed to install.");
            }
        }

    }
}
