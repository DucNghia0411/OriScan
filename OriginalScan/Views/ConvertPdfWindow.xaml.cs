using FontAwesome5;
using Notification.Wpf;
using Notification.Wpf.Constants;
using Notification.Wpf.Controls;
using Notification.Wpf.View;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using ScanApp.Common.Common;
using ScanApp.Common.Settings;
using ScanApp.Data.Entities;
using ScanApp.Intergration.Constracts;
using ScanApp.Model.Models;
using ScanApp.Model.Requests.Document;
using ScanApp.Service.Constracts;
using System;
using System.Collections.Generic;
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

namespace OriginalScan.Views
{
    /// <summary>
    /// Interaction logic for ConvertPdfWindow.xaml
    /// </summary>
    public partial class ConvertPdfWindow : Window
    {
        private readonly IBatchService _batchService;
        private readonly IDocumentService _documentService;
        private readonly ScanContext _context;
        private readonly NotificationManager _notificationManager;
        private readonly ITransferApiClient _transferApiClient;

        public ConvertPdfWindow
        (
            ScanContext context,
            IBatchService batchService,
            IDocumentService documentService,
            ITransferApiClient transferApiClient
        )
        {
            _context = context;
            _batchService = batchService;
            _documentService = documentService;
            _transferApiClient = transferApiClient;
            _notificationManager = new NotificationManager();

            InitializeComponent();
            this.ResizeMode = ResizeMode.NoResize;
            GetDocumentsByBatch();
            txtApi.Text = _transferApiClient.Api;
            NotificationConstants.MessagePosition = NotificationPosition.TopRight;
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

        public async void GetDocumentsByBatch()
        {
            if (_batchService.SelectedBatch == null)
            {
                return;
            }

            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            userFolderPath = userFolderPath.Replace("/", "\\");

            IEnumerable<ScanApp.Data.Entities.Document> documents = await _documentService.Get(x => x.BatchId == _batchService.SelectedBatch.Id);

            List<object> return_data = new List<object>();
            object? selectedItem = null;

            foreach (ScanApp.Data.Entities.Document document in documents)
            {
                string status;
                if (document.PdfPath == null)
                {
                    status = "Chưa chuyển thành PDF";
                }
                else status = "Đã chuyển thành PDF";

                var obj = new
                {
                    Id = document.Id,
                    DocumentName = document.DocumentName,
                    DocumentPath = document.PdfPath,
                    Status = status,
                    CreatedDate = document.CreatedDate
                };
                return_data.Add(obj);

                if (_documentService.SelectedDocument != null && _documentService.SelectedDocument.Id == document.Id)
                {
                    selectedItem = obj;
                }
            }

            lstvDocuments.ItemsSource = return_data;

            if (selectedItem != null)
            {
                lstvDocuments.SelectedItem = selectedItem;
            }
        }

        private void btnViewDocument_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Controls.Button? clickedButton = sender as System.Windows.Controls.Button;
                if (clickedButton == null)
                    return;

                var dataContext = clickedButton.DataContext;
                lstvDocuments.SelectedItem = dataContext;

                if (_batchService.SelectedBatch == null)
                {
                    return;
                }
                DocumentModel selectedDocument = ValueConverter.ConvertToObject<DocumentModel>(dataContext);

                _documentService.SetDocument(selectedDocument);

                DocumentDetailWindow documentDetailWindow = new DocumentDetailWindow(_documentService, false, _batchService.SelectedBatch);
                documentDetailWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                NotificationShow("error", $"{ex.Message}");
                return;
            }
        }

        private async void btnConvertToPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Controls.Button? clickedButton = sender as System.Windows.Controls.Button;
                if (clickedButton == null)
                    return;

                var dataContext = clickedButton.DataContext;
                lstvDocuments.SelectedItem = dataContext;
                DocumentModel selectedDocument = ValueConverter.ConvertToObject<DocumentModel>(dataContext);

                var currentDocument = await _documentService.FirstOrDefault(e => e.Id == selectedDocument.Id);

                if (currentDocument == null) return;

                string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string systemPath = System.IO.Path.Combine(FolderSetting.AppFolder, FolderSetting.Images);
                string pdfPath = System.IO.Path.Combine(FolderSetting.AppFolder, FolderSetting.PDFs);
                string defaultPath = System.IO.Path.Combine(userFolderPath, systemPath);

                if (!Directory.Exists(defaultPath))
                    defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                string path = System.IO.Path.Combine(userFolderPath, currentDocument.DocumentPath);

                if (Directory.Exists(path))
                {
                    string[] images = Directory.GetFiles(path);
                    string folderName = System.IO.Path.GetFileName(path);

                    if (images.Count() == 0)
                    {
                        NotificationShow("error", $"Không có hình ảnh trong thư mục!");
                        return;
                    }

                    string pdfFileName = folderName + ".pdf";
                    string folderPath = System.IO.Path.Combine(userFolderPath, pdfPath);
                    Directory.CreateDirectory(folderPath);
                    string pdfFilePath = System.IO.Path.Combine(userFolderPath, pdfPath, pdfFileName);

                    string shortPdfPath = System.IO.Path.Combine(pdfPath, pdfFileName);
                    DocumentToPdfRequest request = new DocumentToPdfRequest()
                    {
                        Id = selectedDocument.Id,
                        PdfPath = shortPdfPath,
                    };

                    var updateResult = await _documentService.UpdatePdfPath(request);

                    if (updateResult == 0)
                    {
                        NotificationShow("error", "Cập nhật không thành công!");
                        return;
                    }

                    if (File.Exists(pdfFilePath))
                    {
                        MessageBoxResult pdfConfirm = System.Windows.MessageBox.Show("Đã tồn tại một tệp PDF có cùng tên. Bạn có muốn thay thế nó?", "Thông báo!", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (pdfConfirm == MessageBoxResult.Yes)
                        {
                            try
                            {
                                File.Delete(pdfFileName);
                            }
                            catch (Exception ex)
                            {
                                NotificationShow("error", $"{ex.Message}");
                                return;
                            }
                        }
                        else
                            return;
                    }

                    PdfDocument pdfDocument = new PdfDocument();

                    foreach (string imagePath in images)
                    {
                        using (var image = XImage.FromFile(imagePath))
                        {
                            PdfPage page = pdfDocument.AddPage();
                            page.Width = image.PixelWidth;
                            page.Height = image.PixelHeight;

                            XGraphics gfx = XGraphics.FromPdfPage(page);
                            gfx.DrawImage(image, 0, 0, page.Width, page.Height);
                        }
                    }

                    pdfDocument.Save(pdfFilePath);
                    GetDocumentsByBatch();
                    NotificationShow("success", $"Lưu thành công tại đường dẫn: {pdfFilePath}");
                }
            }
            catch (Exception ex)
            {
                NotificationShow("error", ex.Message);
                return;
            }
        }

        private async void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Controls.Button? clickedButton = sender as System.Windows.Controls.Button;
                if (clickedButton == null)
                    return;

                var dataContext = clickedButton.DataContext;
                lstvDocuments.SelectedItem = dataContext;
                DocumentModel selectedDocument = ValueConverter.ConvertToObject<DocumentModel>(dataContext);

                var currentDocument = await _documentService.FirstOrDefault(e => e.Id == selectedDocument.Id);

                if (currentDocument == null) 
                {
                    NotificationShow("warning", "Không tìm thấy thông tin tài liệu được chọn!");
                    return;
                }

                if (currentDocument.PdfPath == null)
                {
                    NotificationShow("warning", "Tài liệu này chưa được chuyển thành PDF!");
                    return;
                }

                string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var pdfFilePath = System.IO.Path.Combine(userFolderPath, currentDocument.PdfPath);

                if (_transferApiClient.Api.Trim() != txtApi.Text.Trim())
                {
                    _transferApiClient.UpdateApiAddress(txtApi.Text);
                    NotificationShow("success", "Cập nhật đường dẫn mới thành công!");
                }
                bool transferResult = await _transferApiClient.TransferToPortal(pdfFilePath);

                await ShowProgressBar();

                if (transferResult)
                    NotificationShow("success", $"Upload công văn thành công!");
                else
                {
                    NotificationShow("error", "Upload công văn thất bại!");
                    return;
                }
            }
            catch (Exception ex)
            {
                NotificationShow("error", ex.Message);
                return;
            }
        }

        public async Task ShowProgressBar()
        {
            using var progress = _notificationManager.ShowProgressBar(
            "Đang tải lên công văn",
            false,
            true,
            "",
            false,
            1,
            "",
            false,
            false, 
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF444444")),
            new SolidColorBrush(Colors.White),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF01D328")),
            null,
            null,
            null,
            true
        );

            await Task.Run(async () =>
            {
                for (var i = 0; i <= 100; i++)
                {
                    progress.Cancel.ThrowIfCancellationRequested();
                    progress.Report((i, $"Tiến độ: {i}%", "Đang tải lên công văn", true));
                    await Task.Delay(TimeSpan.FromSeconds(0.05), progress.Cancel).ConfigureAwait(false);
                }
            }, progress.Cancel).ConfigureAwait(false);

            
        }
    }
}
