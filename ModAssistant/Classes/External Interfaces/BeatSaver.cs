using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using static ModAssistant.Http;

namespace ModAssistant.API
{
    public class BeatSaver
    {
        private const string BeatSaverURLPrefix = "https://api.beatsaver.com";
        private static readonly string CustomSongsFolder = Path.Combine("Beat Saber_Data", "CustomLevels");
        private static readonly string CustomWIPSongsFolder = Path.Combine("Beat Saber_Data", "CustomWIPLevels");
        private const bool BypassDownloadCounter = false;

        public static async Task<BeatSaverMap> GetFromKey(string Key, bool showNotification = true)
        {
            if (showNotification && App.OCIWindow != "No") OneClickInstaller.Status.Show();
            return await GetMap(Key, "key", showNotification);
        }

        public static async Task<BeatSaverMap> GetFromHash(string Hash, bool showNotification = true)
        {
            if (showNotification && App.OCIWindow != "No") OneClickInstaller.Status.Show();
            return await GetMap(Hash, "hash", showNotification);
        }

        private static async Task<BeatSaverMap> GetMap(string id, string type, bool showNotification)
        {
            string urlSegment;
            switch (type)
            {
                case "hash":
                    urlSegment = "/maps/hash/";
                    break;
                case "key":
                    urlSegment = "/maps/id/";
                    break;
                default:
                    return null;
            }

            BeatSaverMap map = new BeatSaverMap
            {
                Success = false
            };

            if (showNotification) Utils.SetMessage($"{string.Format((string)Application.Current.FindResource("OneClick:Installing"), id)}");
            try
            {
                BeatSaverApiResponse beatsaver = await GetResponse(BeatSaverURLPrefix + urlSegment + id);
                if (beatsaver != null && beatsaver.map != null)
                {
                    map.response = beatsaver;
                    if (type == "hash")
                    {
                        map.HashToDownload = id.ToLowerInvariant();
                    }
                    else
                    {
                        BeatSaverApiResponseMap.BeatsaverMapVersion mapVersion = null;
                        foreach (var version in map.response.map.versions)
                        {
                            if (mapVersion == null || version.createdAt > mapVersion.createdAt) mapVersion = version;
                        }
                        map.HashToDownload = mapVersion.hash;
                    }
                    map.Name = await InstallMap(map, showNotification);
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
                    Utils.SetMessage($"{string.Format((string)Application.Current.FindResource("OneClick:RatelimitHit"), response.ratelimit.ResetTime.ToLocalTime())}");
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

        public static async Task<string> InstallMap(BeatSaverMap Map, bool showNotification = true)
        {
            BeatSaverApiResponseMap responseMap = Map.response.map;
            BeatSaverApiResponseMap.BeatsaverMapVersion mapVersion = responseMap.versions.Where(r => r.hash == Map.HashToDownload).First();
            if (mapVersion == null)
            {
                throw new Exception("Could not find map version.");
            }

            string state = responseMap.versions[0].state;
            string targetSongDirectory = state.Equals("Published") ? CustomSongsFolder : CustomWIPSongsFolder;

            string zip = Path.Combine(Utils.BeatSaberPath, targetSongDirectory, Map.HashToDownload) + ".zip";
            string mapName = string.Concat(($"{responseMap.id} ({responseMap.metadata.songName} - {responseMap.metadata.levelAuthorName})")
                             .Split(ModAssistant.Utils.Constants.IllegalCharacters));

            string directory = Path.Combine(Utils.BeatSaberPath, targetSongDirectory, mapName);

#pragma warning disable CS0162 // Unreachable code detected
            if (BypassDownloadCounter)
            {
                await Utils.DownloadAsset(mapVersion.downloadURL, targetSongDirectory, Map.HashToDownload + ".zip", mapName, showNotification, true);
            }
            else
            {
                await Utils.DownloadAsset(mapVersion.downloadURL, targetSongDirectory, Map.HashToDownload + ".zip", mapName, showNotification, true);
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
                ratelimit.Remaining = int.Parse(Remaining.Current);
                Remaining.Dispose();
            }

            if (headers.TryGetValues("Rate-Limit-Reset", out IEnumerable<string> _reset))
            {
                var Reset = _reset.GetEnumerator();
                Reset.MoveNext();
                ratelimit.Reset = int.Parse(Reset.Current);
                ratelimit.ResetTime = UnixTimestampToDateTime((long)ratelimit.Reset);
                Reset.Dispose();
            }

            if (headers.TryGetValues("Rate-Limit-Total", out IEnumerable<string> _total))
            {
                var Total = _total.GetEnumerator();
                Total.MoveNext();
                ratelimit.Total = int.Parse(Total.Current);
                Total.Dispose();
            }

            return ratelimit;
        }

        public static DateTime UnixTimestampToDateTime(double unixTime)
        {
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long unixTimeStampInTicks = (long)(unixTime * TimeSpan.TicksPerSecond);
            return new DateTime(unixStart.Ticks + unixTimeStampInTicks, DateTimeKind.Utc);
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
                var ratelimit = GetRatelimit(resp.Headers);
                Utils.SetMessage($"{string.Format((string)Application.Current.FindResource("OneClick:RatelimitHit"), ratelimit.ResetTime.ToLocalTime())}");

                await ratelimit.Wait();
                await Download(url, output, retries - 1);
            }

            using (var stream = await resp.Content.ReadAsStreamAsync())
            using (var fs = new FileStream(output, FileMode.OpenOrCreate, FileAccess.Write))
            {
                await stream.CopyToAsync(fs);
            }
        }

#pragma warning disable IDE1006 // Naming Styles
        public class BeatSaverMap
        {
            public BeatSaverApiResponse response { get; set; }
            public bool Success { get; set; }
            public string Name { get; set; }
            public string HashToDownload { get; set; }
        }

        public class BeatSaverApiResponse
        {
            public HttpStatusCode statusCode { get; set; }
            public BeatSaverRatelimit ratelimit { get; set; }
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
                await Task.Delay(new TimeSpan(Math.Max(ResetTime.Ticks - DateTime.UtcNow.Ticks, 0)));
            }
        }

        public class BeatSaverApiResponseMap
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public Uploader uploader { get; set; }
            public Metadata metadata { get; set; }
            public Stats stats { get; set; }
            public DateTime uploaded { get; set; }
            public bool automapper { get; set; }
            public bool ranked { get; set; }
            public bool qualified { get; set; }
            public BeatsaverMapVersion[] versions { get; set; }

            public class Metadata
            {
                public double bpm { get; set; }
                public int duration { get; set; }
                public string songName { get; set; }
                public string songSubName { get; set; }
                public string songAuthorName { get; set; }
                public string levelAuthorName { get; set; }
            }

            public class Uploader
            {
                public int id { get; set; }
                public string name { get; set; }
                public string hash { get; set; }
                public string avatar { get; set; }
            }

            public class Stats
            {
                public int plays { get; set; }
                public int downloads { get; set; }
                public int upvotes { get; set; }
                public int downvotes { get; set; }
                public double score { get; set; }
            }

            public class BeatsaverMapVersion
            {
                public string hash { get; set; }
                public string key { get; set; }
                public string state { get; set; }
                public DateTime createdAt { get; set; }
                public int sageScore { get; set; }
                public Difficulty[] diffs { get; set; }
                public string downloadURL { get; set; }
                public string coverURL { get; set; }
                public string previewURL { get; set; }
            }

            public class Difficulty
            {
                public double njs { get; set; }
                public double offset { get; set; }
                public int notes { get; set; }
                public int bombs { get; set; }
                public int obstacles { get; set; }
                public double nps { get; set; }
                public double length { get; set; }
                public string characteristic { get; set; }
                public string difficulty { get; set; }
                public int events { get; set; }
                public bool chroma { get; set; }
                public bool me { get; set; }
                public bool ne { get; set; }
                public bool cinema { get; set; }
                public double seconds { get; set; }
                public ParitySummary paritySummary { get; set; }
                public double stars { get; set; }
            }

            public class ParitySummary
            {
                public int errors { get; set; }
                public int warns { get; set; }
                public int resets { get; set; }
            }
        }
    }
}
#pragma warning restore IDE1006 // Naming Styles
