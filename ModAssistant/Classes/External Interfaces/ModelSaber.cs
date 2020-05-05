using System;
using System.Threading.Tasks;

namespace ModAssistant.API
{
    class ModelSaber
    {
        private const string ModelSaberURLPrefix = "https://modelsaber.com/files/";
        private const string CustomAvatarsFolder = "CustomAvatars";
        private const string CustomSabersFolder = "CustomSabers";
        private const string CustomPlatformsFolder = "CustomPlatforms";
        private const string CustomBloqsFolder = "CustomNotes";

        public static async Task GetModel(Uri uri)
        {
            switch (uri.Host)
            {
                case "avatar":
                    await Utils.DownloadAsset(ModelSaberURLPrefix + uri.Host + uri.AbsolutePath, CustomAvatarsFolder);
                    break;
                case "saber":
                    await Utils.DownloadAsset(ModelSaberURLPrefix + uri.Host + uri.AbsolutePath, CustomSabersFolder);
                    break;
                case "platform":
                    await Utils.DownloadAsset(ModelSaberURLPrefix + uri.Host + uri.AbsolutePath, CustomPlatformsFolder);
                    break;
                case "bloq":
                    await Utils.DownloadAsset(ModelSaberURLPrefix + uri.Host + uri.AbsolutePath, CustomBloqsFolder);
                    break;
            }
        }

    }
}
