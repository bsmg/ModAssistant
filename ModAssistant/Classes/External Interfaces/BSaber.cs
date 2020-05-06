using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static ModAssistant.Http;

namespace ModAssistant.API
{
    public class BSaber
    {
        private const string PlaylistAPIUrl = "https://bsaber.com/PlaylistAPI";
        private static readonly string PlaylistFolder = "Playlists";

        public static async Task PlaylistInstall(Uri uri)
        {
            switch (uri.Host)
            {
                case "playlist":
                    string filename = uri.Segments.Last();
                    await Utils.DownloadAsset(PlaylistAPIUrl + filename, PlaylistFolder);
                    Playlist playlist = JsonSerializer.Deserialize<Playlist>(File.ReadAllText(Path.Combine(Utils.BeatSaberPath, PlaylistFolder, filename)));
                    foreach (Playlist.Song song in playlist.songs)
                    {
                        if (string.IsNullOrEmpty(song.key))
                        {
                            await BeatSaver.GetFromHash(song.hash);
                        }
                        else
                        {
                            await BeatSaver.GetFromKey(song.key);
                        }
                    }
                    break;
            }
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
};
