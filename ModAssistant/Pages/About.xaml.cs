using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Navigation;
using static ModAssistant.Http;
using MessageBox = System.Windows.Forms.MessageBox;

namespace ModAssistant.Pages
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class About : Page
    {
        public static About Instance = new About();

        public About()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private async void HeadpatsButton_Click(object sender, RoutedEventArgs e)
        {
            PatButton.IsEnabled = false;
            var success = await Task.Run(async () => await HeadPat());

            PatUp.IsOpen = success;
            if (!success) PatButton.IsEnabled = true;
        }

        private async void HugsButton_Click(object sender, RoutedEventArgs e)
        {
            HugButton.IsEnabled = false;
            var success = await Task.Run(async () => await Hug());

            HugUp.IsOpen = success;
            if (!success) HugButton.IsEnabled = true;
        }

        private async Task<string> WeebCDN(string type)
        {
            var resp = await HttpClient.GetAsync(Utils.Constants.WeebCDNAPIURL + type + "/random");
            var body = await resp.Content.ReadAsStringAsync();

            var response = JsonSerializer.Deserialize<Utils.WeebCDNRandomResponse>(body);
            return response.url;
        }

        private async Task<bool> HeadPat()
        {
            try
            {
                var image = await WeebCDN("pats");
                PatImage.Load(image);

                return true;
            }
            catch (Exception ex)
            {
                var buttonType = (string)FindResource("About:HeadpatsButton");
                var title = string.Format((string)FindResource("About:ImageError:Title"), buttonType);
                var message = string.Format((string)FindResource("About:ImageError:Message"), buttonType);

                MessageBox.Show($"{message}\n{ex.Message}", title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task<bool> Hug()
        {
            try
            {
                var image = await WeebCDN("hugs");
                HugImage.Load(image);

                return true;
            }
            catch (Exception ex)
            {
                var buttonType = (string)FindResource("About:HugsButton");
                var title = string.Format((string)FindResource("About:ImageError:Title"), buttonType);
                var message = string.Format((string)FindResource("About:ImageError:Message"), buttonType);

                MessageBox.Show($"{message}\n{ex.Message}", title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
