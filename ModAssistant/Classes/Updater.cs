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
        private static string APILatestURL = "https://api.github.com/repos/Assistant/ModAssistant/releases/latest";

        private static Update LatestUpdate;
        private static Version CurrentVersion;
        private static Version LatestVersion;
        private static bool NeedsUpdate = false;

        public static bool CheckForUpdate()
        {
            string json = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(APILatestURL);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.UserAgent = "ModAssistant/" + App.Version;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                var serializer = new JavaScriptSerializer();
                LatestUpdate = serializer.Deserialize<Update>(reader.ReadToEnd());
            }
            
            LatestVersion = new Version(LatestUpdate.tag_name.Substring(1));
            CurrentVersion = new Version(App.Version);


            return (LatestVersion > CurrentVersion);
        }

        public static void Run()
        {
            try
            {
                NeedsUpdate = CheckForUpdate();
            }
            catch
            {
                Utils.SendNotify("Couldn't check for updates.");
            }

            if (NeedsUpdate) StartUpdate();
        }

        public static void StartUpdate()
        {
            string Directory = Path.GetDirectoryName(Utils.ExePath);
            string OldExe = Path.Combine(Directory, "ModAssistant.old.exe");

            string DownloadLink = null;

            foreach (Update.Asset asset in LatestUpdate.assets)
            {
                if (asset.name == "ModAssistant.exe")
                {
                    DownloadLink = asset.browser_download_url;
                }
            }

            if (String.IsNullOrEmpty(DownloadLink))
            {
                Utils.SendNotify("Couldn't download update.");
            }
            else
            {
                if (File.Exists(OldExe))
                    File.Delete(OldExe);

                File.Move(Utils.ExePath, OldExe);

                Utils.Download(DownloadLink, Utils.ExePath);
                Process.Start(Utils.ExePath);
                App.Current.Shutdown();

            }

        }

    }

    public class Update
    {
        public string url;
        public string assets_url;
        public string upload_url;
        public string html_url;
        public int id;
        public string node_id;
        public string tag_name;
        public string target_commitish;
        public string name;
        public bool draft;
        public User author;
        public bool prerelease;
        public string created_at;
        public string published_at;
        public Asset[] assets;
        public string tarball_url;
        public string zipball_url;
        public string body;

        public class Asset
        {
            public string url;
            public int id;
            public string node_id;
            public string name;
            public string label;
            public User uploader;
            public string content_type;
            public string state;
            public int size;
            public string created_at;
            public string updated_at;
            public string browser_download_url;
        }

        public class User
        {
            public string login;
            public int id;
            public string node_id;
            public string avatar_url;
            public string gravatar_id;
            public string url;
            public string html_url;
            public string followers_url;
            public string following_url;
            public string gists_url;
            public string starred_url;
            public string subscriptions_url;
            public string organizations_url;
            public string repos_url;
            public string events_url;
            public string received_events_url;
            public string type;
            public bool site_admin;

        }
    }
}
