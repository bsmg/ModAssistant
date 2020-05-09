using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows;
using static ModAssistant.Http;

namespace ModAssistant.API
{
    public class BeatSaver
    {
        private const string BeatSaverURLPrefix = "https://beatsaver.com";
        private static readonly string CustomSongsFolder = Path.Combine("Beat Saber_Data", "CustomLevels");
        private const bool BypassDownloadCounter = false;

        public static async Task GetFromKey(string Key, bool showNotification = true)
        {
            BeatSaverApiResponse Map = await GetResponse(BeatSaverURLPrefix + "/api/maps/detail/" + Key);
            await InstallMap(Map, showNotification);
        }

        public static async Task GetFromHash(string Hash, bool showNotification = true)
        {
            BeatSaverApiResponse Map = await GetResponse(BeatSaverURLPrefix + "/api/maps/by-hash/" + Hash);
            await InstallMap(Map, showNotification);
        }

        private static async Task<BeatSaverApiResponse> GetResponse(string url)
        {
            try
            {
                var resp = await HttpClient.GetAsync(url);
                var body = await resp.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<BeatSaverApiResponse>(body);
            }
            catch (Exception e)
            {
                MessageBox.Show($"{Application.Current.FindResource("OneClick:MapDownloadFailed")}\n\n" + e);
                return null;
            }
        }

        public static async Task InstallMap(BeatSaverApiResponse Map, bool showNotification = true)
        {
            string zip = Path.Combine(Utils.BeatSaberPath, CustomSongsFolder, Map.hash) + ".zip";
            string mapName = string.Concat(($"{Map.key} ({Map.metadata.songName} - {Map.metadata.levelAuthorName})")
                             .Split(ModAssistant.Utils.Constants.IllegalCharacters));
            string directory = Path.Combine(Utils.BeatSaberPath, CustomSongsFolder, mapName);

#pragma warning disable CS0162 // Unreachable code detected
            if (BypassDownloadCounter)
            {
                await Utils.DownloadAsset(BeatSaverURLPrefix + Map.directDownload, CustomSongsFolder, Map.hash + ".zip", mapName, showNotification);
            }
            else
            {
                await Utils.DownloadAsset(BeatSaverURLPrefix + Map.downloadURL, CustomSongsFolder, Map.hash + ".zip", mapName, showNotification);
            }
#pragma warning restore CS0162 // Unreachable code detected

            if (File.Exists(zip))
            {
                using (FileStream stream = new FileStream(zip, FileMode.Open))
                using (ZipArchive archive = new ZipArchive(stream))
                {
                    foreach (ZipArchiveEntry file in archive.Entries)
                    {
                        string fileDirectory = Path.GetDirectoryName(Path.Combine(directory, file.FullName));
                        if (!Directory.Exists(fileDirectory))
                        {
                            Directory.CreateDirectory(fileDirectory);
                        }

                        if (!string.IsNullOrEmpty(file.Name))
                        {
                            file.ExtractToFile(Path.Combine(directory, file.FullName), true);
                        }
                    }
                }

                File.Delete(zip);
            }
            else
            {
                string line1 = (string)Application.Current.FindResource("OneClick:SongDownload:Failed");
                string line2 = (string)Application.Current.FindResource("OneClick:SongDownload:NetworkIssues");
                string title = (string)Application.Current.FindResource("OneClick:SongDownload:FailedTitle");
                MessageBox.Show($"{line1}\n{line2}", title);
            }
        }

        public class BeatSaverApiResponse
        {
            public Metadata metadata { get; set; }
            public Stats stats { get; set; }
            public string description { get; set; }
            public DateTime? deletedAt { get; set; }
            public string _id { get; set; }
            public string key { get; set; }
            public string name { get; set; }
            public Uploader uploader { get; set; }
            public DateTime uploaded { get; set; }
            public string hash { get; set; }
            public string directDownload { get; set; }
            public string downloadURL { get; set; }
            public string coverURL { get; set; }

            public class Difficulties
            {
                public bool easy { get; set; }
                public bool normal { get; set; }
                public bool hard { get; set; }
                public bool expert { get; set; }
                public bool expertPlus { get; set; }
            }

            public class Metadata
            {
                public Difficulties difficulties { get; set; }
                public Characteristic[] characteristics { get; set; }
                public double duration { get; set; }
                public string songName { get; set; }
                public string songSubName { get; set; }
                public string songAuthorName { get; set; }
                public string levelAuthorName { get; set; }
                public double bpm { get; set; }
            }

            public class Characteristic
            {
                public string name { get; set; }
                public CharacteristicDifficulties difficulties { get; set; }
            }

            public class CharacteristicDifficulties
            {
                public Difficulty easy { get; set; }
                public Difficulty normal { get; set; }
                public Difficulty hard { get; set; }
                public Difficulty expert { get; set; }
                public Difficulty expertPlus { get; set; }
            }

            public class Difficulty
            {
                public double? duration { get; set; }
                public double? length { get; set; }
                public double bombs { get; set; }
                public double notes { get; set; }
                public double obstacles { get; set; }
                public double njs { get; set; }
                public double njsOffset { get; set; }
            }

            public class Stats
            {
                public int downloads { get; set; }
                public int plays { get; set; }
                public int downVotes { get; set; }
                public int upVotes { get; set; }
                public double heat { get; set; }
                public double rating { get; set; }
            }

            public class Uploader
            {
                public string _id { get; set; }
                public string username { get; set; }
            }
        }
    }
}
