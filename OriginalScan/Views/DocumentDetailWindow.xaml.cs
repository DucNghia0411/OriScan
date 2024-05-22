using FontAwesome5;
using Notification.Wpf;
using Notification.Wpf.Constants;
using Notification.Wpf.Controls;
using ScanApp.Data.Entities;
using ScanApp.Model.Models;
using ScanApp.Model.Requests.Batch;
using ScanApp.Model.Requests.Document;
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace OriginalScan.Views
{
    /// <summary>
    /// Interaction logic for DocumentDetailWindow.xaml
    /// </summary>
    public partial class DocumentDetailWindow : Window
    {
        private readonly IDocumentService _documentService;
        private readonly NotificationManager _notificationManager;

        public bool IsEdit { get; set; }
        public BatchModel? _currentBatch { get; set; }
        public ScanApp.Data.Entities.Document? _currentDocument { get; set; }

        public DocumentDetailWindow(IDocumentService documentService, bool isEdit, BatchModel currentBatch)
        {
            _documentService = documentService;
            IsEdit = isEdit;
            _notificationManager = new NotificationManager();
            _currentBatch = currentBatch;
            NotificationConstants.MessagePosition = NotificationPosition.TopRight;

            InitializeComponent();
            GetDocument();
            GetTask();
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

        public async void GetDocument()
        {
            try
            {
                var documentModel = _documentService.SelectedDocument;

                if (documentModel == null)
                {
                    NotificationShow("error", "Không nhận được thông tin tài liệu.");
                    return;
                }

                var document = await _documentService.FirstOrDefault(e => e.Id == documentModel.Id);

                if (document != null && _currentBatch != null)
                {
                    _currentDocument = document;

                    txtDocumentName.Text = document.DocumentName;
                    txtBatchName.Text = _currentBatch.BatchName;
                    txtNote.Text = document.Note;
                    txtCreatedDate.Text = document.CreatedDate;
                    txtPath.Text = document.DocumentPath;
                    txtAgencyIdentifier.Text = document.AgencyIdentifier;
                    txtDocIdentifier.Text = document.DocumentIdentifier;
                    txtNumOfSheets.Text = document.NumberOfSheets.ToString();
                    
                    txtStoragePeriod.Text = document.StoragePeriod;
                    if (document.StartDate != null && document.StartDate != string.Empty)
                    {
                        dpkStartDate.SelectedDate = DateTime.Parse(document.StartDate);
                    }
                    if (document.EndDate != null && document.EndDate != string.Empty)
                    {
                        dpkEndDate.SelectedDate = DateTime.Parse(document.EndDate);
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationShow("error", ex.Message);
                return;
            }
        }

        public void GetTask()
        {
            if (IsEdit)
            {
                txtDocumentName.IsEnabled = true;
                txtNote.IsEnabled = true;
                txtAgencyIdentifier.IsEnabled = true;
                txtDocIdentifier.IsEnabled = true;
                dpkStartDate.IsEnabled = true;
                dpkEndDate.IsEnabled = true;
                txtStoragePeriod.IsEnabled = true;
            }
            else
            {
                txtDocumentName.IsEnabled = false;
                txtNote.IsEnabled = false;
                txtAgencyIdentifier.IsEnabled = false;
                txtDocIdentifier.IsEnabled = false;
                dpkStartDate.IsEnabled = false;
                dpkEndDate.IsEnabled = false;
                txtStoragePeriod.IsEnabled = false;
                btnEdit.Visibility = Visibility.Collapsed;
            }
        }

        private string CheckDocumentField()
        {
            string notification = string.Empty;
            if (txtDocumentName.Text.Trim() == "")
                notification += "Tên tài liệu không được để trống! \n";

            if (txtNumOfSheets.Text.Trim() == "")
                notification += "Số tờ không được để trống! \n";

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

        private void btnCancelDocument_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private async void CbtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (CheckDocumentField() != "")
            {
                NotificationShow("warning", CheckDocumentField());
                return;
            }

            try
            {
                if (_currentDocument == null || _currentBatch == null)
                {
                    return;
                }

                MessageBoxResult Result = System.Windows.MessageBox.Show($"Bạn muốn sửa tài liệu: {_currentDocument.DocumentName}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (Result == MessageBoxResult.Yes)
                {
                    DocumentUpdateRequest request = new DocumentUpdateRequest()
                    {
                        Id = _currentDocument.Id,
                        DocumentName = txtDocumentName.Text,
                        Note = txtNote.Text,
                        AgencyIdentifier = txtAgencyIdentifier.Text,
                        DocumentIdentifier = txtDocIdentifier.Text,
                        NumberOfSheets = int.Parse(txtNumOfSheets.Text),
                        StartDate = dpkStartDate.Text,
                        EndDate = dpkEndDate.Text,
                        StoragePeriod = txtStoragePeriod.Text
                    };                    

                    if (txtDocumentName.Text != _currentDocument.DocumentName)
                    {
                        var checkExistedResult = await _documentService.CheckExisted(_currentBatch.Id, txtDocumentName.Text);
                        if (checkExistedResult)
                        {
                            NotificationShow("warning", "Tên tài liệu bị trùng lặp!");
                            return;
                        }
                    }

                    var updateResult = await _documentService.Update(request);

                    if (updateResult == 0)
                    {
                        NotificationShow("error", "Cập nhật không thành công!");
                        return;
                    }
                    else
                    {
                        DocumentModel documentModel = new DocumentModel()
                        {
                            Id = _currentDocument.Id,
                            BatchId = _currentBatch.Id,
                            DocumentName = txtDocumentName.Text,
                            DocumentPath = txtPath.Text,
                            NumberOfSheets = int.Parse(txtNumOfSheets.Text)
                        };

                        _documentService.SetDocument(documentModel);
                    }

                    BatchWindow? batchManagerWindow = System.Windows.Application.Current.Windows.OfType<BatchWindow>().FirstOrDefault();
                    if (batchManagerWindow != null)
                        batchManagerWindow.GetDocumentsByBatch(_currentBatch.Id);

                    NotificationShow("success", $"Cập nhật thành công tài liệu với id: {updateResult}");
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void txtNumOfSheets_KeyDown(object sender, KeyEventArgs e)
        {
            if (!((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)))
            {
                e.Handled = true;
            }
        }

        private void dpkEndDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpkStartDate.SelectedDate == null)
            {
                return;
            }

            if (dpkEndDate.SelectedDate < dpkStartDate.SelectedDate)
            {
                NotificationShow("error", $"Ngày kết thúc không thể trước ngày bắt đầu");
                dpkEndDate.SelectedDate = null;
                return;
            }
        }

        private void dpkStartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpkEndDate.SelectedDate == null)
            {
                return;
            }

            if (dpkEndDate.SelectedDate < dpkStartDate.SelectedDate)
            {
                NotificationShow("error", $"Ngày kết thúc không thể trước ngày bắt đầu");
                dpkStartDate.SelectedDate = null;
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

        private void dpkStartDate_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (dpkStartDate.SelectedDate != null)
            {
                dpkStartDate.Text = dpkStartDate.SelectedDate.ToString();
            }
            else
            {
                dpkStartDate.Text = string.Empty;
            }
        }

        private void dpkEndDate_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (dpkEndDate.SelectedDate != null)
            {
                dpkEndDate.Text = dpkEndDate.SelectedDate.ToString();
            }
            else
            {
                dpkEndDate.Text = string.Empty;
            }
        }
    }
}
