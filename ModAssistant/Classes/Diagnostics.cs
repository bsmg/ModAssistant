using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace ModAssistant.Classes
{
    public static class Diagnostics
    {
        public static string[] ReadFolder(string path, int level = 0)
        {
            List<string> entries = new List<string>();

            foreach (string file in Directory.GetFileSystemEntries(path))
            {
                if (File.Exists(file))
                {
                    entries.Add(
                        $"{Utils.CalculateMd5(file)} {LevelSeparator(level)}├─ {Path.GetFileName(file)}");
                }
                else if (Directory.Exists(file))
                {
                    entries.Add(
                        $"{Utils.Constants.Md5Spacer}{LevelSeparator(level)}├─ {Path.GetFileName(file)}");

                    entries.AddRange(ReadFolder(file.Replace(@"\", @"\\"), level + 1));
                }
                else
                {
                    MessageBox.Show($"! {file}");
                }
            }

            if (entries.Count > 0)
                entries[entries.Count - 1] = entries[entries.Count - 1].Replace("├", "└");

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