using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ModAssistant.Pages;
using static ModAssistant.Http;

namespace ModAssistant
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;
        public static bool ModsOpened = false;
        public static bool ModsLoading = false;
        public static string GameVersion;
        public static string GameVersionOverride;
        public TaskCompletionSource<bool> VersionLoadStatus = new TaskCompletionSource<bool>();

        public string MainText
        {
            get
            {
                return MainTextBlock.Text;
            }
            set
            {
                Dispatcher.Invoke(new Action(() => { MainWindow.Instance.MainTextBlock.Text = value; }));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;

            const int ContentWidth = 1280;
            const int ContentHeight = 720;

            double ChromeWidth = SystemParameters.WindowNonClientFrameThickness.Left + SystemParameters.WindowNonClientFrameThickness.Right;
            double ChromeHeight = SystemParameters.WindowNonClientFrameThickness.Top + SystemParameters.WindowNonClientFrameThickness.Bottom;
            double ResizeBorder = SystemParameters.ResizeFrameVerticalBorderWidth;

            Width = ChromeWidth + ContentWidth + 2 * ResizeBorder;
            Height = ChromeHeight + ContentHeight + 2 * ResizeBorder;

            VersionText.Text = App.Version;

            if (Utils.IsVoid())
            {
                Main.Content = Invalid.Instance;
                MainWindow.Instance.ModsButton.IsEnabled = false;
                MainWindow.Instance.OptionsButton.IsEnabled = false;
                MainWindow.Instance.IntroButton.IsEnabled = false;
                MainWindow.Instance.AboutButton.IsEnabled = false;
                MainWindow.Instance.GameVersionsBox.IsEnabled = false;
                return;
            }

            Themes.LoadThemes();
            Themes.FirstLoad(Properties.Settings.Default.SelectedTheme);

            Task.Run(() => LoadVersionsAsync());

            if (!Properties.Settings.Default.Agreed || string.IsNullOrEmpty(Properties.Settings.Default.LastTab))
            {
                Main.Content = Intro.Instance;
            }
            else
            {
                switch (Properties.Settings.Default.LastTab)
                {
                    case "Intro":
                        Main.Content = Intro.Instance;
                        break;
                    case "Mods":
                        _ = ShowModsPage();
                        break;
                    case "About":
                        Main.Content = About.Instance;
                        break;
                    case "Options":
                        Main.Content = Options.Instance;
                        Themes.LoadThemes();
                        break;
                    default:
                        Main.Content = Intro.Instance;
                        break;
                }
            }
            if(Utils.Constants.DownloadSite == "国内中转")
            {
                MessageBox.Show("你当前正在使用国内中转源的ModAssistant。不是从BeatMods源站下载，\n而是连接WGzeyu提供的国际线路较好的国内服务器，中转访问源站下载。\n\n通常，我们建议使用源站版，直接连接BeatMods下载。\n但国内部分运营商连接BeatMods时，下载速度甚至低于20KB/s，\n完全下不动导致报错无法安装，这个版本就是为了这些用户准备的。\n\n国内中转服务器由中文版作者WGzeyu提供。\n由于中转服务器带宽较低，且每次中转安装都会占用作者的网速与流量，\n所以我们对中转下载设置了限速2Mbps，安装常用Mod总计需一分钟左右。\n\n【注意】如果软件发布了更新，那么国内中转版会默认更新到源站版，\n但文件名不会变！如有需要可重新到群文件下载新的国内中转版。\n\n点击确定将打开源站中文列表版ModAssistant下载地址。");
                System.Diagnostics.Process.Start("https://github.com/wgzeyu/ModAssistant-CN/releases/latest");
            }
        }

        private async void LoadVersionsAsync()
        {
            try
            {
                var resp = await HttpClient.GetAsync(Utils.Constants.BeatModsVersions);
                var body = await resp.Content.ReadAsStringAsync();
                List<string> versions = JsonSerializer.Deserialize<string[]>(body).ToList();

                resp = await HttpClient.GetAsync(Utils.Constants.BeatModsAlias);
                body = await resp.Content.ReadAsStringAsync();
                object jsonObject = JsonSerializer.DeserializeObject(body);

                Dispatcher.Invoke(() =>
                {
                    GameVersion = GetGameVersion(versions, jsonObject);

                    GameVersionsBox.ItemsSource = versions;
                    GameVersionsBox.SelectedValue = GameVersion;

                    if (!string.IsNullOrEmpty(GameVersionOverride))
                    {
                        GameVersionsBox.Visibility = Visibility.Collapsed;
                        GameVersionsBoxOverride.Visibility = Visibility.Visible;
                        GameVersionsBoxOverride.Text = GameVersionOverride;
                        GameVersionsBoxOverride.IsEnabled = false;
                    }

                    if (!string.IsNullOrEmpty(GameVersion) && Properties.Settings.Default.Agreed)
                    {
                        MainWindow.Instance.ModsButton.IsEnabled = true;
                    }
                });

                VersionLoadStatus.SetResult(true);
            }
            catch (Exception e)
            {
                Dispatcher.Invoke(() =>
                {
                    GameVersionsBox.IsEnabled = false;
                    MessageBox.Show($"{Application.Current.FindResource("MainWindow:GameVersionLoadFailed")}\n{e}");
                });

                VersionLoadStatus.SetResult(false);
            }
        }

        private string GetGameVersion(List<string> versions, object aliases)
        {
            string version = Utils.GetVersion();
            if (!string.IsNullOrEmpty(version) && versions.Contains(version))
            {
                return version;
            }

            string aliasOf = CheckAliases(versions, aliases, version);
            if (!string.IsNullOrEmpty(aliasOf))
            {
                return aliasOf;
            }

            string versionsString = String.Join(",", versions.ToArray());
            if (Properties.Settings.Default.AllGameVersions != versionsString)
            {
                Properties.Settings.Default.AllGameVersions = versionsString;
                Properties.Settings.Default.Save();

                string title = (string)Application.Current.FindResource("MainWindow:GameUpdateDialog:Title");
                string line1 = (string)Application.Current.FindResource("MainWindow:GameUpdateDialog:Line1");
                string line2 = (string)Application.Current.FindResource("MainWindow:GameUpdateDialog:Line2");

                Utils.ShowMessageBoxAsync($"{line1}\n\n{line2}", title);
                return versions[0];
            }

            if (!string.IsNullOrEmpty(Properties.Settings.Default.GameVersion) && versions.Contains(Properties.Settings.Default.GameVersion))
                return Properties.Settings.Default.GameVersion;
            return versions[0];
        }

        private string CheckAliases(List<string> versions, object aliases, string detectedVersion)
        {
            Dictionary<string, object> Objects = (Dictionary<string, object>)aliases;
            foreach (string version in versions)
            {
                object[] aliasArray = (object[])Objects[version];
                foreach (object alias in aliasArray)
                {
                    if (alias.ToString() == detectedVersion)
                    {
                        GameVersionOverride = detectedVersion;
                        return version;
                    }
                }
            }
            return string.Empty;
        }

        private async Task ShowModsPage()
        {
            void OpenModsPage()
            {
                Main.Content = Mods.Instance;
                Properties.Settings.Default.LastTab = "Mods";
                Properties.Settings.Default.Save();
                Mods.Instance.RefreshColumns();
            }

            if (ModsOpened == true && Mods.Instance.PendingChanges == false)
            {
                OpenModsPage();
                return;
            }

            Main.Content = Loading.Instance;

            if (ModsLoading) return;
            ModsLoading = true;
            await Mods.Instance.LoadMods();
            ModsLoading = false;

            if (ModsOpened == false) ModsOpened = true;
            if (Mods.Instance.PendingChanges == true) Mods.Instance.PendingChanges = false;

            if (Main.Content == Loading.Instance)
            {
                OpenModsPage();
            }
        }

        private void ModsButton_Click(object sender, RoutedEventArgs e)
        {
            _ = ShowModsPage();
        }

        private void IntroButton_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = Intro.Instance;
            Properties.Settings.Default.LastTab = "Intro";
            Properties.Settings.Default.Save();
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = About.Instance;
            Properties.Settings.Default.LastTab = "About";
            Properties.Settings.Default.Save();
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = Options.Instance;
            Themes.LoadThemes();
            Properties.Settings.Default.LastTab = "Options";
            Properties.Settings.Default.Save();
        }

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            Mods.Instance.InstallMods();
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            if ((Mods.ModListItem)Mods.Instance.ModsListView.SelectedItem == null)
            {
                MessageBox.Show((string)Application.Current.FindResource("MainWindow:NoModSelected"));
                return;
            }
            Mods.ModListItem mod = ((Mods.ModListItem)Mods.Instance.ModsListView.SelectedItem);
            string infoUrl = mod.ModInfo.link;
            if (string.IsNullOrEmpty(infoUrl))
            {
                MessageBox.Show(string.Format((string)Application.Current.FindResource("MainWindow:NoModInfoPage"), mod.ModName));
            }
            else
            {
                System.Diagnostics.Process.Start(infoUrl);
            }
        }

        private async void GameVersionsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string oldGameVersion = GameVersion;

            GameVersion = (sender as ComboBox).SelectedItem.ToString();

            if (string.IsNullOrEmpty(oldGameVersion)) return;

            Properties.Settings.Default.GameVersion = GameVersion;
            Properties.Settings.Default.Save();

            if (ModsOpened)
            {
                var prevPage = Main.Content;

                Mods.Instance.PendingChanges = true;
                await ShowModsPage();

                Main.Content = prevPage;
            }
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            About.Instance.PatUp.IsOpen = false;
            About.Instance.PatButton.IsEnabled = true;
            About.Instance.HugUp.IsOpen = false;
            About.Instance.HugButton.IsEnabled = true;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Main.Content == Mods.Instance)
            {
                Mods.Instance.RefreshColumns();
            }
        }

        private void BackgroundVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            BackgroundVideo.Position = TimeSpan.Zero;
            BackgroundVideo.Play();
        }
    }
}
