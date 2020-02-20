using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.IO;
using System.Windows.Media;
using ModAssistant.Pages;
using System.Reflection;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.IO.Compression;
using System.Windows.Markup;

namespace ModAssistant
{
    public class Themes
    {
        public static string LoadedTheme { get; private set; } //Currently loaded theme
        public static List<string> LoadedThemes { get => loadedThemes.Keys.ToList(); } //String of themes that can be loaded
        public static string ThemeDirectory => $"{Environment.CurrentDirectory}/Themes"; //Self explanatory.

        //Local dictionary of ResourceDictionarys mapped by their names.
        private static Dictionary<string, Theme> loadedThemes = new Dictionary<string, Theme>();
        private static List<string> preInstalledThemes = new List<string> { "Light", "Dark", "Light Pink" }; //These themes will always be available to use.

        /// <summary>
        /// Load all themes from local Themes subfolder and from embedded resources.
        /// This also refreshes the Themes dropdown in the Options screen.
        /// </summary>
        public static void LoadThemes()
        {
            loadedThemes.Clear();
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
                    if (info.Extension.ToLower().Equals(".xaml") && !loadedThemes.ContainsKey(name))
                    {
                        Theme theme = LoadTheme(name, false);
                        theme.Waifus = LoadWaifus(name);
                        if (theme != null)
                        {
                            loadedThemes.Add(name, theme);
                        }
                    }
                }
                foreach (string file in Directory.EnumerateFiles(ThemeDirectory))
                {
                    FileInfo info = new FileInfo(file);
                    string name = Path.GetFileNameWithoutExtension(info.Name);
                    //Look for zip files with ".mat" extension.
                    if (info.Extension.ToLower().Equals(".mat") && !loadedThemes.ContainsKey(name))
                    {
                        Theme theme = LoadZipTheme(ThemeDirectory, name, ".mat");
                        if (theme != null)
                        {
                            loadedThemes.Add(name, theme);
                        }
                    }
                }
            }
            foreach (string localTheme in preInstalledThemes) //Load local themes (Light and Dark)
            {
                if (!loadedThemes.ContainsKey(localTheme))
                {
                    Theme theme = LoadTheme(localTheme, true);
                    loadedThemes.Add(localTheme, theme);
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
            if (string.IsNullOrEmpty(savedTheme))
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
                MainWindow.Instance.MainText = (string)Application.Current.FindResource("Themes:ThemeNotFound");
            }
        }

        /// <summary>
        /// Applies a loaded theme to ModAssistant.
        /// </summary>
        /// <param name="theme">Name of the theme.</param>
        /// <param name="sendMessage">Send message to MainText (default: true).</param>
        public static void ApplyTheme(string theme, bool sendMessage = true)
        {
            if (loadedThemes.TryGetValue(theme, out Theme newTheme))
            {
                MainWindow.Instance.BackgroundVideo.Pause();
                MainWindow.Instance.BackgroundVideo.Visibility = Visibility.Hidden;
                Application.Current.Resources.MergedDictionaries.RemoveAt(2); //We might want to change this to a static integer or search by name.
                LoadedTheme = theme;
                Application.Current.Resources.MergedDictionaries.Insert(2, newTheme.ThemeDictionary); //Insert our new theme into the same spot as last time.
                Properties.Settings.Default.SelectedTheme = theme;
                Properties.Settings.Default.Save();
                if (sendMessage)
                {
                    MainWindow.Instance.MainText = string.Format((string)Application.Current.FindResource("Themes:ThemeSet"), theme);
                }
                ApplyWaifus();
                if (File.Exists(newTheme.VideoLocation))
                {
                    Uri videoUri = new Uri(newTheme.VideoLocation, UriKind.Absolute);
                    MainWindow.Instance.BackgroundVideo.Visibility = Visibility.Visible;
                    if (!sendMessage || MainWindow.Instance.BackgroundVideo.Source?.AbsoluteUri != videoUri.AbsoluteUri)
                    {
                        MainWindow.Instance.BackgroundVideo.Stop();
                        MainWindow.Instance.BackgroundVideo.Source = videoUri;
                    }
                    MainWindow.Instance.BackgroundVideo.Play();
                }
                ReloadIcons();
            }
            else
            {
                throw new ArgumentException(string.Format((string)Application.Current.FindResource("Themes:ThemeMissing"), theme));
            }
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
                /*
                 * Light Pink theme is set to build as an Embedded Resource instead of the default Page.
                 * This is so that we can grab its exact content from Manifest, shown below.
                 * Writing it as is instead of using XAMLWriter keeps the source as is with comments, spacing, and organization.
                 * Using XAMLWriter would compress it into an unorganized mess.
                 */
                using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream($"ModAssistant.Themes.{themeName}.xaml"))
                using (FileStream writer = new FileStream($@"{ThemeDirectory}\\{themeName}.xaml", FileMode.Create))
                {
                    byte[] buffer = new byte[s.Length];
                    int read = s.Read(buffer, 0, (int)s.Length);
                    writer.Write(buffer, 0, buffer.Length);
                }

                MainWindow.Instance.MainText = string.Format((string)Application.Current.FindResource("Themes:SavedTemplateTheme"), themeName);
            }
            else
            {
                MessageBox.Show((string)Application.Current.FindResource("Themes:TemplateThemeExists"));
            }
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
        /// <param name="localUri">Specifies whether or not to search locally or in the Themes subfolder.</param>
        /// <returns></returns>
        private static Theme LoadTheme(string name, bool localUri = false)
        {
            string location = $"{ThemeDirectory}/{name}.xaml";
            if (!File.Exists(location) && !localUri) //Return null if we're looking for an item in the Themes subfolder that doesn't exist.
            {
                return null;
            }
            if (localUri) //Change the location of the theme since we're not looking in a directory but rather in ModAssistant itself.
            {
                location = $"Themes/{name}.xaml";
            }
            Uri uri = new Uri(location, localUri ? UriKind.Relative : UriKind.Absolute);
            ResourceDictionary dictionary = new ResourceDictionary();
            try
            {
                dictionary.Source = uri;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not load {name}.\n\n{ex.Message}\n\nIgnoring...");
                return null;
            }
            Theme theme = new Theme(name, dictionary);
            if (File.Exists($"{ThemeDirectory}/{name}.mp4"))
            {
                theme.VideoLocation = $"{ThemeDirectory}/{name}.mp4";
            }
            return theme;
        }

        /// <summary>
        /// Loads themes from pre-packged zips.
        /// </summary>
        /// <param name="directory">Theme directory</param>
        /// <param name="name">Theme name</param>
        /// <param name="extension">Theme extension</param>
        private static Theme LoadZipTheme(string directory, string name, string extension)
        {
            Waifus waifus = new Waifus();
            ResourceDictionary dictionary = null;
            using (FileStream stream = new FileStream(Path.Combine(directory, name + extension), FileMode.Open))
            using (ZipArchive archive = new ZipArchive(stream))
            {
                foreach (ZipArchiveEntry file in archive.Entries)
                {
                    if (file.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) &&
                        !file.Name.EndsWith(".side.png", StringComparison.OrdinalIgnoreCase))
                    {
                        waifus.Background = GetImageFromStream(Utils.StreamToArray(file.Open()));
                    }
                    if (file.Name.EndsWith(".side.png", StringComparison.OrdinalIgnoreCase))
                    {
                        waifus.Sidebar = GetImageFromStream(Utils.StreamToArray(file.Open()));
                    }
                    if (file.Name.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!loadedThemes.ContainsKey(name))
                        {
                            try
                            {
                                dictionary = (ResourceDictionary)XamlReader.Load(file.Open());
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Could not load {name}.\n\n{ex.Message}\n\nIgnoring...");
                            }
                        }
                    }
                }
            }
            Theme theme = new Theme(name, dictionary);
            theme.Waifus = waifus;
            return theme;
        }

        /// <summary>
        /// Returns a BeatmapImage from a memory stream.
        /// </summary>
        /// <param name="stream">memory stream containing an image.</param>
        /// <returns></returns>
        private static BitmapImage GetImageFromStream(byte[] array)
        {
            using (var mStream = new MemoryStream(array))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = mStream;
                image.EndInit();
                if (image.CanFreeze)
                    image.Freeze();
                return image;
            }
        }

        /// <summary>
        /// Applies waifus from currently loaded Theme.
        /// </summary>
        private static void ApplyWaifus()
        {
            Waifus waifus = loadedThemes[LoadedTheme].Waifus;
            if (waifus?.Background is null)
            {
                MainWindow.Instance.BackgroundImage.Opacity = 0;
            }
            else
            {
                MainWindow.Instance.BackgroundImage.Opacity = 1;
                MainWindow.Instance.BackgroundImage.ImageSource = waifus.Background;
            }

            if (waifus?.Sidebar is null)
            {
                MainWindow.Instance.SideImage.Visibility = Visibility.Hidden;
            }
            else
            {
                MainWindow.Instance.SideImage.Visibility = Visibility.Visible;
                MainWindow.Instance.SideImage.Source = waifus.Sidebar;
            }
        }

        /// <summary>
        /// Loads the waifus from the Themes folder or theme files if they exist.
        /// </summary>
        /// <param name="name">Theme's name.</param>
        private static Waifus LoadWaifus(string name)
        {
            Waifus waifus = new Waifus();
            if (File.Exists($"{ThemeDirectory}\\{name}.png"))
            {
                waifus.Background = new BitmapImage(new Uri(Path.Combine($"{ThemeDirectory}\\{name}.png")));
            }
            if (File.Exists($"{ThemeDirectory}\\{name}.png"))
            {
                waifus.Sidebar = new BitmapImage(new Uri(Path.Combine($"{ThemeDirectory}\\{name}.side.png")));
            }
            return waifus;
        }

        /// <summary>
        /// Reload the icon colors for the About, Info, Options, and Mods buttons from the currently loaded theme.
        /// </summary>
        private static void ReloadIcons()
        {
            ResourceDictionary icons = Application.Current.Resources.MergedDictionaries.First(x => x.Source?.ToString() == "Resources/Icons.xaml");

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
            Application.Current.Resources[ResourceColorName] = loadedThemes[LoadedTheme].ThemeDictionary[ResourceColorName];
            ((GeometryDrawing)((DrawingGroup)icons[DrawingGroupName]).Children[0]).Brush = (Brush)Application.Current.Resources[ResourceColorName];
        }

        private class Waifus
        {
            public BitmapImage Background = null;
            public BitmapImage Sidebar = null;
        }

        private class Theme
        {
            public string Name;
            public ResourceDictionary ThemeDictionary;
            public Waifus Waifus = null;
            public string VideoLocation = null;

            public Theme(string name, ResourceDictionary dictionary)
            {
                Name = name;
                ThemeDictionary = dictionary;
            }
        }
    }
}
