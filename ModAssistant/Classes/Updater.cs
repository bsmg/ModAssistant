using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;
using System.Windows;

namespace ModAssistant.Classes
{
    class Updater
    {
        private static string _apiLatestUrl = "https://api.github.com/repos/Assistant/ModAssistant/releases/latest";

        private static Update _latestUpdate;
        private static Version _currentVersion;
        private static Version _latestVersion;
        private static bool _needsUpdate = false;

        private static bool CheckForUpdate()
        {
            var request = (HttpWebRequest) WebRequest.Create(_apiLatestUrl);

            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.UserAgent = "ModAssistant/" + App.Version;

            using (var response = (HttpWebResponse) request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
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

        private static void StartUpdate()
        {
            var directory = Path.GetDirectoryName(Utils.ExePath);
            var oldExe = Path.Combine(directory, "ModAssistant.old.exe");

            string downloadLink = null;

            foreach (var asset in _latestUpdate.Assets)
            {
                if (asset.Name.Equals("ModAssistant.exe"))
                {
                    downloadLink = asset.BrowserDownloadUrl;
                }
            }

            if (string.IsNullOrEmpty(downloadLink))
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
                Application.Current.Shutdown();
            }
        }
    }

    public abstract class Update
    {
        public string Url { get; set; }
        public string AssetsUrl { get; set; }
        public string UploadUrl { get; set; }
        public string HtmlUrl { get; set; }
        public int Id { get; set; }
        public string NodeId { get; set; }
        public string TagName { get; set; }
        public string TargetCommitish { get; set; }
        public string Name { get; set; }
        public bool Draft { get; set; }
        public User Author { get; set; }
        public bool Prerelease { get; set; }
        public string CreatedAt { get; set; }
        public string PublishedAt { get; set; }
        public Asset[] Assets { get; set; }
        public string TarballUrl { get; set; }
        public string ZipballUrl { get; set; }
        public string Body { get; set; }

        public abstract class Asset
        {
            public string Url { get; set; }
            public int Id { get; set; }
            public string NodeId { get; set; }
            public string Name { get; set; }
            public string Label { get; set; }
            public User Uploader { get; set; }
            public string ContentType { get; set; }
            public string State { get; set; }
            public int Size { get; set; }
            public string CreatedAt { get; set; }
            public string UpdatedAt { get; set; }
            public string BrowserDownloadUrl { get; set; }
        }

        public abstract class User
        {
            public string Login { get; set; }
            public int Id { get; set; }
            public string NodeId { get; set; }
            public string AvatarUrl { get; set; }
            public string GravatarId { get; set; }
            public string Url { get; set; }
            public string HtmlUrl { get; set; }
            public string FollowersUrl { get; set; }
            public string FollowingUrl { get; set; }
            public string GistsUrl { get; set; }
            public string StarredUrl { get; set; }
            public string SubscriptionsUrl { get; set; }
            public string OrganizationsUrl { get; set; }
            public string ReposUrl { get; set; }
            public string EventsUrl { get; set; }
            public string ReceivedEventsUrl { get; set; }
            public string Type { get; set; }
            public bool SiteAdmin { get; set; }
        }
    }
}