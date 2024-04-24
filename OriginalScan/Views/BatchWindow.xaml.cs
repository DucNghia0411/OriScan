using FontAwesome5;
using Notification.Wpf;
using ScanApp.Common.Common;
using ScanApp.Common.Settings;
using ScanApp.Data.Entities;
using ScanApp.Model.Requests.Batch;
using ScanApp.Service.Constracts;
using ScanApp.Service.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for BatchWindow.xaml
    /// </summary>
    public partial class BatchWindow : Window
    {
        private readonly IBatchService _batchService;
        private readonly NotificationManager _notificationManager;

        public BatchWindow(ScanContext context)
        {
            _batchService = new BatchService(context);
            _notificationManager = new NotificationManager();
            InitializeComponent();
            GetBatches();
        }

        private async void CreateBatch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CheckBatchCreateField() != "")
                {
                    System.Windows.Forms.MessageBox.Show(CheckBatchCreateField(), "Thông báo!", MessageBoxButtons.OK);
                    return; 
                }

                DateTime now = DateTime.Now;
                string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string systemPath = System.IO.Path.Combine(FolderSetting.AppFolder, FolderSetting.TempData, $"{txtBatchName.Text}_{now.ToString("yyyyMMddHHmmss")}");
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

                Directory.CreateDirectory(path);

                Batch? existBatch = await _batchService.FirstOrDefault(x => x.BatchName.Trim().ToUpper() == txtBatchName.Text.Trim().ToUpper());

                if(existBatch != null)
                {
                    var duplicateNoti = new NotificationContent
                    {
                        Title = "Thông báo!",
                        Message = $"Tên gói bị trùng lặp.",
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
                    _notificationManager.Show(duplicateNoti);
                    return;
                }

                BatchCreateRequest request = new BatchCreateRequest()
                {
                    BatchName = txtBatchName.Text,
                    Note = txtBatchNote.Text,
                    BatchPath = systemPath,
                    CreatedDate = now.ToString()
                };

                int batchId = await _batchService.Create(request);

                var successNoti = new NotificationContent
                {
                    Title = "Thành công!",
                    Message = $"Tạo gói mới thành công với mã {batchId}.",
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

                GetBatches();
            }
            catch (Exception ex)
            {
                var errorNoti = new NotificationContent
                {
                    Title = "Lỗi!",
                    Message = $"Tạo gói mới thất bại! Có lỗi: {ex.Message}",
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

        private async void GetBatches()
        {
            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            userFolderPath = userFolderPath.Replace("/", "\\");

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

        private void btnCreateDocument_Click(object sender, RoutedEventArgs e)
        {
            CreateDocumentWindow createDocumentWindow = new CreateDocumentWindow();
            createDocumentWindow.ShowDialog();
        }

        private void lstvBatches_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstvBatches.SelectedItem == null)
            {
                return;
            }

            BatchDetailWindow batchDetailWindow = new BatchDetailWindow();
            batchDetailWindow.ShowDialog();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Edited!");
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Deleted!");
        }
    }
}
