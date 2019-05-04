using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ModAssistant
{
    class Updater
    {
        private static string APILatestURL = "https://api.github.com/repos/Assistant/ModAssistant/releases/latest";
        private static string ExePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

        public static bool CheckForUpdate()
        {
            return false;
        }

        public static void Run()
        {
        }
    }

    class Update
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
        public Assets[] assets;
        public string tarball_url;
        public string zipball_url;
        public string body;
    }

    class Assets
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

    class User
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
