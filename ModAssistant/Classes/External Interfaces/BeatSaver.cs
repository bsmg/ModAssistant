using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using static ModAssistant.Http;

namespace ModAssistant.API
{
    public class BeatSaver
    {
        private const string BeatSaverURLPrefix = "https://beatsaver.com";
        private static readonly string CustomSongsFolder = Path.Combine("Beat Saber_Data", "CustomLevels");
        private const bool BypassDownloadCounter = false;

        public static async Task<BeatSaverMap> GetFromKey(string Key, bool showNotification = true)
        {
            if (showNotification) OneClickInstaller.Status.Show();
            return await GetMap(Key, "key", showNotification);
        }

        public static async Task<BeatSaverMap> GetFromHash(string Hash, bool showNotification = true)
        {
            if (showNotification) OneClickInstaller.Status.Show();
            return await GetMap(Hash, "hash", showNotification);
        }

        private static async Task<BeatSaverMap> GetMap(string id, string type, bool showNotification)
        {
            string urlSegment;
            switch (type)
            {
                case "hash":
                    urlSegment = "/api/maps/by-hash/";
                    break;
                case "key":
                    urlSegment = "/api/maps/detail/";
                    break;
                default:
                    return null;
            }

            BeatSaverMap map = new BeatSaverMap();
            map.Success = false;
            if (showNotification) Utils.SetMessage($"{string.Format((string)Application.Current.FindResource("OneClick:Installing"), id)}");
            try
            {
                BeatSaverApiResponse beatsaver = await GetResponse(BeatSaverURLPrefix + urlSegment + id);
                if (beatsaver != null && beatsaver.map != null)
                {
                    map.response = beatsaver;
                    map.Name = await InstallMap(beatsaver.map, showNotification);
                    map.Success = true;
                }
            }
            catch (Exception e)
            {
                ModAssistant.Utils.Log($"Failed downloading BeatSaver map: {id} | Error: {e.Message}", "ERROR");
                Utils.SetMessage($"{string.Format((string)Application.Current.FindResource("OneClick:Failed"), (map.Name ?? id))}");
                App.CloseWindowOnFinish = false;
            }
            return map;
        }

        private static async Task<BeatSaverApiResponse> GetResponse(string url, bool showNotification = true, int retries = 3)
        {
            if (retries == 0)
            {
                ModAssistant.Utils.Log($"Max tries reached: Skipping {url}", "ERROR");
                Utils.SetMessage($"{string.Format((string)Application.Current.FindResource("OneClick:RatelimitSkip"), url)}");
                App.CloseWindowOnFinish = false;
                throw new Exception("Max retries allowed");
            }

            BeatSaverApiResponse response = new BeatSaverApiResponse();
            try
            {
                var resp = await HttpClient.GetAsync(url);
                response.statusCode = resp.StatusCode;
                response.ratelimit = GetRatelimit(resp.Headers);
                string body = await resp.Content.ReadAsStringAsync();

                if ((int)resp.StatusCode == 429)
                {
                    Utils.SetMessage($"{string.Format((string)Application.Current.FindResource("OneClick:RatelimitHit"), response.ratelimit.ResetTime)}");
                    await response.ratelimit.Wait();
                    return await GetResponse(url, showNotification, retries - 1);
                }

                if (response.statusCode == HttpStatusCode.OK)
                {
                    response.map = JsonSerializer.Deserialize<BeatSaverApiResponseMap>(body);
                    return response;
                }
                else
                {
                    Utils.SetMessage($"{string.Format((string)Application.Current.FindResource("OneClick:Failed"), url)}");
                    App.CloseWindowOnFinish = false;
                    return response;
                }
            }
            catch (Exception e)
            {
                if (showNotification)
                {
                    MessageBox.Show($"{Application.Current.FindResource("OneClick:MapDownloadFailed")}\n\n" + e);
                }
                return null;
            }
        }

        public static async Task<string> InstallMap(BeatSaverApiResponseMap Map, bool showNotification = true)
        {
            string zip = Path.Combine(Utils.BeatSaberPath, CustomSongsFolder, Map.hash) + ".zip";
            string mapName = string.Concat(($"{Map.key} ({Map.metadata.songName} - {Map.metadata.levelAuthorName})")
                             .Split(ModAssistant.Utils.Constants.IllegalCharacters));
            string directory = Path.Combine(Utils.BeatSaberPath, CustomSongsFolder, mapName);

#pragma warning disable CS0162 // Unreachable code detected
            if (BypassDownloadCounter)
            {
                await Utils.DownloadAsset(BeatSaverURLPrefix + Map.directDownload, CustomSongsFolder, Map.hash + ".zip", mapName, showNotification, true);
            }
            else
            {
                await Utils.DownloadAsset(BeatSaverURLPrefix + Map.downloadURL, CustomSongsFolder, Map.hash + ".zip", mapName, showNotification, true);
            }
#pragma warning restore CS0162 // Unreachable code detected

            if (File.Exists(zip))
            {
                string mimeType = MimeMapping.GetMimeMapping(zip);

                if (!mimeType.StartsWith("application/x-zip"))
                {
                    ModAssistant.Utils.Log($"Failed extracting BeatSaver map: {zip} \n| Content: {string.Join("\n", File.ReadAllLines(zip))}", "ERROR");
                    throw new Exception("File not a zip.");
                }

                try
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
                }
                catch (Exception e)
                {
                    File.Delete(zip);
                    ModAssistant.Utils.Log($"Failed extracting BeatSaver map: {zip} | Error: {e} \n| Content: {string.Join("\n", File.ReadAllLines(zip))}", "ERROR");
                    throw new Exception("File extraction failed.");
                }
                File.Delete(zip);
            }
            else
            {
                if (showNotification)
                {
                    string line1 = (string)Application.Current.FindResource("OneClick:SongDownload:Failed");
                    string line2 = (string)Application.Current.FindResource("OneClick:SongDownload:NetworkIssues");
                    string title = (string)Application.Current.FindResource("OneClick:SongDownload:FailedTitle");
                    MessageBox.Show($"{line1}\n{line2}", title);
                }
                throw new Exception("Zip file not found.");
            }
            return mapName;
        }

        public static BeatSaverRatelimit GetRatelimit(HttpResponseHeaders headers)
        {
            BeatSaverRatelimit ratelimit = new BeatSaverRatelimit();


            if (headers.TryGetValues("Rate-Limit-Remaining", out IEnumerable<string> _remaining))
            {
                var Remaining = _remaining.GetEnumerator();
                Remaining.MoveNext();
                ratelimit.Remaining = Int32.Parse(Remaining.Current);
                Remaining.Dispose();
            }

            if (headers.TryGetValues("Rate-Limit-Reset", out IEnumerable<string> _reset))
            {
                var Reset = _reset.GetEnumerator();
                Reset.MoveNext();
                ratelimit.Reset = Int32.Parse(Reset.Current);
                ratelimit.ResetTime = UnixTimestampToDateTime((long)ratelimit.Reset);
                Reset.Dispose();
            }

            if (headers.TryGetValues("Rate-Limit-Total", out IEnumerable<string> _total))
            {
                var Total = _total.GetEnumerator();
                Total.MoveNext();
                ratelimit.Total = Int32.Parse(Total.Current);
                Total.Dispose();
            }

            return ratelimit;
        }

        public static DateTime UnixTimestampToDateTime(double unixTime)
        {
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            long unixTimeStampInTicks = (long)(unixTime * TimeSpan.TicksPerSecond);
            return new DateTime(unixStart.Ticks + unixTimeStampInTicks, System.DateTimeKind.Utc);
        }

        public static async Task Download(string url, string output, int retries = 3)
        {
            if (retries == 0)
            {
                Utils.SetMessage($"{string.Format((string)Application.Current.FindResource("OneClick:RatelimitSkip"), url)}");
                App.CloseWindowOnFinish = false;
                ModAssistant.Utils.Log($"Max tries reached: Couldn't download {url}", "ERROR");
                throw new Exception("Max retries allowed");
            }

            var resp = await HttpClient.GetAsync(url);

            if ((int)resp.StatusCode == 429)
            {
                var ratelimit = new BeatSaver.BeatSaverRatelimit();
                ratelimit = GetRatelimit(resp.Headers);
                Utils.SetMessage($"{string.Format((string)Application.Current.FindResource("OneClick:RatelimitHit"), ratelimit.ResetTime)}");
                await ratelimit.Wait();
                await Download(url, output, retries - 1);
            }

            using (var stream = await resp.Content.ReadAsStreamAsync())
            using (var fs = new FileStream(output, FileMode.OpenOrCreate, FileAccess.Write))
            {
                await stream.CopyToAsync(fs);
            }
        }

        public class BeatSaverMap
        {
            public BeatSaverApiResponse response { get; set; }
            public bool Success { get; set; }
            public string Name { get; set; }
        }

        public class BeatSaverApiResponse
        {
            public HttpStatusCode statusCode { get; set; }
            public BeatSaverRatelimit ratelimit { get; set;}
            public BeatSaverApiResponseMap map { get; set; }
        }

        public class BeatSaverRatelimit
        {
            public int? Remaining { get; set; }
            public int? Total { get; set; }
            public int? Reset { get; set; }
            public DateTime ResetTime { get; set; }
            public async Task Wait()
            {
                await Task.Delay(new TimeSpan(ResetTime.Ticks - DateTime.Now.Ticks));
            }
        }

        public class BeatSaverApiResponseMap
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
