using System;
using System.Collections.Generic;
using ModAssistant.Pages;

namespace ModAssistant
{
    public class Mod
    {
        public string name;
        public string nameWithTranslation;
        public string version;
        public string gameVersion;
        public string trueGameVersion;
        public string _id;
        public string status;
        public string authorId;
        public string uploadedDate;
        public string updatedDate;
        public Author author;
        public string description;
        public string descriptionWithTranslation;
        public string link;
        public string category;
        public DownloadLink[] downloads;
        public bool required;
        public Dependency[] dependencies;
        public List<Mod> Dependents = new List<Mod>();
        public Mods.ModListItem ListItem;
        public Translation[] translations;

        public class Author
        {
            public string _id;
            public string username;
            public string lastLogin;
        }

        public class DownloadLink
        {
            public string type;
            public string url;
            public FileHashes[] hashMd5;
        }

        public class FileHashes
        {
            public string hash;
            public string file;
        }

        public class Dependency
        {
            public string name;
            public string _id;
            public Mod Mod;
        }

        public class Translation {
            public string name;
            public string description;
            public string language;
        }

        public class TranslationWGzeyu
        {
            public string name;
            public string description;
            public string newname;
            public string newdescription;
        }
    }
}
