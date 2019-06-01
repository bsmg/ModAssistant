using System;
using System.Collections.Generic;
using ModAssistant.Pages;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModAssistant
{
    public partial class Mod
    {
        public string Name;
        public string Version;
        public string GameVersion;
        public string Id;
        public string AuthorId;
        public string UploadedDate;
        public string UpdatedDate;
        public Author Author_; //ToDo find a better name
        public string Description;
        public string Link;
        public string Category;
        public DownloadLink[] Downloads;
        public bool Required;
        public Dependency[] Dependencies;
        public List<Mod> Dependents = new List<Mod>();
        public Mods.ModListItem ListItem;
    }
}
