using FontAwesome5;
using Notification.Wpf;
using ScanApp.Data.Entities;
using ScanApp.Model.Models;
using ScanApp.Model.Requests.Batch;
using ScanApp.Service.Constracts;
using ScanApp.Service.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
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
    /// Interaction logic for BatchDetailWindow.xaml
    /// </summary>
    public partial class BatchDetailWindow : Window
    {
        private readonly IBatchService _batchService;
        private readonly NotificationManager _notificationManager;

        public bool IsEdit { get; set; }
        public Batch? _currentBatch { get; set; }

        public BatchDetailWindow(IBatchService batchService, bool isEdit)
        {
            _batchService = batchService;
            IsEdit = isEdit;
            _notificationManager = new NotificationManager();
            InitializeComponent();
            GetBatch();
            GetTask();
        }

        public async void GetBatch()
        {
            try
            {
                var batchModel = _batchService.SelectedBatch;

                if (batchModel == null)
                {
                    NotificationShow("error", "Không nhận được thông tin gói tài liệu.");
                    return;
                }

                var batch = await _batchService.FirstOrDefault(e => e.Id == batchModel.Id);

                if (batch != null)
                {
                    _currentBatch = batch;

                    txtBatchName.Text = batch.BatchName;
                    txtNote.Text = batch.Note;
                    txtCreatedDate.Text = batch.CreatedDate;
                    txtPath.Text = batch.BatchPath;
                }
            }
            catch(Exception ex)
            {
                NotificationShow("error", ex.Message);
                return;
            }
        }

        public void GetTask()
        {
            if (IsEdit)
            {
                txtBatchName.IsReadOnly = false;
                txtNote.IsReadOnly = false;
            }
            else
            {
                txtBatchName.IsReadOnly = true;
                txtNote.IsReadOnly = true;
                btnEdit.Visibility = Visibility.Collapsed;
            }
        }

        private string CheckBatchField()
        {
            string notification = string.Empty;
            if (txtBatchName.Text.Trim() == "")
                notification += "Tên gói tài liệu không được để trống! \n";

            return notification;
        }

        void NotificationShow(string type, string message)
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void btnCancelDocument_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void CbtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBatchField() != "")
            {
                NotificationShow("warning", CheckBatchField());
                return;
            }

            try
            {
                if (_currentBatch == null)
                {
                    return;
                }

                _notificationManager.ShowButtonWindow($"Bạn muốn sửa gói: {_currentBatch.BatchName}?", "Xác nhận",
                    async () => {
                        BatchUpdateRequest request = new BatchUpdateRequest()
                        {
                            Id = _currentBatch.Id,
                            BatchName = txtBatchName.Text,
                            Note = txtNote.Text
                        };

                        var updateResult = await _batchService.Update(request);

                        if (updateResult == 0)
                        {
                            NotificationShow("error", "Cập nhật không thành công!");
                        }
                        else
                        {
                            NotificationShow("success", $"Cập nhật thành công gói tài liệu với id: {updateResult}");
                        }

                        this.Visibility = Visibility.Hidden;
                    }, "OK", () => { }, "Cancel");
               
            }
            catch (Exception ex)
            {
                NotificationShow("error", $"Có lỗi: {ex.Message}");
                return;
            }
        }
    }
}
