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
    /// Interaction logic for Invalid.xaml
    /// </summary>
    public partial class Invalid : Page
    {
        public static Invalid Instance = new Invalid();
        public string InstallDirectory { get; set; }

        public Invalid()
        {
            InitializeComponent();
            InstallDirectory = App.BeatSaberInstallDirectory;
            DirectoryTextBlock.Text = InstallDirectory;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void SelectDirButton_Click(object sender, RoutedEventArgs e)
        {
            InstallDirectory = Utils.GetManualDir();
            DirectoryTextBlock.Text = InstallDirectory;
        }
    }
}
