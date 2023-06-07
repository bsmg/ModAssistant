using System;
using System.Threading.Tasks;
using System.Windows;

namespace ModAssistant.API
{
    class ModelSaber
    {
        private static string ModelSaberURLPrefix
        {
            get =>
            Properties.Settings.Default.AssetsDownloadServer == AssetsServer.Default ? ModAssistant.Utils.Constants.ModelSaberURLPrefix_default :
                (Properties.Settings.Default.AssetsDownloadServer == AssetsServer.WGZeyu ? ModAssistant.Utils.Constants.ModelSaberURLPrefix_wgzeyu :
                ModAssistant.Utils.Constants.ModelSaberURLPrefix_beatsaberchina);
        }
        private const string CustomAvatarsFolder = "CustomAvatars";
        private const string CustomSabersFolder = "CustomSabers";
        private const string CustomPlatformsFolder = "CustomPlatforms";
        private const string CustomBloqsFolder = "CustomNotes";

        public static async Task GetModel(Uri uri)
        {
            string proxyURL = ModelSaberURLPrefix;
            bool throughProxy = true;
            bool fallback = false;

            if (Properties.Settings.Default.AssetsDownloadServer == "国内源@WGzeyu" && !ModAssistant.ZeyuCount.checkModelSaberSingle())
            {
                proxyURL = ModAssistant.Utils.Constants.ModelSaberURLPrefix_default;
                Utils.SetMessage($"{string.Format((string)Application.Current.FindResource("OneClick:Fallback"), "默认@default")}");
                throughProxy = false;
                fallback = true;
            }

            switch (uri.Host)
            {
                case "avatar":
                    await Utils.DownloadAsset(proxyURL + uri.Host + uri.AbsolutePath, CustomAvatarsFolder, null, null, true, false, false, throughProxy, fallback);
                    break;
                case "saber":
                    await Utils.DownloadAsset(proxyURL + uri.Host + uri.AbsolutePath, CustomSabersFolder, null, null, true, false, false, throughProxy, fallback);
                    break;
                case "platform":
                    await Utils.DownloadAsset(proxyURL + uri.Host + uri.AbsolutePath, CustomPlatformsFolder, null, null, true, false, false, throughProxy, fallback);
                    break;
                case "bloq":
                    await Utils.DownloadAsset(proxyURL + uri.Host + uri.AbsolutePath, CustomBloqsFolder, null, null, true, false, false, throughProxy, fallback);
                    break;
            }
        }
        public static string proxyURL()
        {
            if (Properties.Settings.Default.AssetsDownloadServer == "国内源@WGzeyu" && !ModAssistant.ZeyuCount.checkModelSaberSingle())
            {
                return ModAssistant.Utils.Constants.ModelSaberURLPrefix_default;
            }

            return ModelSaberURLPrefix;
        }
    }
}
