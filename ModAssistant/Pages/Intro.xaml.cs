using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for Intro.xaml
    /// </summary>
    public partial class Intro : Page
    {
        public static Intro Instance = new Intro();

        public Intro()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Disagree_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.ModsButton.IsEnabled = false;
            Properties.Settings.Default.Agreed = false;
            Properties.Settings.Default.Save();
            MessageBox.Show((string)FindResource("Intro:ClosingApp"));
            System.Windows.Application.Current.Shutdown();
        }

        private void Agree_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(MainWindow.GameVersion))
            {
                string line1 = (string)FindResource("Intro:VersionDownloadFailed");
                string line2 = (string)FindResource("Intro:ModsTabDisabled");

                MessageBox.Show($"{line1}.\n{line2}");
            }
            else
            {
                MainWindow.Instance.ModsButton.IsEnabled = true;

                string text = (string)FindResource("Intro:ModsTabEnabled");
                Utils.SendNotify(text);
                MainWindow.Instance.MainText = text;
            }
            Properties.Settings.Default.Agreed = true;
            Properties.Settings.Default.Save();
        }
    }
}
