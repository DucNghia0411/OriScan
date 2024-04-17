using Microsoft.AspNetCore.Http;
using NTwain;
using NTwain.Data;
using OriginalScan.Views;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using ScanApp.Common.Common;
using ScanApp.Common.Settings;
using ScanApp.Data.Entities;
using ScanApp.Intergration.ApiClients;
using ScanApp.Intergration.Constracts;
using System;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Printing;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using MessageBox = System.Windows.Forms.MessageBox;

namespace OriginalScan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DataSource? dataSource;
        public TwainSession? _twainSession;
        public List<Bitmap> scannedImages;
        public ObservableCollection<BitmapImage> listImages = new ObservableCollection<BitmapImage>();
        private DateTime _scanTime;
        private readonly ITransferApiClient _transferApiClient;
        private readonly ScanContext _context;
        public MainWindow
        (
            ScanContext context
        )
        {
            InitializeComponent();
            _context = context;
            scannedImages = new List<Bitmap>();
            DataContext = this;
            CreateSession();
            _transferApiClient = new TransferApiClient();
        }

        private void CreateSession()
        {
            try
            {
                var appId = TWIdentity.CreateFromAssembly(DataGroups.Image, Assembly.GetExecutingAssembly());
                var session = new TwainSession(appId);
                _twainSession = session;
                _twainSession.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi: {ex.Message}", "Thông báo!", MessageBoxButtons.OK);
                return;
            }
        }

        public void ScanButton_Click(object sender, EventArgs e)
        {
            try
            {
                if(_twainSession == null)
                {
                    MessageBox.Show("Vui lòng kiểm tra lại thiết bị trước khi thực hiện Scan!", "Thông báo!", MessageBoxButtons.OK);
                    return;
                }

                if (dataSource == null)
                {
                    MessageBox.Show("Vui lòng kiểm tra lại thiết bị trước khi thực hiện Scan!", "Thông báo!", MessageBoxButtons.OK);
                    return;
                }

                _scanTime = DateTime.Now;

                _twainSession.DataTransferred -= DataTransferred;
                _twainSession.DataTransferred += DataTransferred;

                if(!_twainSession.IsSourceOpen)
                    dataSource.Open();

                if (!_twainSession.IsSourceOpen)
                {
                    MessageBox.Show("Vui lòng kiểm tra lại thiết bị trước khi thực hiện Scan!", "Thông báo!", MessageBoxButtons.OK);
                    return;
                }

                dataSource.Capabilities.CapDuplexEnabled.SetValue(BoolType.True);
                dataSource.Enable(SourceEnableMode.NoUI, false, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi: {ex.Message}", "Thông báo!", MessageBoxButtons.OK);
                return;
            }
        }

        private void DataTransferred(object s, DataTransferredEventArgs e)
        {
            try
            {
                DateTime now = _scanTime;
                string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string systemPath = System.IO.Path.Combine(FolderSetting.AppFolder, FolderSetting.Images);
                string path = System.IO.Path.Combine(userFolderPath, systemPath, now.ToString("yyyyMMddHHmmss"));
                Directory.CreateDirectory(path);

                if (e.NativeData != IntPtr.Zero)
                {
                    var stream = e.GetNativeImageStream();
                    if (stream != null)
                    {
                        scannedImages.Clear();
                        scannedImages.Add(new Bitmap(stream));
                    }

                    foreach (var img in scannedImages)
                    {
                        BitmapImage bitmapImage = new BitmapImage();

                        using (MemoryStream memory = new MemoryStream())
                        {
                            img.Save(memory, ImageFormat.Png);
                            memory.Position = 0;
                            bitmapImage.BeginInit();
                            bitmapImage.StreamSource = memory;
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.EndInit();
                            bitmapImage.Freeze();
                        }

                        Guid guid = Guid.NewGuid();
                        string imagesName = now.ToString("yyyyMMddHHmmss") + guid.ToString("N") + ".png";
                        string imagePath = System.IO.Path.Combine(path, imagesName);
                        img.Save(imagePath);

                        App.Current.Dispatcher.Invoke((Action)delegate
                        {
                            listImages.Add(bitmapImage);
                        });
                    }

                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        ListMainImages.ItemsSource = listImages;
                        ListExtraImages.ItemsSource = listImages;
                    });
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void ConvertToPdfButton_Click(object sender, EventArgs e)
        {
            try
            {
                string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string systemPath = System.IO.Path.Combine(FolderSetting.AppFolder, FolderSetting.Images);
                string pdfPath = System.IO.Path.Combine(FolderSetting.AppFolder, FolderSetting.PDFs);
                string defaultPath = System.IO.Path.Combine(userFolderPath, systemPath);

                if (!Directory.Exists(defaultPath))
                    defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                using (var dialog = new FolderBrowserDialog())
                {
                    dialog.SelectedPath = defaultPath;

                    DialogResult result = dialog.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                    {
                        string selectedFolder = dialog.SelectedPath;
                        string[] images = Directory.GetFiles(selectedFolder);
                        string folderName = System.IO.Path.GetFileName(selectedFolder);

                        if (images.Count() == 0)
                        {
                            MessageBox.Show($"Không có hình ảnh trong thư mục!", "Thông báo!", MessageBoxButtons.OK);
                            return;
                        }

                        string pdfFileName = folderName + ".pdf";
                        string folderPath = System.IO.Path.Combine(userFolderPath, pdfPath);
                        Directory.CreateDirectory(folderPath);
                        string pdfFilePath = System.IO.Path.Combine(userFolderPath, pdfPath, pdfFileName);

                        if (File.Exists(pdfFilePath))
                        {
                            MessageBoxResult pdfConfirm = System.Windows.MessageBox.Show("Đã tồn tại một tệp PDF có cùng tên. Bạn có muốn thay thế nó?", "Thông báo!", MessageBoxButton.YesNo, MessageBoxImage.Question);

                            if (pdfConfirm == MessageBoxResult.Yes)
                                File.Delete(pdfFileName);
                            else
                                return;
                        }

                        PdfDocument pdfDocument = new PdfDocument();

                        foreach (string imagePath in images)
                        {
                            PdfPage page = pdfDocument.AddPage();

                            using (var image = XImage.FromFile(imagePath))
                            {
                                XGraphics gfx = XGraphics.FromPdfPage(page);
                                gfx.DrawImage(image, 0, 0, page.Width, page.Height);
                            }
                        }

                        pdfDocument.Save(pdfFilePath);
                        MessageBox.Show($"Lưu thành công tại đường dẫn: {pdfFilePath}", "Thông báo!", MessageBoxButtons.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi: {ex.Message}", "Thông báo!", MessageBoxButtons.OK);
                return;
            }
        }

        public async void TransferToPortal_Click(object sender, EventArgs e)
        {
            try
            {
                string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string systemPath = System.IO.Path.Combine(FolderSetting.AppFolder, FolderSetting.Images);
                string pdfPath = System.IO.Path.Combine(FolderSetting.AppFolder, FolderSetting.PDFs);
                string defaultPath = System.IO.Path.Combine(userFolderPath, pdfPath);

                if (!Directory.Exists(defaultPath))
                    defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = defaultPath;
                openFileDialog.Filter = "PDF files (*.pdf)|*.pdf";

                DialogResult result = openFileDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
                {
                    string selectedFolder = openFileDialog.FileName;
                    bool transferResult =  await _transferApiClient.TransferToPortal(selectedFolder);

                    if(transferResult)
                        MessageBox.Show($"Upload công văn thành công!", "Thông báo!", MessageBoxButtons.OK);
                    else
                    {
                        MessageBox.Show($"Upload công văn thất bại!", "Thông báo!", MessageBoxButtons.OK);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi: {ex.Message}", "Thông báo!", MessageBoxButtons.OK);
                return;
            }
        }

        private void ChooseDevice_Click(object sender, RoutedEventArgs e)
        {
            DeviceWindow deviceWindow = new DeviceWindow();
            deviceWindow.twainSession = _twainSession;
            deviceWindow.mainWindow = this;
            deviceWindow.GetListDevice();
            deviceWindow.ShowDialog();
        }

        private void OpenBatch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BatchWindow batchWindow = new BatchWindow(_context);
                batchWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi: {ex.Message}", "Thông báo!", MessageBoxButtons.OK);
                return;
            }
        }
    }
}