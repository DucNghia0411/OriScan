using FontAwesome5;
using Notification.Wpf;
using NTwain;
using NTwain.Data;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OriginalScan.Views
{
    /// <summary>
    /// Interaction logic for DeviceWindow.xaml
    /// </summary>
    public partial class DeviceWindow : Window
    {
        public TwainSession? twainSession;
        public MainWindow? mainWindow;
        public DataSource? dataSource;
        private readonly NotificationManager _notificationManager;

        public IEnumerable<DataSource> dataSources = Enumerable.Empty<DataSource>();

        public DeviceWindow()
        {
            _notificationManager = new NotificationManager();

            InitializeComponent();
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

        public void GetListDevice()
        {
            try
            {
                if (twainSession != null)
                {
                    dataSources = twainSession.GetSources().ToList();
                    lbDevice.ItemsSource = dataSources;

                    if (dataSource != null)
                    {
                        foreach (var item in lbDevice.Items)
                        {
                            DataSource source = (DataSource)item;

                            if (source == dataSource)
                            {
                                lbDevice.SelectedItem = item;
                                SettingDevice(dataSource);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationShow("error", $"{ex.Message}");
                return;
            }   
        }

        private void SettingDevice(DataSource data)
        {
            try
            {
                if (data == null)
                {
                    NotificationShow("warning", $"Bạn chưa chọn máy scan");
                    return;
                }
                if (mainWindow != null)
                {
                    mainWindow.dataSource = data;
                    txtCurrenDevice.Text = data.Name;                   
                }

                dataSource = data;
            }
            catch (Exception ex)
            {
                NotificationShow("error", $"{ex.Message}");
                return;
            }
        }

        private void lblDevice_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DataSource dataSource = (DataSource)lbDevice.SelectedItem;
                SettingDevice(dataSource);
            }
            catch (Exception ex)
            {
                NotificationShow("error", $"{ex.Message}");
                return;
            }
        }

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            if (lbDevice.SelectedItem == null || dataSource == null)
            {
                NotificationShow("warning", $"Bạn chưa chọn máy scan");
                return;
            }

            ProfileSettingWindow profileSettingWindow = new ProfileSettingWindow();
            profileSettingWindow.twainSession = twainSession;
            profileSettingWindow.deviceWindow = this;
            profileSettingWindow.mainWindow = mainWindow;
            profileSettingWindow.dataSource = dataSource;
            if (twainSession != null && !twainSession.IsSourceOpen)
                dataSource.Open();
            profileSettingWindow.LoadData(dataSource);

            profileSettingWindow.ShowDialog();
        }
    }
}
