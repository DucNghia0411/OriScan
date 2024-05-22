using FontAwesome5;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Notification.Wpf;
using Notification.Wpf.Constants;
using Notification.Wpf.Controls;
using NTwain;
using NTwain.Data;
using OriginalScan.Models;
using ScanApp.Data.Entities;
using ScanApp.Model.Requests.DeviceSetting;
using ScanApp.Service.Constracts;
using ScanApp.Service.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
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
        public DataSource? dataSource;
        public DeviceWindow? deviceWindow;
        public MainWindow? mainWindow;
        public TwainSession? twainSession;
        private readonly IDeviceSettingService _deviceSettingService;

        public bool IsSavedSetting { get; set; }

        public ProfileSettingWindow(IDeviceSettingService deviceSettingService, bool isSavedSetting)
        {
            _deviceSettingService = deviceSettingService;
            _notificationManager = new NotificationManager();
            InitializeComponent();
            IsSavedSetting = isSavedSetting;
            if (!isSavedSetting)
            {
                btnDelete.Visibility = Visibility.Collapsed;
            }
            this.ResizeMode = ResizeMode.NoResize;
            NotificationConstants.MessagePosition = NotificationPosition.TopRight;
        }

        public string FormatDateTime(string? inputDate)
        {
            if (inputDate == null) return "";
            string formattedDate = "";
            CultureInfo currentCulture = CultureInfo.CurrentCulture;

            DateTimeFormatInfo dateTimeFormat = currentCulture.DateTimeFormat;
            string[] allDatePatterns = dateTimeFormat.GetAllDateTimePatterns();

            bool isConverted = false;

            foreach (string format in allDatePatterns)
            {
                DateTime createdDate;

                if (DateTime.TryParseExact(inputDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out createdDate))
                {
                    formattedDate = createdDate.ToString("dd/MM/yyyy h:mm:ss tt");
                    isConverted = true;
                }
            }

            if (!isConverted)
            {
                formattedDate = inputDate;
            }

            return formattedDate;
        }

        public void LoadData(DataSource src)
        {
            if (dataSource == null || twainSession == null) return;

            txtDevice.Text = dataSource.Name;

            List<string> listCapDup = new List<string>() { "1 mặt", "2 mặt" };
            cbCapDuplex.ItemsSource = listCapDup;
            cbCapDuplex.SelectedIndex = 1;

            if (!DeviceSettingConverter._isDefault && mainWindow != null)
            {
                mainWindow.SetupDevice();
                if (DeviceSettingConverter._duplex == true)
                {
                    cbCapDuplex.SelectedIndex = 1;
                }
                else cbCapDuplex.SelectedIndex = 0;
            }

            if (src.Capabilities.ICapBitDepth.IsSupported)
            {
                LoadDepth(src.Capabilities.ICapBitDepth);
            }

            if (src.Capabilities.ICapXResolution.IsSupported && src.Capabilities.ICapYResolution.IsSupported)
            {
                LoadDPI(src.Capabilities.ICapXResolution);
            }

            if (src.Capabilities.ICapSupportedSizes.IsSupported)
            {
                LoadPaperSize(src.Capabilities.ICapSupportedSizes);
            }

            if (src.Capabilities.ICapRotation.IsSupported)
            {
                LoadRotate(src.Capabilities.ICapRotation);
            }

            if (src.Capabilities.ICapPixelType.IsSupported)
            {
                LoadPixelType(src.Capabilities.ICapPixelType);
            }

            if (src.Capabilities.ICapBrightness.IsSupported)
            {
                LoadBrightness(src.Capabilities.ICapBrightness);
            }

            if (src.Capabilities.ICapContrast.IsSupported)
            {
                LoadContrast(src.Capabilities.ICapContrast);
            }
        }

        private void LoadDepth(ICapWrapper<int> cap)
        {
            var list = cap.GetValues().ToList();
            cbBitDepth.ItemsSource = list;
            var cur = cap.GetCurrent();
            if (list.Contains(cur))
            {
                cbBitDepth.SelectedItem = cur;
            }
        }

        private void LoadDPI(ICapWrapper<TWFix32> cap)
        {
            var list = cap.GetValues().Where(dpi => (dpi % 50) == 0).ToList();
            cbResolution.ItemsSource = list;
            var cur = cap.GetCurrent();
            if (list.Contains(cur))
            {
                cbResolution.SelectedItem = cur;
            }
        }

        private void LoadPaperSize(ICapWrapper<SupportedSize> cap)
        {
            var list = cap.GetValues().ToList();
            cbPageSize.ItemsSource = list;
            var cur = cap.GetCurrent();
            if (list.Contains(cur))
            {
                cbPageSize.SelectedItem = cur;
            }
        }

        private void LoadPixelType(ICapWrapper<PixelType> cap)
        {
            var list = cap.GetValues().ToList();
            cbPixelType.ItemsSource = list;
            var cur = cap.GetCurrent();
            if (list.Contains(cur))
            {
                cbPixelType.SelectedItem = cur;
            }
        }

        private void LoadRotate(ICapWrapper<TWFix32> cap)
        {
            var list = cap.GetValues().Where(degree => ((degree % 90) == 0) && (degree <= 270) && (degree >= 0)).ToList();
            cbRotateDegree.ItemsSource = list;
            var cur = cap.GetCurrent();
            if (list.Contains(cur))
            {
                cbRotateDegree.SelectedItem = cur;
            }
        }

        private void LoadContrast(ICapWrapper<TWFix32> cap)
        {
            var cur = cap.GetCurrent();
            sldContrast.Value = cur;
            txtContrast.Text = cur.ToString();
        }

        private void LoadBrightness(ICapWrapper<TWFix32> cap)
        {
            var cur = cap.GetCurrent();
            sldBrightness.Value = cur;
            txtBrightness.Text = cur.ToString();
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
            sldBrightness.Value = Math.Round(sldBrightness.Value / 8) * 8;
            txtBrightness.Text = sldBrightness.Value.ToString("N0");
        }

        private void sldContrast_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sldContrast.Value = Math.Round(sldContrast.Value / 8) * 8;
            txtContrast.Text = sldContrast.Value.ToString("N0");
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private async void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataSource == null || mainWindow == null) return;
                mainWindow.ClearSetting();

                bool isDuplex;
                if (cbCapDuplex.Text == "1 mặt")
                {
                    DeviceSettingConverter.SetDuplex(false);
                    isDuplex = false;
                }
                else
                {
                    DeviceSettingConverter.SetDuplex(true);
                    isDuplex = true;
                }

                if (cbPageSize.SelectedItem is SupportedSize selectedSize)
                {
                    DeviceSettingConverter.SetSize(selectedSize);
                }

                if (cbResolution.SelectedItem is TWFix32 selectedResolution)
                {
                    DeviceSettingConverter.SetDpi(selectedResolution);
                }

                if (cbPixelType.SelectedItem is PixelType selectedPixelType)
                {
                    DeviceSettingConverter.SetPixeType(selectedPixelType);
                }

                if (cbBitDepth.SelectedItem is int selectedBitDepth)
                {
                    DeviceSettingConverter.SetBitDepth(selectedBitDepth);
                }

                if (cbRotateDegree.SelectedItem is TWFix32 selectedRotateDegree)
                {
                    DeviceSettingConverter.SetRotateDegree(selectedRotateDegree);
                }

                int brightnessValue = int.Parse(txtBrightness.Text);
                TWFix32 brightness = new TWFix32
                {
                    Whole = (short)brightnessValue,
                    Fraction = 0
                };
                DeviceSettingConverter.SetBrightness(brightness);

                int contrastValue = int.Parse(txtContrast.Text);
                TWFix32 contrast = new TWFix32
                {
                    Whole = (short)contrastValue,
                    Fraction = 0
                };
                DeviceSettingConverter.SetContrast(contrast);
                DeviceSettingConverter._isDefault = false;

                if (IsSavedSetting && _deviceSettingService.SelectedSetting != null)
                {
                    DeviceSettingUpdateRequest request = new DeviceSettingUpdateRequest()
                    {
                        Id = _deviceSettingService.SelectedSetting.Id,
                        DeviceName = txtDevice.Text,
                        IsDuplex = isDuplex ? 2 : 1,
                        Size = cbPageSize.Text,
                        Dpi = Convert.ToInt32(cbResolution.SelectedItem.ToString()),
                        PixelType = cbPixelType.Text,
                        BitDepth = Convert.ToInt32(cbBitDepth.SelectedItem.ToString()),
                        RotateDegree = Convert.ToInt32(cbRotateDegree.SelectedItem.ToString()),
                        Brightness = Convert.ToInt32(brightness.ToString()),
                        Contrast = Convert.ToInt32(contrast.ToString()),
                    };

                    var settingId = await _deviceSettingService.Update(request);
                    NotificationShow("success", $"Cập nhật cấu hình thành công với id: {settingId}!");
                }
                else
                {
                    DeviceSettingCreateRequest request = new DeviceSettingCreateRequest()
                    {
                        DeviceName = txtDevice.Text,
                        IsDuplex = isDuplex ? 2 : 1,
                        Size = cbPageSize.Text,
                        Dpi = Convert.ToInt32(cbResolution.SelectedItem.ToString()),
                        PixelType = cbPixelType.Text,
                        BitDepth = Convert.ToInt32(cbBitDepth.SelectedItem.ToString()),
                        RotateDegree = Convert.ToInt32(cbRotateDegree.SelectedItem.ToString()),
                        Brightness = Convert.ToInt32(brightness.ToString()),
                        Contrast = Convert.ToInt32(contrast.ToString()),
                        CreatedDate = FormatDateTime(DateTime.Now.ToString())
                    };

                    var settingId = await _deviceSettingService.Create(request);
                    NotificationShow("success", $"Cài đặt thiết bị {txtDevice.Text} thành công và lưu cấu hình với id: {settingId}!");
                }
                
                if (deviceWindow != null)
                {
                    deviceWindow.GetListDevice();
                }
                
                this.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                NotificationShow("error", ex.Message);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_deviceSettingService.SelectedSetting == null) return; 

            MessageBoxResult Result = System.Windows.MessageBox.Show($"Bạn muốn xóa cấu hình của thiết bị: {txtDevice.Text} được tạo vào {FormatDateTime(_deviceSettingService.SelectedSetting.CreatedDate)}", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (Result == MessageBoxResult.Yes)
            {
                try
                {
                    _deviceSettingService.Delete(_deviceSettingService.SelectedSetting.Id);
                    NotificationShow("success", $"Xóa cấu hình thiết bị {txtDevice.Text} được tạo vào {FormatDateTime(_deviceSettingService.SelectedSetting.CreatedDate)} thành công!");
                    if (deviceWindow != null)
                    {
                        deviceWindow.GetListDevice();
                    }

                    this.Visibility = Visibility.Hidden;
                }
                catch (Exception ex)
                {
                    NotificationShow("error", $"{ex.Message}");
                    return;
                }
            }
            else
            {
                return;
            }
        }
    }
}
