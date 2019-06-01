using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ModAssistant.Pages
{
    /// <summary>
    /// Interaction logic for Invalid.xaml
    /// </summary>
    public partial class Invalid : Page
    {
        public static Invalid Instance = new Invalid();

        public Invalid()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}