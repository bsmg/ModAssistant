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

        public static void SetMessage(string message)
        {
            if (App.OCIWindow != "No")
            {
                if (App.window == null)
                {
                    if (App.OCIWindow == "No") OneClickStatus.Instance = null;
                    if (OneClickStatus.Instance == null) return;

                    OneClickStatus.Instance.MainText = message;
                }
                else
                {
                    MainWindow.Instance.MainText = message;
                }
            }
        }

        public static async Task<string> DownloadAsset(string link, string folder, string fileName = null, string displayName = null, bool showNotification = true, bool beatsaver = false, bool preferContentDisposition = false)
        {
            if (string.IsNullOrEmpty(BeatSaberPath))
            {
                ModAssistant.Utils.SendNotify((string)Application.Current.FindResource("OneClick:InstallDirNotFound"));
            }
            try
            {
                var parentDir = Path.Combine(BeatSaberPath, folder);
                Directory.CreateDirectory(parentDir);

                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = new Uri(link).Segments.Last();
                }

                if (beatsaver)
                {
                    fileName = WebUtility.UrlDecode(Path.Combine(parentDir, fileName));
                    await BeatSaver.Download(link, fileName);
                }
                else
                {
                    fileName = await ModAssistant.Utils.Download(link, parentDir, fileName, preferContentDisposition);
                }

                if (string.IsNullOrEmpty(displayName))
                {
                    displayName = Path.GetFileNameWithoutExtension(fileName);
                }

                if (showNotification)
                {
                    SetMessage(string.Format((string)Application.Current.FindResource("OneClick:InstalledAsset"), displayName));
                }

                return fileName;
            }
            catch
            {
                SetMessage((string)Application.Current.FindResource("OneClick:AssetInstallFailed"));
                App.CloseWindowOnFinish = false;
                return null;
            }
        }
    }
}
