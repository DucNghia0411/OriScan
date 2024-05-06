using FontAwesome5;
using Notification.Wpf;
using Notification.Wpf.Constants;
using Notification.Wpf.Controls;
using ScanApp.Data.Entities;
using ScanApp.Model.Models;
using ScanApp.Model.Requests.Document;
using ScanApp.Service.Constracts;
using ScanApp.Service.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
    /// Interaction logic for CreateDocumentWindow.xaml
    /// </summary>
    public partial class CreateDocumentWindow : Window
    {
        private readonly IBatchService _batchService;
        private readonly IDocumentService _documentService;
        private readonly NotificationManager _notificationManager;

        public CreateDocumentWindow
        (
            ScanContext context, 
            IBatchService batchService
        )
        {
            _documentService = new DocumentService(context);
            _batchService = batchService;
            _notificationManager = new NotificationManager();
            InitializeComponent();
            this.ResizeMode = ResizeMode.NoResize;
            NotificationConstants.MessagePosition = NotificationPosition.TopRight;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
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

        private async void CreateDocument_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CheckDocumentCreateField() != "")
                {
                    NotificationShow("warning", CheckDocumentCreateField());
                    return;
                }

                BatchModel? currentBatch = _batchService.SelectedBatch;

                if(currentBatch == null)
                {
                    NotificationShow("warning", "Vui lòng chọn gói trước khi tạo tài liệu.");
                    return;
                }

                DateTime now = DateTime.Now;
                string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string systemPath = System.IO.Path.Combine(currentBatch.BatchPath, $"{txtDocumentName.Text}_{now.ToString("yyyyMMdd")}");
                string path = System.IO.Path.Combine(userFolderPath, systemPath);

                DocumentCreateRequest request = new DocumentCreateRequest()
                {
                    BatchId = currentBatch.Id,
                    DocumentName = txtDocumentName.Text,
                    DocumentPath = systemPath,
                    Note = txtNote.Text,
                    CreatedDate = now.ToString()
                };

                var checkExistedResult = await _documentService.CheckExisted(currentBatch.Id, txtDocumentName.Text);
                if (checkExistedResult)
                {
                    NotificationShow("warning", "Tên tài liệu bị trùng lặp!");
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

                int documentId = await _documentService.Create(request);

                if (documentId == 0) 
                {
                    NotificationShow("error", "Tạo tài liệu mới thất bại!");
                    return;
                }

                BatchWindow? batchManagerWindow = System.Windows.Application.Current.Windows.OfType<BatchWindow>().FirstOrDefault();
                if (batchManagerWindow != null) 
                    batchManagerWindow.GetDocumentsByBatch(currentBatch.Id);

                MainWindow? mainWindow = System.Windows.Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (mainWindow != null)
                    mainWindow.LoadDirectoryTree();

                NotificationShow("success", $"Tạo tài liệu mới thành công với mã {documentId}.");
                this.Close();
            }
            catch (Exception ex)
            {
                NotificationShow("error", $"Tạo thất bại! Có lỗi: {ex.Message}");
                return;
            }
        }

        private void btnCancelDocument_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private string CheckDocumentCreateField()
        {
            string notification = string.Empty;
            if (txtDocumentName.Text.Trim() == "")
                notification += "Tên tài liệu không được để trống! \n";
            return notification;
        }
    }
}
