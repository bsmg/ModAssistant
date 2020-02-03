using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace ModAssistant.Classes
{
    public class Themes
    {
        public static string LoadedTheme { get; private set; }

        private static Dictionary<string, ResourceDictionary> loadedThemes = new Dictionary<string, ResourceDictionary>();
        private static List<string> preInstalledThemes = new List<string> { "Light", "Dark" };

        public static void LoadThemes()
        {
            loadedThemes.Clear();
            foreach (string localTheme in preInstalledThemes)
            {
                ResourceDictionary theme = LoadTheme(localTheme, true);
                loadedThemes.Add(localTheme, theme);
            }
            //MessageBox.Show($"Loaded Themes: {string.Join(", ", loadedThemes.Keys.ToArray())}")
        }

        private static ResourceDictionary LoadTheme(string name, bool localUri = false)
        {
            string location = $"{Environment.CurrentDirectory}/Themes/{name}.xaml";
            if (localUri) location = $"Themes/{name}.xaml";
            Uri uri = new Uri(location, localUri ? UriKind.Relative : UriKind.Absolute);
            ResourceDictionary dictionary = new ResourceDictionary();
            dictionary.Source = uri;
            return dictionary;
        }

        public static void ApplyTheme(string theme)
        {
            ResourceDictionary newTheme = loadedThemes[theme];
            if (newTheme != null)
            {
                Application.Current.Resources.MergedDictionaries.RemoveAt(0);
                LoadedTheme = theme;
                Application.Current.Resources.MergedDictionaries.Insert(0, newTheme);
            }
            else throw new ArgumentException($"{theme} does not exist.");
        }
    }
}
