using System;
using System.Windows;
using System.Windows.Data;

namespace ModAssistant
{
    /// <summary>
    /// Interaction logic for OneClickStatus.xaml
    /// </summary>
    public partial class OneClickStatus : Window
    {
        public static OneClickStatus Instance;

        public string HistoryText
        {
            get
            {
                return HistoryTextBlock.Text;
            }
            set
            {
                Dispatcher.Invoke(new Action(() => { OneClickStatus.Instance.HistoryTextBlock.Text = value; }));
            }
        }
        public string MainText
        {
            get
            {
                return HistoryTextBlock.Text;
            }
            set
            {
                Dispatcher.Invoke(new Action(() => {
                    OneClickStatus.Instance.HistoryTextBlock.Text = string.IsNullOrEmpty(MainText) ? $"{value}" : $"{value}\n{MainText}";
                }));
            }
        }

        public OneClickStatus()
        {
            InitializeComponent();
            Instance = this;
        }
    }

    [ValueConversion(typeof(double), typeof(double))]
    public class DivideDoubleByTwoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(double))
            {
                throw new InvalidOperationException("The target must be a double");
            }
            double d = (double)value;
            return ((double)d) / 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
