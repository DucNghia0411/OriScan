using FontAwesome5;
using Microsoft.EntityFrameworkCore;
using Notification.Wpf;
using Notification.Wpf.Classes;
using NTwain.Data;
using ScanApp.Common.Common;
using ScanApp.Common.Settings;
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
        private readonly ScanContext _context;
        private readonly NotificationManager _notificationManager;

        public BatchWindow(ScanContext context)
        {
            _context = context;
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
                    NotificationShow("error", CheckBatchCreateField());
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
                    NotificationShow("error", $"Khởi tạo thư mục thất bại! Vui lòng cấp quyền cho hệ thống: {ex.Message}");
                    return;
                }

                Directory.CreateDirectory(path);

                Batch? existBatch = await _batchService.FirstOrDefault(x => x.BatchName.Trim().ToUpper() == txtBatchName.Text.Trim().ToUpper());

                if (existBatch != null)
                {
                    NotificationShow("warning", "Tên gói bị trùng lặp!");
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

                NotificationShow("success", $"Tạo gói mới thành công với mã {batchId}.");

                GetBatches();
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

        private void btnCreateDocument_Click(object sender, RoutedEventArgs e)
        {
            CreateDocumentWindow createDocumentWindow = new CreateDocumentWindow();
            createDocumentWindow.ShowDialog();
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
                _batchService.SetBatch(selectedBatch);

                /*BatchDetailWindow batchDetailWindow = new BatchDetailWindow();
                batchDetailWindow.ShowDialog();*/
            }
            catch (Exception ex)
            {
                NotificationShow("error", $"Có lỗi: {ex.Message}");
                return;
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Controls.Button? clickedButton = sender as System.Windows.Controls.Button;
                if (clickedButton == null)
                    return;

                var dataContext = clickedButton.DataContext;
                BatchModel selectedBatch = ValueConverter.ConvertToObject<BatchModel>(dataContext);

                _batchService.SetBatch(selectedBatch);

                BatchDetailWindow batchDetailWindow = new BatchDetailWindow(_batchService, true);
                batchDetailWindow.ShowDialog();

                GetBatches();
                txtCurrentBatch.Text = string.Empty;
            }
            catch (Exception ex)
            {
                NotificationShow("error", $"Sửa thất bại! Có lỗi: {ex.Message}");
                return;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Controls.Button? clickedButton = sender as System.Windows.Controls.Button;
                if (clickedButton == null)
                    return;

                var dataContext = clickedButton.DataContext;
                BatchModel selectedBatch = ValueConverter.ConvertToObject<BatchModel>(dataContext);

                _notificationManager.ShowButtonWindow($"Bạn muốn xóa gói: {selectedBatch.BatchName}?", "Xác nhận", 
                    async () => {
                        string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        string folderPath = selectedBatch.BatchPath;
                        string path = System.IO.Path.Combine(userFolderPath, folderPath);

                        Directory.Delete(path, true);

                        var deleteResult = await _batchService.Delete(selectedBatch.Id);

                        if (deleteResult)
                        {
                            NotificationShow("success", $"Xóa thành công gói tài liệu {selectedBatch.BatchName}");
                            GetBatches();
                            txtCurrentBatch.Text = string.Empty;
                            txtCurrentDocument.Text = string.Empty;
                        }
                    }, "OK", () => { }, "Cancel");

            }
            catch (Exception ex)
            {
                NotificationShow("error", $"Xóa thất bại! Có lỗi: {ex.Message}");
                return;
            }
        }

<<<<<<< Updated upstream
=======
        public async void GetDocumentsByBatch(int batchId)
        {
            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            userFolderPath = userFolderPath.Replace("/", "\\");

            IEnumerable<ScanApp.Data.Entities.Document> documents = await _documentService.Get(x => x.BatchId == batchId);
        }

>>>>>>> Stashed changes
        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Controls.Button? clickedButton = sender as System.Windows.Controls.Button;
                if (clickedButton == null)
                    return;

                var dataContext = clickedButton.DataContext;
                BatchModel selectedBatch = ValueConverter.ConvertToObject<BatchModel>(dataContext);

                _batchService.SetBatch(selectedBatch);

                BatchDetailWindow batchDetailWindow = new BatchDetailWindow(_batchService, false);
                batchDetailWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                NotificationShow("error", $"Có lỗi: {ex.Message}");
                return;
            }
        }
    }
}
