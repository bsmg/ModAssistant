using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.IO;
using System.Windows.Media;

namespace ModAssistant
{
    public class Themes
    {
        public static string LoadedTheme { get; private set; }
        public static List<string> LoadedThemes { get => loadedThemes.Keys.ToList(); }

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
            if (Directory.Exists($"{Environment.CurrentDirectory}/Themes"))
            {
                foreach (string file in Directory.EnumerateFiles($"{Environment.CurrentDirectory}/Themes"))
                {
                    FileInfo info = new FileInfo(file);
                    //Ignore Themes without the xaml extension and ignore themes with the same names as others.
                    //If requests are made I can instead make a Theme class that splits the pre-installed themes from
                    //user-made ones so that one more user-made Light/Dark theme can be added.
                    if (info.Extension.ToLowerInvariant() == "xaml" && !loadedThemes.ContainsKey(info.Name))
                    {
                        ResourceDictionary theme = LoadTheme(info.Name);
                        if (theme != null)
                        {
                            loadedThemes.Add(info.Name, theme);
                        }
                    }
                }
                //MessageBox.Show($"(DEBUG) Loaded {loadedThemes.Count - 2} themes from Themes folder.");
            }
        }

        private static ResourceDictionary LoadTheme(string name, bool localUri = false)
        {
            string location = $"{Environment.CurrentDirectory}/Themes/{name}.xaml";
            if (!File.Exists(location) && !localUri) return null;
            if (localUri) location = $"Themes/{name}.xaml";
            Uri uri = new Uri(location, localUri ? UriKind.Relative : UriKind.Absolute);
            ResourceDictionary dictionary = new ResourceDictionary();
            dictionary.Source = uri;
            return dictionary;
        }

        public static void ApplyTheme(string theme, FrameworkElement element)
        {
            ResourceDictionary newTheme = loadedThemes[theme];
            if (newTheme != null)
            {
                Application.Current.Resources.MergedDictionaries.RemoveAt(0);
                LoadedTheme = theme;
                Application.Current.Resources.MergedDictionaries.Insert(0, newTheme);
                ReloadIcons(element);
            }
            else throw new ArgumentException($"{theme} does not exist.");
        }

        private static void ReloadIcons(FrameworkElement element)
        {
            ResourceDictionary icons = Application.Current.Resources.MergedDictionaries.First(x => x.Source.ToString() == "Resources/Icons.xaml");

            ChangeColor(element, icons, "AboutIconColor", "heartDrawingGroup");
            ChangeColor(element, icons, "InfoIconColor", "info_circleDrawingGroup");
            ChangeColor(element, icons, "OptionsIconColor", "cogDrawingGroup");
            ChangeColor(element, icons, "ModsIconColor", "microchipDrawingGroup");
        }

        private static void ChangeColor(FrameworkElement element, ResourceDictionary icons, string ResourceColorName, string DrawingGroupName)
        {
            element.Resources[ResourceColorName] = loadedThemes[LoadedTheme][ResourceColorName];
            ((GeometryDrawing)((DrawingGroup)icons[DrawingGroupName]).Children[0]).Brush = (Brush)element.Resources[ResourceColorName];
        }
    }
}
