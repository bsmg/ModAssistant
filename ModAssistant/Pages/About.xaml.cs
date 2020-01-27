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

        private async void HugsButton_Click(object sender, RoutedEventArgs e)
        {
            HugButton.IsEnabled = false;
            await Task.Run(() => Hug());
            HugUp.IsOpen = true;
        }

        private string WeebCDN(string type)
        {
            Utils.WeebCDNRandomResponse Response;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Utils.Constants.WeebCDNAPIURL + type + "/random");
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.UserAgent = "ModAssistant/" + App.Version;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                var serializer = new JavaScriptSerializer();
                Response = serializer.Deserialize<Utils.WeebCDNRandomResponse>(reader.ReadToEnd());
            }
            return Response.url;
        }

        private void HeadPat()
        {
            PatImage.Load(WeebCDN("pats"));
        }

        private void Hug()
        {
            HugImage.Load(WeebCDN("hugs"));
        }
    }
}
