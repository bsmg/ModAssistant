using System.Collections.Generic;
using ModAssistant.Pages;

namespace ModAssistant.Classes
{
    public partial class Mod
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string GameVersion { get; set; }
        public string Id { get; set; }
        public string AuthorId { get; set; }
        public string UploadedDate { get; set; }
        public string UpdatedDate { get; set; }
        public Author Author_ { get; set; } //ToDo find a better name
        public string Description { get; set; }
        public string Link { get; set; }
        public string Category { get; set; }
        public DownloadLink[] Downloads { get; set; }
        public bool Required { get; set; }
        public Dependency[] Dependencies { get; set; }

        private List<Classes.Mod> _dependents = new List<Classes.Mod>();

        public List<Classes.Mod> Dependents
        {
            get { return _dependents; }
            set { _dependents = value; }
        }

        public Mods.ModListItem ListItem { get; set; }
    }
}