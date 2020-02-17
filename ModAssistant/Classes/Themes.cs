using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.IO;
using System.Windows.Media;
using ModAssistant.Pages;
using System.Xml;
using System.Windows.Markup;
using System.Reflection;
using Microsoft.Win32;

namespace ModAssistant
{
    public class Themes
    {
        public static string LoadedTheme { get; private set; }
        public static List<string> LoadedThemes { get => loadedThemes.Keys.ToList(); }
        public static string ThemeDirectory => $"{Environment.CurrentDirectory}/Themes";

        private static Dictionary<string, ResourceDictionary> loadedThemes = new Dictionary<string, ResourceDictionary>();
        private static List<string> preInstalledThemes = new List<string> { "Light", "Dark" };

        /// <summary>
        /// Load all themes from local Themes subfolder and from embedded resources.
        /// This also refreshes the Themes dropdown in the Options screen.
        /// </summary>
        public static void LoadThemes()
        {
            loadedThemes.Clear();
            foreach (string localTheme in preInstalledThemes) //Load local themes (Light and Dark)
            {
                ResourceDictionary theme = LoadTheme(localTheme, true);
                loadedThemes.Add(localTheme, theme);
            }
            if (Directory.Exists(ThemeDirectory)) //Load themes from Themes subfolder if it exists.
            {
                foreach (string file in Directory.EnumerateFiles(ThemeDirectory))
                {
                    FileInfo info = new FileInfo(file);
                    //FileInfo includes the extension in its Name field, so we have to select only the actual name.
                    string name = Path.GetFileNameWithoutExtension(info.Name);
                    //Ignore Themes without the xaml extension and ignore themes with the same names as others.
                    //If requests are made I can instead make a Theme class that splits the pre-installed themes from
                    //user-made ones so that one more user-made Light/Dark theme can be added.
                    if (info.Extension.ToLower().Contains("xaml") && !loadedThemes.ContainsKey(name))
                    {
                        ResourceDictionary theme = LoadTheme(name);
                        if (theme != null)
                        {
                            loadedThemes.Add(name, theme);
                        }
                    }
                }
            }
            if (Options.Instance != null && Options.Instance.ApplicationThemeComboBox != null) //Refresh Themes dropdown in Options screen.
            {
                Options.Instance.ApplicationThemeComboBox.ItemsSource = LoadedThemes;
                Options.Instance.ApplicationThemeComboBox.SelectedIndex = LoadedThemes.IndexOf(LoadedTheme);
            }
        }

        /// <summary>
        /// Runs once at the start of the program, performs settings checking.
        /// </summary>
        /// <param name="savedTheme">Theme name retrieved from the settings file.</param>
        public static void FirstLoad(string savedTheme)
        {
            if (savedTheme == "")
            {
                Themes.ApplyWindowsTheme();
                return;
            }
            try
            {
                Themes.ApplyTheme(savedTheme, false);
            }
            catch (ArgumentException)
            {
                Themes.ApplyWindowsTheme();
                MainWindow.Instance.MainText = "Theme not found, reverting to default theme...";
            }
        }

        /// <summary>
        /// Applies a loaded theme to ModAssistant.
        /// </summary>
        /// <param name="theme">Name of the theme.</param>
        /// <param name="sendMessage">Send message to MainText (default: true).</param>
        public static void ApplyTheme(string theme, bool sendMessage = true)
        {
            if (loadedThemes.TryGetValue(theme, out ResourceDictionary newTheme))
            {
                Application.Current.Resources.MergedDictionaries.RemoveAt(0);
                LoadedTheme = theme;
                Application.Current.Resources.MergedDictionaries.Insert(0, newTheme);
                Properties.Settings.Default.SelectedTheme = theme;
                Properties.Settings.Default.Save();
                if (sendMessage)
                {
                    MainWindow.Instance.MainText = $"Theme set to {theme}.";
                }
                ReloadIcons();
            }
            else throw new ArgumentException($"{theme} does not exist.");
        }

        /// <summary>
        /// Writes a local theme to disk. You cannot write a theme loaded from the Themes subfolder to disk.
        /// </summary>
        /// <param name="themeName">Name of local theme.</param>
        public static void WriteThemeToDisk(string themeName)
        {
            if (!Directory.Exists(ThemeDirectory))
            {
                Directory.CreateDirectory(ThemeDirectory);
            }

            if (!File.Exists($@"{ThemeDirectory}\\{themeName}.xaml"))
            {
                using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream($"ModAssistant.Themes.{themeName}.xaml"))
                using (FileStream writer = new FileStream($@"{ThemeDirectory}\\{themeName}.xaml", FileMode.Create))
                {
                    byte[] buffer = new byte[s.Length];
                    int read = s.Read(buffer, 0, (int)s.Length);
                    writer.Write(buffer, 0, buffer.Length);
                }
                MainWindow.Instance.MainText = $"Template theme \"{themeName}\" saved to Themes folder.";
            }
            else MessageBox.Show("Template theme already exists!");
        }

        /// <summary>
        /// Finds the theme set on Windows and applies it.
        /// </summary>
        public static void ApplyWindowsTheme()
        {
            using (RegistryKey key = Registry.CurrentUser
                  .OpenSubKey("Software").OpenSubKey("Microsoft")
                  .OpenSubKey("Windows").OpenSubKey("CurrentVersion")
                  .OpenSubKey("Themes").OpenSubKey("Personalize"))
            {
                object registryValueObject = key?.GetValue("AppsUseLightTheme");
                if (registryValueObject != null)
                {
                    if ((int)registryValueObject <= 0)
                    {
                        ApplyTheme("Dark", false);
                        return;
                    }
                }
                ApplyTheme("Light", false);
            }
        }

        /// <summary>
        /// Loads a ResourceDictionary from either Embedded Resources or from a file location.
        /// </summary>
        /// <param name="name">ResourceDictionary file name.</param>
        /// <param name="localUri">Specifies whether or not to search in Embedded Resources or in the Themes subfolder.</param>
        /// <returns></returns>
        private static ResourceDictionary LoadTheme(string name, bool localUri = false)
        {
            string location = $"{Environment.CurrentDirectory}/Themes/{name}.xaml";
            if (!File.Exists(location) && !localUri) return null;
            if (localUri) location = $"Themes/{name}.xaml";
            Uri uri = new Uri(location, localUri ? UriKind.Relative : UriKind.Absolute);
            ResourceDictionary dictionary = new ResourceDictionary();
            try
            {
                dictionary.Source = uri;
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Could not load {name}.\n\n{ex.Message}\n\nIgnoring...");
                return null;
            }
            return dictionary;
        }

        /// <summary>
        /// Reload the icon colors for the About, Info, Options, and Mods buttons from the currently loaded theme.
        /// </summary>
        private static void ReloadIcons()
        {
            ResourceDictionary icons = Application.Current.Resources.MergedDictionaries.First(x => x.Source.ToString() == "Resources/Icons.xaml");

            ChangeColor(icons, "AboutIconColor", "heartDrawingGroup");
            ChangeColor(icons, "InfoIconColor", "info_circleDrawingGroup");
            ChangeColor(icons, "OptionsIconColor", "cogDrawingGroup");
            ChangeColor(icons, "ModsIconColor", "microchipDrawingGroup");
        }

        /// <summary>
        /// Change the color of an image from the loaded theme.
        /// </summary>
        /// <param name="icons">ResourceDictionary that contains the image.</param>
        /// <param name="ResourceColorName">Resource name of the color to change.</param>
        /// <param name="DrawingGroupName">DrawingGroup name for the image.</param>
        private static void ChangeColor(ResourceDictionary icons, string ResourceColorName, string DrawingGroupName)
        {
            Application.Current.Resources[ResourceColorName] = loadedThemes[LoadedTheme][ResourceColorName];
            ((GeometryDrawing)((DrawingGroup)icons[DrawingGroupName]).Children[0]).Brush = (Brush)Application.Current.Resources[ResourceColorName];
        }
    }
}
