using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace ModAssistant.API
{
    public class Utils
    {
        public static readonly string BeatSaberPath = App.BeatSaberInstallDirectory;

        public static async Task DownloadAsset(string link, string folder, string fileName = null, string displayName = null)
        {
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = Path.GetFileNameWithoutExtension(fileName);
            }
            if (string.IsNullOrEmpty(BeatSaberPath))
            {
                ModAssistant.Utils.SendNotify((string)Application.Current.FindResource("OneClick:InstallDirNotFound"));
            }
            try
            {
                Directory.CreateDirectory(Path.Combine(BeatSaberPath, folder));
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = WebUtility.UrlDecode(Path.Combine(BeatSaberPath, folder, new Uri(link).Segments.Last()));
                }
                else
                {
                    fileName = WebUtility.UrlDecode(Path.Combine(BeatSaberPath, folder, fileName));
                }

                await ModAssistant.Utils.Download(link, fileName);
                ModAssistant.Utils.SendNotify(string.Format((string)Application.Current.FindResource("OneClick:InstalledAsset"), displayName));
            }
            catch
            {
                ModAssistant.Utils.SendNotify((string)Application.Current.FindResource("OneClick:AssetInstallFailed"));
            }
        }

    }
}
