using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            await Task.Run(() => HeadPat());
            PatUp.IsOpen = true;
        }

        private void HeadPat()
        {
            Utils.WeebCDNRandomResponse Pat;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Utils.Constants.WeebCDNAPIURL + "pats/random");
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.UserAgent = "ModAssistant/" + App.Version;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                var serializer = new JavaScriptSerializer();
                Pat = serializer.Deserialize<Utils.WeebCDNRandomResponse>(reader.ReadToEnd());
            }
            PatImage.Load(Pat.url);
        }
    }
}
