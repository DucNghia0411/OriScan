using FontAwesome5;
using Notification.Wpf;
using Notification.Wpf.Constants;
using Notification.Wpf.Controls;
using NTwain.Data;
using ScanApp.Data.Entities;
using ScanApp.Model.Models;
using ScanApp.Model.Requests.Batch;
using ScanApp.Service.Constracts;
using ScanApp.Service.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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

            NotificationConstants.MessagePosition = NotificationPosition.TopRight;
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
                    txtPath.Text = batch.BatchPath;
                    txtNote.Text = batch.Note;
                    txtCreatedDate.Text = batch.CreatedDate;
                    txtNumberingFont.Text = batch.NumberingFont;
                    txtDocRack.Text = batch.DocumentRack;
                    txtDocShelf.Text = batch.DocumentShelf;
                    txtNumTableOfContents.Text = batch.NumericalTableOfContents;
                    txtFileCabinet.Text = batch.FileCabinet;
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
                txtBatchName.IsEnabled = true;
                txtNote.IsEnabled = true;
                txtNumberingFont.IsEnabled = true;
                txtDocRack.IsEnabled = true;
                txtDocShelf.IsEnabled = true;
                txtNumTableOfContents.IsEnabled = true;
                txtFileCabinet.IsEnabled = true;
            }
            else
            {
                txtBatchName.IsEnabled = false;
                txtNote.IsEnabled = false;
                txtNumberingFont.IsEnabled = false;
                txtDocRack.IsEnabled = false;
                txtDocShelf.IsEnabled = false;
                txtNumTableOfContents.IsEnabled = false;
                txtFileCabinet.IsEnabled = false;

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

        private async void CbtnEdit_Click(object sender, RoutedEventArgs e)
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

                MessageBoxResult Result = System.Windows.MessageBox.Show($"Bạn muốn sửa gói: {_currentBatch.BatchName}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (Result == MessageBoxResult.Yes)
                {
                    BatchUpdateRequest request = new BatchUpdateRequest()
                    {
                        Id = _currentBatch.Id,
                        BatchName = txtBatchName.Text,
                        Note = txtNote.Text,
                        NumberingFont = txtNumberingFont.Text,
                        DocumentRack = txtDocRack.Text,
                        DocumentShelf = txtDocShelf.Text,
                        NumericalTableOfContents = txtNumTableOfContents.Text,
                        FileCabinet = txtFileCabinet.Text
                    };

                    if (txtBatchName.Text.Trim() != _currentBatch.BatchName.Trim())
                    {
                        var checkExistedResult = await _batchService.CheckExisted(txtBatchName.Text);

                        if (checkExistedResult)
                        {
                            NotificationShow("warning", "Tên gói bị trùng lặp!");
                            return;
                        }
                    }
                    
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
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                NotificationShow("error", $"Có lỗi: {ex.Message}");
                return;
            }
        }

        private void CbtnPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                userFolderPath = userFolderPath.Replace("/", "\\");
                string folderPath = System.IO.Path.Combine(userFolderPath, txtPath.Text);

                if (!Directory.Exists(folderPath))
                {
                    NotificationShow("error", $"Không tìm thấy đường dẫn: {folderPath}");
                    return;
                }

                Process.Start("explorer.exe", folderPath);
            }
            catch (Exception ex)
            {
                NotificationShow("error", $"Có lỗi: {ex.Message}");
                return;
            }
        }
    }
}
