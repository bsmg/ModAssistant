using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ModAssistant.Pages;

namespace ModAssistant
{
    public class Themes
    {
        public static string LoadedTheme { get; private set; }
        public static List<string> LoadedThemes { get => loadedThemes.Keys.ToList(); }
        public static string ThemeDirectory = Path.Combine(Path.GetDirectoryName(Utils.ExePath), "Themes");

        /// <summary>
        /// Local dictionary of Resource Dictionaries mapped by their names.
        /// </summary>
        private static readonly Dictionary<string, Theme> loadedThemes = new Dictionary<string, Theme>();
        private static readonly List<string> preInstalledThemes = new List<string> { "Light", "Dark", "BSMG", "Light Pink" };

        /// <summary>
        /// Index of "LoadedTheme" in App.xaml
        /// </summary>
        private static readonly int LOADED_THEME_INDEX = 3;

        private static readonly List<string> supportedVideoExtensions = new List<string>() { ".mp4", ".webm", ".mkv", ".avi", ".m2ts" };

        /// <summary>
        /// Load all themes from local Themes subfolder and from embedded resources.
        /// This also refreshes the Themes dropdown in the Options screen.
        /// </summary>
        public static void LoadThemes()
        {
            loadedThemes.Clear();

            /*
             * Begin by loading local themes. We should always load these first.
             * I am doing loading here to prevent the LoadTheme function from becoming too crazy.
             */
            foreach (string localTheme in preInstalledThemes)
            {
                string location = $"Themes/{localTheme}.xaml";
                Uri local = new Uri(location, UriKind.Relative);

                ResourceDictionary localDictionary = new ResourceDictionary
                {
                    Source = local
                };

                /*
                 * Load any Waifus that come with these built-in themes, too.
                 * The format must be: Background.png and Sidebar.png as a subfolder with the same name as the theme name.
                 * For example: "Themes/Dark/Background.png", or "Themes/Ugly Kulu-Ya-Ku/Sidebar.png"
                 */
                Waifus waifus = new Waifus
                {
                    Background = GetImageFromEmbeddedResources(localTheme, "Background"),
                    Sidebar = GetImageFromEmbeddedResources(localTheme, "Sidebar")
                };

                Theme theme = new Theme(localTheme, localDictionary)
                {
                    Waifus = waifus
                };

                loadedThemes.Add(localTheme, theme);
            }

            // Load themes from Themes subfolder if it exists.
            if (Directory.Exists(ThemeDirectory))
            {
                foreach (string file in Directory.EnumerateFiles(ThemeDirectory))
                {
                    FileInfo info = new FileInfo(file);
                    string name = Path.GetFileNameWithoutExtension(info.Name);

                    if (info.Extension.ToLowerInvariant().Equals(".mat"))
                    {
                        Theme theme = LoadZipTheme(ThemeDirectory, name, ".mat");
                        if (theme is null) continue;

                        AddOrModifyTheme(name, theme);
                    }
                }

                // Finally load any loose theme files in subfolders.
                foreach (string directory in Directory.EnumerateDirectories(ThemeDirectory))
                {
                    string name = directory.Split('\\').Last();
                    Theme theme = LoadTheme(directory, name);

                    if (theme is null) continue;
                    AddOrModifyTheme(name, theme);
                }
            }

            // Refresh Themes dropdown in Options screen.
            if (Options.Instance != null && Options.Instance.ApplicationThemeComboBox != null)
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
                try
                {
                    ApplyWindowsTheme();
                }
                catch
                {
                    ApplyTheme("Light", false);
                }
                return;
            }

            try
            {
                ApplyTheme(savedTheme, false);
            }
            catch (ArgumentException)
            {
                ApplyWindowsTheme();
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
                LoadedTheme = theme;
                MainWindow.Instance.BackgroundVideo.Pause();
                MainWindow.Instance.BackgroundVideo.Visibility = Visibility.Hidden;

                if (newTheme.ThemeDictionary != null)
                {
                    // TODO: Search by name
                    Application.Current.Resources.MergedDictionaries.RemoveAt(LOADED_THEME_INDEX);
                    Application.Current.Resources.MergedDictionaries.Insert(LOADED_THEME_INDEX, newTheme.ThemeDictionary);
                }

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

                    // Load the source video if it's not the same as what's playing, or if the theme is loading for the first time.
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
        /// Writes an Embedded Resource theme to disk. You cannot write an outside theme to disk.
        /// </summary>
        /// <param name="themeName">Name of local theme.</param>
        public static void WriteThemeToDisk(string themeName)
        {
            Directory.CreateDirectory(ThemeDirectory);
            Directory.CreateDirectory($"{ThemeDirectory}\\{themeName}");

            if (File.Exists($@"{ThemeDirectory}\\{themeName}.xaml") == false)
            {
                /*
                 * Any theme that you want to write must be set as an Embedded Resource instead of the default Page.
                 * This is so that we can grab its exact content from Manifest, shown below.
                 * Writing it as is instead of using XAMLWriter keeps the source as is with comments, spacing, and organization.
                 * Using XAMLWriter would compress it into an unorganized mess.
                 */
                using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream($"ModAssistant.Themes.{themeName}.xaml"))
                using (FileStream writer = new FileStream($@"{ThemeDirectory}\\{themeName}\\{themeName}.xaml", FileMode.Create))
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
        /// Loads a Theme from a directory location.
        /// </summary>
        /// <param name="directory">The full directory path to the theme.</param>
        /// <param name="name">Name of the containing folder.</param>
        /// <returns></returns>
        private static Theme LoadTheme(string directory, string name)
        {
            Theme theme = new Theme(name, null)
            {
                Waifus = new Waifus()
            };

            foreach (string file in Directory.EnumerateFiles(directory).OrderBy(x => x))
            {
                FileInfo info = new FileInfo(file);
                bool isPng = info.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
                bool isSidePng = info.Name.EndsWith(".side.png", StringComparison.OrdinalIgnoreCase);
                bool isXaml = info.Name.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase);

                if (isPng && !isSidePng)
                {
                    theme.Waifus.Background = new BitmapImage(new Uri(info.FullName));
                }

                if (isSidePng)
                {
                    theme.Waifus.Sidebar = new BitmapImage(new Uri(info.FullName));
                }

                if (isXaml)
                {
                    try
                    {
                        Uri resourceSource = new Uri(info.FullName);
                        ResourceDictionary dictionary = new ResourceDictionary
                        {
                            Source = resourceSource
                        };

                        theme.ThemeDictionary = dictionary;
                    }
                    catch (Exception ex)
                    {
                        string message = string.Format((string)Application.Current.FindResource("Themes:FailedToLoadXaml"), name, ex.Message);
                        MessageBox.Show(message);
                    }
                }

                if (supportedVideoExtensions.Contains(info.Extension))
                {
                    if (info.Name != $"_{name}{info.Extension}" || theme.VideoLocation is null)
                    {
                        theme.VideoLocation = info.FullName;
                    }
                }
            }

            return theme;
        }

        /// <summary>
        /// Modifies an already existing theme, or adds the theme if it doesn't exist
        /// </summary>
        /// <param name="name">Name of the theme.</param>
        /// <param name="theme">Theme to modify/apply</param>
        private static void AddOrModifyTheme(string name, Theme theme)
        {
            if (loadedThemes.TryGetValue(name, out _))
            {
                if (theme.ThemeDictionary != null)
                {
                    loadedThemes[name].ThemeDictionary = theme.ThemeDictionary;
                }

                if (theme.Waifus?.Background != null)
                {
                    if (loadedThemes[name].Waifus is null) loadedThemes[name].Waifus = new Waifus();
                    loadedThemes[name].Waifus.Background = theme.Waifus.Background;
                }

                if (theme.Waifus?.Sidebar != null)
                {
                    if (loadedThemes[name].Waifus is null) loadedThemes[name].Waifus = new Waifus();
                    loadedThemes[name].Waifus.Sidebar = theme.Waifus.Sidebar;
                }

                if (!string.IsNullOrEmpty(theme.VideoLocation))
                {
                    loadedThemes[name].VideoLocation = theme.VideoLocation;
                }
            }
            else
            {
                loadedThemes.Add(name, theme);
            }
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

            using (FileStream stream = new FileStream(Path.Combine(directory, name + extension), FileMode.Open, FileAccess.Read))
            using (ZipArchive archive = new ZipArchive(stream))
            {
                foreach (ZipArchiveEntry file in archive.Entries)
                {
                    bool isPng = file.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
                    bool isSidePng = file.Name.EndsWith(".side.png", StringComparison.OrdinalIgnoreCase);
                    bool isXaml = file.Name.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase);

                    if (isPng && !isSidePng)
                    {
                        waifus.Background = GetImageFromStream(Utils.StreamToArray(file.Open()));
                    }

                    if (isSidePng)
                    {
                        waifus.Sidebar = GetImageFromStream(Utils.StreamToArray(file.Open()));
                    }

                    string videoExtension = $".{file.Name.Split('.').Last()}";
                    if (supportedVideoExtensions.Contains(videoExtension))
                    {
                        string videoName = $"{ThemeDirectory}\\{name}\\_{name}{videoExtension}";
                        Directory.CreateDirectory($"{ThemeDirectory}\\{name}");

                        if (File.Exists(videoName) == false)
                        {
                            file.ExtractToFile(videoName, false);
                        }
                        else
                        {
                            /*
                             * Check to see if the lengths of each file are different. If they are, overwrite what currently exists.
                             * The reason we are also checking LoadedTheme against the name variable is to prevent overwriting a file that's
                             * already being used by ModAssistant and causing a System.IO.IOException.
                             */
                            FileInfo existingInfo = new FileInfo(videoName);
                            if (existingInfo.Length != file.Length && LoadedTheme != name)
                            {
                                file.ExtractToFile(videoName, true);
                            }
                        }
                    }

                    if (isXaml && loadedThemes.ContainsKey(name) == false)
                    {
                        try
                        {
                            dictionary = (ResourceDictionary)XamlReader.Load(file.Open());
                        }
                        catch (Exception ex)
                        {
                            string message = string.Format((string)Application.Current.FindResource("Themes:FailedToLoadXaml"), name, ex.Message);
                            MessageBox.Show(message);
                        }
                    }
                }
            }

            Theme theme = new Theme(name, dictionary)
            {
                Waifus = waifus
            };

            return theme;
        }

        /// <summary>
        /// Returns a BeatmapImage from a byte array.
        /// </summary>
        /// <param name="array">byte array containing an image.</param>
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
                if (image.CanFreeze) image.Freeze();

                return image;
            }
        }

        private static BitmapImage GetImageFromEmbeddedResources(string themeName, string imageName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();
            var desiredResourceName = $"ModAssistant.Themes.{themeName}.{imageName}.png";

            // Don't attempt to access non-existent manifest resources
            if (!resourceNames.Contains(desiredResourceName))
            {
                return null;
            }

            try
            {
                using (Stream stream = assembly.GetManifestResourceStream(desiredResourceName))
                {
                    byte[] imageBytes = new byte[stream.Length];
                    stream.Read(imageBytes, 0, (int)stream.Length);
                    return GetImageFromStream(imageBytes);
                }
            }
            catch { return null; } //We're going to ignore errors here because backgrounds/sidebars should be optional.
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
        /// Reload the icon colors for the About, Info, Options, and Mods buttons from the currently loaded theme.
        /// </summary>
        private static void ReloadIcons()
        {
            ResourceDictionary icons = Application.Current.Resources.MergedDictionaries.First(x => x.Source?.ToString() == "Resources/Icons.xaml");

            ChangeColor(icons, "AboutIconColor", "heartDrawingGroup");
            ChangeColor(icons, "InfoIconColor", "info_circleDrawingGroup");
            ChangeColor(icons, "OptionsIconColor", "cogDrawingGroup");
            ChangeColor(icons, "ModsIconColor", "microchipDrawingGroup");
            ChangeColor(icons, "LoadingIconColor", "loadingInnerDrawingGroup");
            ChangeColor(icons, "LoadingIconColor", "loadingMiddleDrawingGroup");
            ChangeColor(icons, "LoadingIconColor", "loadingOuterDrawingGroup");
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
