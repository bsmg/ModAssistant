using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using static ModAssistant.Http;
using System.Windows;

namespace ModAssistant.API
{
    public class Playlists
    {
        private const string BSaberURLPrefix = "https://bsaber.com/PlaylistAPI/";
        private const string PlaylistsFolder = "Playlists";
        private static readonly string BeatSaberPath = Utils.BeatSaberPath;

        public static async Task DownloadAll(Uri uri)
        {
            switch (uri.Host)
            {
                case "playlist":
                    Uri url = new Uri($"{uri.LocalPath.Trim('/')}");
                    string filename = await Get(url);
                    await DownloadFrom(filename);
                    break;

            }
        }

        public static async Task<string> Get(Uri url)
        {
            string filename = url.Segments.Last();
            string absolutePath = Path.Combine(BeatSaberPath, PlaylistsFolder, filename);
            try
            {
                await Utils.DownloadAsset(url.ToString(), PlaylistsFolder, filename);
                return absolutePath;
            }
            catch
            {
                return null;
            }
        }

        public static async Task DownloadFrom(string file)
        {
            Playlist playlist = JsonSerializer.Deserialize<Playlist>(File.ReadAllText(file));
            int songCount = playlist.songs.Length;
            int processedSongs = 0;
            foreach (Playlist.Song song in playlist.songs)
            {
                if (!string.IsNullOrEmpty(song.hash))
                {
                    await BeatSaver.GetFromHash(song.hash, false);
                }
                else if (!string.IsNullOrEmpty(song.key))
                {
                    await BeatSaver.GetFromKey(song.key, false);
                }
                processedSongs++;
            }
        }


        class Playlist
        {
            public string playlistTitle { get; set; }
            public string playlistAuthor { get; set; }
            public string image { get; set; }
            public Song[] songs { get; set; }

            public class Song
            {
                public string key { get; set; }
                public string hash { get; set; }
                public string songName { get; set; }
                public string uploader { get; set; }
            }
        }
    }
}
