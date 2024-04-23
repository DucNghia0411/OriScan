using ScanApp.Common.Settings;
using ScanApp.Data.Entities;
using ScanApp.Model.Requests.Batch;
using ScanApp.Service.Constracts;
using ScanApp.Service.Services;
using System;
using System.Collections.Generic;
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

        public BatchWindow(ScanContext context)
        {
            _batchService = new BatchService(context);
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

                BatchCreateRequest request = new BatchCreateRequest()
                {
                    BatchName = txtBatchName.Text,
                    Note = txtBatchNote.Text,
                    BatchPath = systemPath,
                    CreatedDate = now.ToString()
                };

                int batchId = await _batchService.Create(request);
                System.Windows.Forms.MessageBox.Show($"Tạo gói mới thành công với mã {batchId}!", "Thông báo!");
                GetBatches();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Tạo gói mới thất bại! Có lỗi: {ex.Message}", "Thông báo!");
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
                var obj = new
                {
                    Id = batch.Id,
                    BatchName = batch.BatchName,
                    BatchPath = $"{userFolderPath}\\{batch.BatchPath}",
                    Note = batch.Note,
                    CreatedDate = batch.CreatedDate
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
    }
}
