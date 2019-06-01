using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;

namespace ModAssistant
{
    class Updater
    {
        private static string _apiLatestUrl = "https://api.github.com/repos/Assistant/ModAssistant/releases/latest";

        private static Update _latestUpdate;
        private static Version _currentVersion;
        private static Version _latestVersion;
        private static bool _needsUpdate = false;

        public static bool CheckForUpdate()
        {
            string json = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_apiLatestUrl);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.UserAgent = "ModAssistant/" + App.Version;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                var serializer = new JavaScriptSerializer();
                _latestUpdate = serializer.Deserialize<Update>(reader.ReadToEnd());
            }
            
            _latestVersion = new Version(_latestUpdate.TagName.Substring(1));
            _currentVersion = new Version(App.Version);


            return (_latestVersion > _currentVersion);
        }

        public static void Run()
        {
            try
            {
                _needsUpdate = CheckForUpdate();
            }
            catch
            {
                Utils.SendNotify("Couldn't check for updates.");
            }

            if (_needsUpdate) StartUpdate();
        }

        public static void StartUpdate()
        {
            string directory = Path.GetDirectoryName(Utils.ExePath);
            string oldExe = Path.Combine(directory, "ModAssistant.old.exe");

            string downloadLink = null;

            foreach (Update.Asset asset in _latestUpdate.Assets)
            {
                if (asset.Name == "ModAssistant.exe")
                {
                    downloadLink = asset.BrowserDownloadUrl;
                }
            }

            if (String.IsNullOrEmpty(downloadLink))
            {
                Utils.SendNotify("Couldn't download update.");
            }
            else
            {
                if (File.Exists(oldExe))
                    File.Delete(oldExe);

                File.Move(Utils.ExePath, oldExe);

                Utils.Download(downloadLink, Utils.ExePath);
                Process.Start(Utils.ExePath);
                App.Current.Shutdown();

            }

        }

    }

    public class Update
    {
        public string Url;
        public string AssetsUrl;
        public string UploadUrl;
        public string HtmlUrl;
        public int Id;
        public string NodeId;
        public string TagName;
        public string TargetCommitish;
        public string Name;
        public bool Draft;
        public User Author;
        public bool Prerelease;
        public string CreatedAt;
        public string PublishedAt;
        public Asset[] Assets;
        public string TarballUrl;
        public string ZipballUrl;
        public string Body;

        public class Asset
        {
            public string Url;
            public int Id;
            public string NodeId;
            public string Name;
            public string Label;
            public User Uploader;
            public string ContentType;
            public string State;
            public int Size;
            public string CreatedAt;
            public string UpdatedAt;
            public string BrowserDownloadUrl;
        }

        public class User
        {
            public string Login;
            public int Id;
            public string NodeId;
            public string AvatarUrl;
            public string GravatarId;
            public string Url;
            public string HtmlUrl;
            public string FollowersUrl;
            public string FollowingUrl;
            public string GistsUrl;
            public string StarredUrl;
            public string SubscriptionsUrl;
            public string OrganizationsUrl;
            public string ReposUrl;
            public string EventsUrl;
            public string ReceivedEventsUrl;
            public string Type;
            public bool SiteAdmin;

        }
    }
}
