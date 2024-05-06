using FontAwesome5;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;
using Notification.Wpf;
using Notification.Wpf.Classes;
using Notification.Wpf.Constants;
using Notification.Wpf.Controls;
using NTwain.Data;
using ScanApp.Common.Common;
using ScanApp.Common.Settings;
using ScanApp.Data.Entities;
using ScanApp.Model.Models;
using ScanApp.Model.Requests.Batch;
using ScanApp.Model.Requests.Document;
using ScanApp.Service.Constracts;
using ScanApp.Service.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.AccessControl;
using System.Security.Principal;
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
    /// Interaction logic for BatchWindow.xaml
    /// </summary>
    public partial class BatchWindow : Window
    {
        private readonly IBatchService _batchService;
        private readonly IDocumentService _documentService;
        private readonly ScanContext _context;
        private readonly NotificationManager _notificationManager;

        public BatchWindow(ScanContext context)
        {
            _batchService = new BatchService(context);
            _documentService = new DocumentService(context);
            _context = context;
            _notificationManager = new NotificationManager();
            InitializeComponent();
            GetBatches();
            NotificationConstants.MessagePosition = NotificationPosition.TopRight;
        }

        private async void CreateBatch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CheckBatchCreateField() != "")
                {
                    NotificationShow("error", CheckBatchCreateField());
                    return;
                }

                DateTime now = DateTime.Now;
                string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string systemPath = System.IO.Path.Combine(FolderSetting.AppFolder, FolderSetting.TempData, $"{txtBatchName.Text}_{now.ToString("yyyyMMddHHmmss")}");
                string path = System.IO.Path.Combine(userFolderPath, systemPath);

                BatchCreateRequest request = new BatchCreateRequest()
                {
                    BatchName = txtBatchName.Text,
                    Note = txtBatchNote.Text,
                    BatchPath = systemPath,
                    CreatedDate = now.ToString()
                };

                var checkExistedResult = await _batchService.CheckExisted(txtBatchName.Text);

                if (checkExistedResult)
                {
                    NotificationShow("warning", "Tên gói bị trùng lặp!");
                    return;
                }

                try
                {
                    DirectoryInfo directoryInfo = Directory.CreateDirectory(path);

                    DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();
                    string currentUser = WindowsIdentity.GetCurrent().Name;
                    FileSystemAccessRule accessRule = new FileSystemAccessRule(currentUser, FileSystemRights.Write,
                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                        PropagationFlags.None, AccessControlType.Allow);
                    directorySecurity.AddAccessRule(accessRule);
                    directoryInfo.SetAccessControl(directorySecurity);
                }
                catch (Exception ex)
                {
                    NotificationShow("error", $"Khởi tạo thư mục thất bại! Vui lòng cấp quyền cho hệ thống: {ex.Message}");
                    return;
                }

                int batchId = await _batchService.Create(request);
                Directory.CreateDirectory(path);

                NotificationShow("success", $"Tạo gói mới thành công với mã {batchId}.");

                lstvBatches.SelectedItems.Clear();
                ResetData();
                LoadTreeView();
            }
            catch (Exception ex)
            {
                NotificationShow("error", $"Tạo gói mới thất bại! Có lỗi: {ex.Message}");
                return;
            }
        }

        private async void GetBatches()
        {
            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            userFolderPath = userFolderPath.Replace("/", "\\");
            string path = System.IO.Path.Combine(userFolderPath, FolderSetting.AppFolder);

            IEnumerable<Batch> batches = await _batchService.GetAll();
            List<object> return_data = new List<object>();

            foreach (Batch batch in batches)
            {
                string formattedCreatedDate = "";

                DateTime createdDate;

                if (DateTime.TryParseExact(batch.CreatedDate, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out createdDate)
                    || DateTime.TryParseExact(batch.CreatedDate, "dd-MMM-yy h:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out createdDate))
                {
                    formattedCreatedDate = createdDate.ToString("M/d/yyyy h:mm:ss tt");
                }

                var obj = new
                {
                    Id = batch.Id,
                    BatchName = batch.BatchName,
                    BatchPath = batch.BatchPath,
                    Note = batch.Note,
                    CreatedDate = formattedCreatedDate
                };
                return_data.Add(obj);
            }

            lstvBatches.ItemsSource = return_data;
        }

        private string CheckBatchCreateField()
        {
            string notification = string.Empty;
            if (txtBatchName.Text.Trim() == "")
                notification += "Tên gói không được để trống! \n";
            return notification;
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

        private void ResetData()
        {
            GetBatches();
            GetDocumentsByBatch(0);
            txtBatchName.Text = string.Empty;
            txtBatchNote.Text = string.Empty;
            txtCurrentBatch.Text = string.Empty;
            txtCurrentDocument.Text = string.Empty;

            LoadTreeView();
        }

        public void LoadTreeView()
        {
            MainWindow? mainWindow = System.Windows.Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                if (_batchService.SelectedBatch == null || _batchService.SelectedBatch.BatchPath == null)
                {
                    mainWindow.trvBatchExplorer.Items.Clear();
                    return;
                }

                string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string folderPath = _batchService.SelectedBatch.BatchPath;
                string path = System.IO.Path.Combine(userFolderPath, folderPath);

                if (!Directory.Exists(path))
                {
                    return;
                }

                string name = System.IO.Path.GetFileName(path);

                var directoryItem = new TreeViewItem()
                {
                    Header = new StackPanel()
                    {
                        Orientation = System.Windows.Controls.Orientation.Horizontal,
                        Children =
                            {
                                new System.Windows.Controls.Image()
                                {
                                    Source = new BitmapImage(new Uri("/Resource/Images/foldericon.png", UriKind.Relative)),
                                    Width = 16,
                                    Height = 16,
                                    Margin = new Thickness(0, 10, 5, 0)
                                },
                                new TextBlock()
                                {
                                    Text = name,
                                    Margin = new Thickness(0, 10, 0, 0)
                                }
                            }
                    }
                };

                if (mainWindow.IsItemAlreadyExists(directoryItem, name))
                {
                    mainWindow.trvBatchExplorer.Items.Clear();
                } 

                mainWindow.trvBatchExplorer.Items.Add(directoryItem);
                mainWindow.LoadDirectory(directoryItem, path);
                mainWindow.ExpandTreeViewItem(path);
            }
        }

        private void btnCreateDocument_Click(object sender, RoutedEventArgs e)
        {
            CreateDocumentWindow createDocumentWindow = new CreateDocumentWindow(_context, _batchService);
            createDocumentWindow.ShowDialog();
            lstvDocuments.SelectedItems.Clear();
            LoadTreeView();
        }

        private void lstvBatches_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (lstvBatches.SelectedItem == null)
                {
                    return;
                }

                BatchModel selectedBatch = ValueConverter.ConvertToObject<BatchModel>(lstvBatches.SelectedItem);

                if (_batchService.SelectedBatch == selectedBatch)
                {
                    return;
                }
                _batchService.SetBatch(selectedBatch);

                GetDocumentsByBatch(selectedBatch.Id);
                txtCurrentBatch.Text = selectedBatch.BatchName;
                txtCurrentDocument.Text = string.Empty;

                LoadTreeView();
            }
            catch (Exception ex)
            {
                NotificationShow("error", $"Có lỗi: {ex.Message}");
                return;
            }
        }

        private void btnEditBatch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Controls.Button? clickedButton = sender as System.Windows.Controls.Button;
                if (clickedButton == null)
                    return;

                var dataContext = clickedButton.DataContext;
                lstvBatches.SelectedItem = dataContext;

                BatchModel selectedBatch = ValueConverter.ConvertToObject<BatchModel>(dataContext);

                _batchService.SetBatch(selectedBatch);

                BatchDetailWindow batchDetailWindow = new BatchDetailWindow(_batchService, true);
                batchDetailWindow.ShowDialog();

                ResetData();
            }
            catch (Exception ex)
            {
                NotificationShow("error", $"Sửa thất bại! Có lỗi: {ex.Message}");
                return;
            }
        }

        private async void btnDeleteBatch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Controls.Button? clickedButton = sender as System.Windows.Controls.Button;
                if (clickedButton == null)
                    return;

                var dataContext = clickedButton.DataContext;
                lstvBatches.SelectedItem = dataContext;

                BatchModel selectedBatch = ValueConverter.ConvertToObject<BatchModel>(dataContext);

                MessageBoxResult Result = System.Windows.MessageBox.Show($"Bạn muốn xóa gói: {selectedBatch.BatchName} và tất cả tài liệu?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (Result == MessageBoxResult.Yes)
                {
                    try
                    {
                        GetDocumentsByBatch(0);
                        _documentService.SetDocument(new DocumentModel());

                        var documentDelete = await _documentService.DeleteByBatch(selectedBatch.Id);

                        string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        string folderPath = selectedBatch.BatchPath;
                        string path = System.IO.Path.Combine(userFolderPath, folderPath);

                        try
                        {
                            Directory.Delete(path, true);
                        }
                        catch (DirectoryNotFoundException) { }

                        var deleteResult = await _batchService.Delete(selectedBatch.Id);

                        if (deleteResult)
                        {
                            NotificationShow("success", $"Xóa thành công gói tài liệu {selectedBatch.BatchName}");
                            _batchService.SetBatch(new BatchModel());

                            ResetData();
                        }
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
            catch (Exception ex)
            {
                NotificationShow("error", $"Xóa thất bại! {ex.Message}");
                return;
            }
        }

        public async void GetDocumentsByBatch(int batchId)
        {
            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            userFolderPath = userFolderPath.Replace("/", "\\");

            IEnumerable<ScanApp.Data.Entities.Document> documents = await _documentService.Get(x => x.BatchId == batchId);

            List<object> return_data = new List<object>();

            foreach (ScanApp.Data.Entities.Document document in documents)
            {
                string formattedCreatedDate = "";

                DateTime createdDate;

                if (DateTime.TryParseExact(document.CreatedDate, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out createdDate)
                    || DateTime.TryParseExact(document.CreatedDate, "dd-MMM-yy h:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out createdDate))
                {
                    formattedCreatedDate = createdDate.ToString("M/d/yyyy h:mm:ss tt");
                }

                var obj = new
                {
                    Id = document.Id,
                    DocumentName = document.DocumentName,
                    DocumentPath = document.DocumentPath,
                    Note = document.Note,
                    CreatedDate = formattedCreatedDate
                };
                return_data.Add(obj);
            }

            lstvDocuments.ItemsSource = return_data;
        }

        private void btnViewBatch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Controls.Button? clickedButton = sender as System.Windows.Controls.Button;
                if (clickedButton == null)
                    return;

                var dataContext = clickedButton.DataContext;
                lstvBatches.SelectedItem = dataContext;

                BatchModel selectedBatch = ValueConverter.ConvertToObject<BatchModel>(dataContext);

                _batchService.SetBatch(selectedBatch);

                BatchDetailWindow batchDetailWindow = new BatchDetailWindow(_batchService, false);
                batchDetailWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                NotificationShow("error", $"{ex.Message}");
                return;
            }
        }

        private void lstvDocuments_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (lstvDocuments.SelectedItem == null || _batchService.SelectedBatch == null)
                {
                    return;
                }

                DocumentModel selectedDocument = ValueConverter.ConvertToObject<DocumentModel>(lstvDocuments.SelectedItem);

                selectedDocument.BatchId = _batchService.SelectedBatch.Id;
                txtCurrentDocument.Text = selectedDocument.DocumentName;
            }
            catch (Exception ex)
            {
                NotificationShow("error", $"{ex.Message}");
                return;
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

        private void btnEditDocument_Click(object sender, RoutedEventArgs e)
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

                DocumentDetailWindow documentDetailWindow = new DocumentDetailWindow(_documentService, true, _batchService.SelectedBatch);
                documentDetailWindow.ShowDialog();

                txtCurrentDocument.Text = string.Empty;
                LoadTreeView();
            }
            catch (Exception ex)
            {
                NotificationShow("error", $"Sửa thất bại! {ex.Message}");
                return;
            }
        }

        private async void btnDeleteDocument_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Controls.Button? clickedButton = sender as System.Windows.Controls.Button;
                if (clickedButton == null)
                    return;

                var dataContext = clickedButton.DataContext;
                lstvDocuments.SelectedItem = dataContext;

                DocumentModel selectedDocument = ValueConverter.ConvertToObject<DocumentModel>(dataContext);

                MessageBoxResult Result = System.Windows.MessageBox.Show($"Bạn muốn xóa tài liệu: {selectedDocument.DocumentName}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (Result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _documentService.SetDocument(new DocumentModel());

                        string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        string folderPath = selectedDocument.DocumentPath;
                        string path = System.IO.Path.Combine(userFolderPath, folderPath);

                        try
                        {
                            Directory.Delete(path, true);
                        }
                        catch (DirectoryNotFoundException) { }

                        var documentDelete = await _documentService.Delete(selectedDocument.Id);

                        if (documentDelete)
                        {
                            NotificationShow("success", $"Xóa thành công gói tài liệu {selectedDocument.DocumentName}");
                            if (_batchService.SelectedBatch != null)
                            {
                                GetDocumentsByBatch(_batchService.SelectedBatch.Id);
                            }
                            LoadTreeView();
                        }
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
            catch (Exception ex)
            {
                NotificationShow("error", $"Xóa thất bại! {ex.Message}");
                return;
            }
        }
    }
}
