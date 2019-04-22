using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModAssistant
{
    class OneClickInstaller
    {
        private const string ModelSaberURLPrefix = "https://modelsaber.com/files";

        private const string CustomAvatarsFolder = "CustomAvatars";
        private const string CustomSabersFolder = "CustomSabers";
        private const string CustomPlatformsFolder = "CustomPlatforms";

        private static readonly string[] Protocols = new[] { "modsaber" };

        public static void InstallAsset(string link)
        {
            Uri uri = new Uri(link);
            if (!Protocols.Contains(uri.Scheme)) return;



        }

        private static bool DownloadAsset(string link, string folder)
        {
            string BeatSaberPath = "";
            if (string.IsNullOrEmpty(BeatSaberPath))
            {
                Utils.SendNotify("Beat Saber installation path not found.");
                return false;
            }
            return false;
        }


    }
}
