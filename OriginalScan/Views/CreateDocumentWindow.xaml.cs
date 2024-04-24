using FontAwesome5;
using Notification.Wpf;
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
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private async void CreateDocument_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CheckDocumentCreateField() != "")
                {
                    var nullNoti = new NotificationContent
                    {
                        Title = "Thông báo!",
                        Message = CheckDocumentCreateField(),
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
                    _notificationManager.Show(nullNoti);
                    return;
                }

                BatchModel? currentBatch = _batchService.SelectedBatch;

                if(currentBatch == null)
                {
                    var batchNotFoundNoti = new NotificationContent
                    {
                        Title = "Thông báo!",
                        Message = $"Vui lòng chọn gói trước khi tạo tài liệu.",
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
                    _notificationManager.Show(batchNotFoundNoti);
                    return;
                }

                DateTime now = DateTime.Now;
                string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string systemPath = System.IO.Path.Combine(currentBatch.BatchPath, $"{txtDocumentName.Text}_{now.ToString("yyyyMMdd")}");
                string path = System.IO.Path.Combine(userFolderPath, systemPath);

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
                    System.Windows.Forms.MessageBox.Show($"Khởi tạo thư mục thất bại! Vui lòng cấp quyền cho hệ thống: {ex.Message}");
                    return;
                }

                DocumentCreateRequest request = new DocumentCreateRequest()
                { 
                    BatchId = currentBatch.Id,
                    DocumentName = txtDocumentName.Text,
                    DocumentPath = systemPath,
                    Note = txtNote.Text,
                    CreatedDate = now.ToString()
                };

                int documentId = await _documentService.Create(request);

                if (documentId == 0) 
                {
                    var errorNoti = new NotificationContent
                    {
                        Title = "Lỗi!",
                        Message = $"Tạo tài liệu mới thất bại!",
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
                    return;
                }

                BatchWindow? batchManagerWindow = System.Windows.Application.Current.Windows.OfType<BatchWindow>().FirstOrDefault();
                if (batchManagerWindow != null)
                    batchManagerWindow.GetDocumentsByBatch(currentBatch.Id);

                var successNoti = new NotificationContent
                {
                    Title = "Thành công!",
                    Message = $"Tạo tài liệu mới thành công với mã {documentId}.",
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
            }
            catch (Exception ex)
            {
                var errorNoti = new NotificationContent
                {
                    Title = "Lỗi!",
                    Message = $"Tạo tài liệu mới thất bại! Có lỗi: {ex.Message}",
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
