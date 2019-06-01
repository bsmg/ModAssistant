namespace ModAssistant.Classes
{
    public partial class Mod
    {
        public class DownloadLink
        {
            public string Type { get; set; }
            public string Url { get; set; }
            public FileHashes[] HashMd5 { get; set; }
        }
    }
}