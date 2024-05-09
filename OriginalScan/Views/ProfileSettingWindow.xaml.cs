using FontAwesome5;
using Notification.Wpf;
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
        private readonly NotificationManager _notificationManager;

        public ProfileSettingWindow()
        {
            _notificationManager = new NotificationManager();
            InitializeComponent();

            LoadData();

            this.ResizeMode = ResizeMode.NoResize;
            NotificationConstants.MessagePosition = NotificationPosition.TopRight;
        }

        void LoadData()
        {
            List<string> pageSources = new List<string>() { "Kính", "Khay" };
            List<string> resolutions = new List<string>() { "100 dpi", "150 dpi", "300 dpi" };
            List<string> bitDepths = new List<string>() { "24 bit", "30 bit", "36 bit" };
            List<string> horAligns = new List<string>() { "Phải", "Trái", "Giữa" };

            foreach (string pageSource in pageSources)
            {
                cbPaperSource.Items.Add(pageSource);
            }
            cbPaperSource.SelectedIndex = 0;

            for (int i = 0; i < 8; i++)
            {
                string pageSize = "A" + i;
                cbPageSize.Items.Add(pageSize);
            }
            cbPageSize.SelectedIndex = 0;

            foreach (string resolution in resolutions)
            {
                cbResolution.Items.Add(resolution);
            }
            cbResolution.SelectedIndex = 0;

            foreach (string bitDepth in bitDepths)
            {
                cbBitDepth.Items.Add(bitDepth);
            }
            cbBitDepth.SelectedIndex = 0;

            foreach (string horAlign in horAligns)
            {
                cbHorAlign.Items.Add(horAlign);
            }
            cbHorAlign.SelectedIndex = 0;

            for (int i = 1; i < 5; i++)
            {
                string scale = "1:" + i;
                cbScale.Items.Add(scale);
            }
            cbScale.SelectedIndex = 0;

            sldBrightness.Value = 50;
            sldContrast.Value = 50;
        }

        private void NotificationShow(string type, string message)
        {
            switch (type)
            {
                case "error":
                    {
                        var errorNoti = new NotificationContent
                        {
                            Title = "Lỗi!",
                            Message = $"Có lỗi: {message}",
                            Type = NotificationType.Error,
                            Icon = new SvgAwesome()
                            {
                                Icon = EFontAwesomeIcon.Solid_Times,
                                Height = 25,
                                Foreground = new SolidColorBrush(Colors.Black)
                            },
                            Background = new SolidColorBrush(Colors.Red),
                            Foreground = new SolidColorBrush(Colors.White),
                        };
                        _notificationManager.Show(errorNoti);
                        break;
                    }
                case "success":
                    {
                        var successNoti = new NotificationContent
                        {
                            Title = "Thành công!",
                            Message = $"{message}",
                            Type = NotificationType.Success,
                            Icon = new SvgAwesome()
                            {
                                Icon = EFontAwesomeIcon.Solid_Check,
                                Height = 25,
                                Foreground = new SolidColorBrush(Colors.Black)
                            },
                            Background = new SolidColorBrush(Colors.Green),
                            Foreground = new SolidColorBrush(Colors.White),
                        };
                        _notificationManager.Show(successNoti);
                        break;
                    }
                case "warning":
                    {
                        var warningNoti = new NotificationContent
                        {
                            Title = "Thông báo!",
                            Message = $"{message}",
                            Type = NotificationType.Warning,
                            Icon = new SvgAwesome()
                            {
                                Icon = EFontAwesomeIcon.Solid_ExclamationTriangle,
                                Height = 25,
                                Foreground = new SolidColorBrush(Colors.Black)
                            },
                            Background = new SolidColorBrush(Colors.Yellow),
                            Foreground = new SolidColorBrush(Colors.Black),
                        };
                        _notificationManager.Show(warningNoti);
                        break;
                    }
            }
        }

        private void sldBrightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txtBrightness.Text = sldBrightness.Value.ToString("N1");
        }

        private void sldContrast_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txtContrast.Text = sldContrast.Value.ToString("N1");
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            NotificationShow("success", $"Cài đặt thành công thiết bị {txtDevice.Text} với các thuộc tính:\n"
                                        + $"- Nguồn giấy: {cbPaperSource.Text}\n"
                                        + $"- Cỡ giấy: {cbPageSize.Text}\n"
                                        + $"- Độ phân giải: {cbResolution.Text}\n"
                                        + $"- Độ sâu Bit: {cbBitDepth.Text}\n"
                                        + $"- Căn lề: {cbHorAlign.Text}\n"
                                        + $"- Tỉ lệ: {cbScale.Text}\n"
                                        + $"- Độ sáng: {txtBrightness.Text}\n"
                                        + $"- Độ tương phản: {txtContrast.Text}");
            this.Visibility = Visibility.Hidden;
        }
    }
}
