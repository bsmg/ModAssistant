using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using static ModAssistant.Http;

namespace ModAssistant.API
{
    public class Utils
    {
        public static readonly string BeatSaberPath = App.BeatSaberInstallDirectory;

        public static void SetMessage(string message)
        {
            if (App.window == null)
            {
                OneClickStatus.Instance.MainText = message;
            }
            else
            {
                MainWindow.Instance.MainText = message;
            }
        }

        public static async Task DownloadAsset(string link, string folder, bool showNotifcation, string fileName = null)
        {
            await DownloadAsset(link, folder, fileName, null, showNotifcation);
        }

        public static async Task DownloadAsset(string link, string folder, string fileName = null, string displayName = null)
        {
            await DownloadAsset(link, folder, fileName, displayName, true);
        }

        public static async Task DownloadAsset(string link, string folder, string fileName, string displayName, bool showNotification, bool beatsaver = false)
        {
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
                if (string.IsNullOrEmpty(displayName))
                {
                    displayName = Path.GetFileNameWithoutExtension(fileName);
                }

                if (beatsaver) await BeatSaver.Download(link, fileName);
                else await ModAssistant.Utils.Download(link, fileName);

                if (showNotification)
                {
                    SetMessage(string.Format((string)Application.Current.FindResource("OneClick:InstalledAsset"), displayName));
                }
            }
            catch
            {
                SetMessage((string)Application.Current.FindResource("OneClick:AssetInstallFailed"));
                App.CloseWindowOnFinish = false;
            }
        }
    }
}
