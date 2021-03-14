using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace ModAssistant
{
    class Diagnostics
    {
        public static string[] ReadFolder(string path, int level = 0)
        {
            List<string> entries = new List<string>();

            foreach (string file in Directory.GetFileSystemEntries(path))
            {
                string line = string.Empty;

                if (File.Exists(file))
                {
                    line = Utils.CalculateMD5(file) + " " + LevelSeparator(level) + "├─ " + Path.GetFileName(file);
                    entries.Add(line);

                }
                else if (Directory.Exists(file))
                {
                    line = Utils.Constants.MD5Spacer + LevelSeparator(level) + "├─ " + Path.GetFileName(file);
                    entries.Add(line);

                    foreach (string entry in ReadFolder(file.Replace(@"\", @"\\"), level + 1))
                    {
                        //MessageBox.Show(entry);
                        entries.Add(entry);
                    }

                }
                else
                {
                    MessageBox.Show("! " + file);
                }
            }
            if (entries.Count > 0)
            {
                entries[entries.Count - 1] = entries[entries.Count - 1].Replace("├", "└");
            }

            return entries.ToArray();
        }

        private static string LevelSeparator(int level)
        {
            string separator = string.Empty;
            for (int i = 0; i < level; i++)
            {
                separator += "│  ";
            }
            return separator;
        }
    }
}
