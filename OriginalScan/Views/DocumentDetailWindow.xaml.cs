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
                txtDocumentName.IsReadOnly = false;
                txtNote.IsReadOnly = false;
            }
            else
            {
                txtDocumentName.IsReadOnly = true;
                txtNote.IsReadOnly = true;
                btnEdit.Visibility = Visibility.Collapsed;
            }
        }

        private string CheckDocumentField()
        {
            string notification = string.Empty;
            if (txtDocumentName.Text.Trim() == "")
                notification += "Tên tài liệu không được để trống! \n";

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
                        Note = txtNote.Text
                    };

                    var checkExistedResult = await _documentService.CheckExisted(_currentBatch.Id, txtDocumentName.Text);
                    if (checkExistedResult && txtDocumentName.Text != _currentDocument.DocumentName)
                    {
                        NotificationShow("warning", "Tên tài liệu bị trùng lặp!");
                        return;
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
                            DocumentPath = txtPath.Text
                        };

                        _documentService.SetDocument(documentModel);
                    }

                    BatchWindow? batchManagerWindow = System.Windows.Application.Current.Windows.OfType<BatchWindow>().FirstOrDefault();
                    if (batchManagerWindow != null)
                        batchManagerWindow.GetDocumentsByBatch(_currentBatch.Id);

                    MainWindow? mainWindow = System.Windows.Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                    if (mainWindow != null)
                        mainWindow.LoadDirectoryTree();

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
    }
}
