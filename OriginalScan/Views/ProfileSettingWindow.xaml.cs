using Notification.Wpf.Constants;
using Notification.Wpf.Controls;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace OriginalScan.Views
{
    /// <summary>
    /// Interaction logic for ProfileSettingWindow.xaml
    /// </summary>
    public partial class ProfileSettingWindow : Window
    {
        public ProfileSettingWindow()
        {
            InitializeComponent();

            sldBrightness.Value = 50;
            sldContrast.Value = 50;

            this.ResizeMode = ResizeMode.NoResize;
            NotificationConstants.MessagePosition = NotificationPosition.TopRight;
        }

        private void sldBrightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txtBrightness.Text = sldBrightness.Value.ToString("N2");
        }

        private void sldContrast_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txtContrast.Text = sldContrast.Value.ToString("N2");
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }
    }
}
