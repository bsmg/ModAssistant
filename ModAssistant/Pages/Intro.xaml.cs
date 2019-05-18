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
            MessageBox.Show("Closing Application: You did not agree to terms and conditions.");
            System.Windows.Application.Current.Shutdown();
        }

        private void Agree_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(MainWindow.GameVersion))
            {
                MessageBox.Show("Could not download versions list.\nMods tab disabled. Please restart to try again.");
            }
            else
            {
                MainWindow.Instance.ModsButton.IsEnabled = true;
                Utils.SendNotify("You can now use the Mods tab!");
                MainWindow.Instance.MainText = "You can now use the Mods tab!";
            }
            Properties.Settings.Default.Agreed = true;
            Properties.Settings.Default.Save();
        }
    }
}
