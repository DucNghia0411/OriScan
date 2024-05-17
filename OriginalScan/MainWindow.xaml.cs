using FontAwesome5;
using Microsoft.AspNetCore.Http;
using Notification.Wpf;
using Notification.Wpf.Constants;
using Notification.Wpf.Controls;
using NTwain;
using NTwain.Data;
using OriginalScan.Models;
using OriginalScan.Views;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using ScanApp.Common.Common;
using ScanApp.Common.Settings;
using ScanApp.Data.Entities;
using ScanApp.Intergration.ApiClients;
using ScanApp.Intergration.Constracts;
using ScanApp.Model.Models;
using ScanApp.Model.Requests.Document;
using ScanApp.Service.Constracts;
using ScanApp.Service.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Printing;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OriginalScan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public DataSource? dataSource;
        public TwainSession? _twainSession;
        public List<Bitmap> scannedImages;
        public ObservableCollection<BitmapImage> listImages = new ObservableCollection<BitmapImage>();
        private DateTime _scanTime;
        private readonly ITransferApiClient _transferApiClient;
        private readonly ScanContext _context;
        private readonly NotificationManager _notificationManager;

        private readonly IBatchService _batchService;
        private readonly IDocumentService _documentService;
        private readonly IImageService _imageService;

        public MainWindow (ScanContext context)
        {
            InitializeComponent();
            _context = context;
            scannedImages = new List<Bitmap>();
            DataContext = this;
            CreateSession();
            _transferApiClient = new TransferApiClient();
            _notificationManager = new NotificationManager();
            _documentService = new DocumentService(context);
            _batchService = new BatchService(context);
            _imageService = new ImageService(context);
            _listImagesMain = new ObservableCollection<ScannedImage>();
            _listImagesSelected = new ObservableCollection<ScannedImage>();
            NotificationConstants.MessagePosition = NotificationPosition.TopRight;
        }

        public string? RootPath { get; set; }

        public string? BatchPath { get; set; }

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
                NotificationShow("error", ex.Message);
                return;
            }
        }

        public void ScanButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_twainSession == null)
                {
                    NotificationShow("warning", "Vui lòng kiểm tra lại thiết bị trước khi thực hiện quét!");
                    return;
                }

                if (dataSource == null)
                {
                    NotificationShow("warning", "Vui lòng kiểm tra lại thiết bị trước khi thực hiện quét!");
                    return;
                }

                _scanTime = DateTime.Now;

                _twainSession.DataTransferred -= DataTransferred;
                _twainSession.DataTransferred += DataTransferred;

                if (!_twainSession.IsSourceOpen)
                    dataSource.Open();

                if (!_twainSession.IsSourceOpen)
                {
                    NotificationShow("warning", "Vui lòng kiểm tra lại thiết bị trước khi thực hiện quét!");
                    return;
                }
                
                SetupDevice();
                dataSource.Enable(SourceEnableMode.NoUI, false, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                NotificationShow("error", ex.Message);
                return;
            }
        }

        private void DataTransferred(object s, DataTransferredEventArgs e)
        {
            try
            {
                BatchModel? currentBatch = _batchService.SelectedBatch;

                if (currentBatch == null)
                {
                    NotificationShow("warning", "Vui lòng chọn gói trước khi thực hiện quét.");
                    return;
                }

                DocumentModel? currentDocument = _documentService.SelectedDocument;

                if (currentDocument == null)
                {
                    NotificationShow("warning", "Vui lòng chọn tài liệu trước khi thực hiện quét.");
                    return;
                }

                DateTime now = _scanTime;
                string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string path = Path.Combine(userFolderPath, currentDocument.DocumentPath);
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
                        string imagePath = Path.Combine(path, imagesName);
                        img.Save(imagePath);

                        int totalImages = ListImagesMain.Count;

                        ScannedImage imageViewModel = new ScannedImage()
                        { 
                            DocumentId = currentDocument.Id,
                            ImageName = imagesName,
                            ImagePath = imagePath,
                            IsSelected = false,
                            bitmapImage = bitmapImage,
                            Order = totalImages++,
                        };

                        App.Current.Dispatcher.Invoke((Action)delegate
                        {
                           ListImagesMain.Add(imageViewModel);
                        });                        
                    }

                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        ReloadTreeViewItem();
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
                            NotificationShow("error", $"Không có hình ảnh trong thư mục!");
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
                            PdfPage page = pdfDocument.AddPage();

                            using (var image = XImage.FromFile(imagePath))
                            {
                                XGraphics gfx = XGraphics.FromPdfPage(page);
                                gfx.DrawImage(image, 0, 0, page.Width, page.Height);
                            }
                        }

                        pdfDocument.Save(pdfFilePath);
                        NotificationShow("success", $"Lưu thành công tại đường dẫn: {pdfFilePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationShow("error", ex.Message);
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
                        NotificationShow("success", $"Upload công văn thành công!");
                    else
                    {
                        NotificationShow("error", "Upload công văn thất bại!");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationShow("error", ex.Message);
                return;
            }
        }

        private void ChooseDevice_Click(object sender, RoutedEventArgs e)
        {
            DeviceWindow deviceWindow = new DeviceWindow();
            deviceWindow.twainSession = _twainSession;
            deviceWindow.mainWindow = this;
            if (dataSource != null)
            {
                deviceWindow.dataSource = dataSource;
            }
            deviceWindow.GetListDevice();
            deviceWindow.ShowDialog();
        }

        public void ClearSetting()
        {
            if (dataSource == null || _twainSession == null)
            {
                return;
            }

            _twainSession.CurrentSource.Capabilities.ResetAll();
            DeviceSettingConverter.SetIsDefault(true);
            DeviceSettingConverter.SetDuplex(false);
            DeviceSettingConverter.SetSize(null);
            DeviceSettingConverter.SetDpi(null);
            DeviceSettingConverter.SetPixeType(null);
            DeviceSettingConverter.SetBitDepth(null);
            DeviceSettingConverter.SetRotateDegree(null);
            DeviceSettingConverter.SetBrightness(null);
            DeviceSettingConverter.SetContrast(null);
        }

        public void SetupDevice()
        {
            if (dataSource == null || _twainSession == null)
            {
                return;
            }

            if (!DeviceSettingConverter._isDefault)
            {
                BoolType isDuplex;

                if (DeviceSettingConverter._duplex == true)
                    isDuplex = BoolType.True;
                else
                    isDuplex = BoolType.False;

                _twainSession.CurrentSource.Capabilities.CapDuplexEnabled.SetValue(isDuplex);

                if (DeviceSettingConverter._size != null)
                {
                    SupportedSize size = (SupportedSize)DeviceSettingConverter._size;
                    _twainSession.CurrentSource.Capabilities.ICapSupportedSizes.SetValue(size);
                }

                if (DeviceSettingConverter._dpi != null)
                {
                    TWFix32 dpi = (TWFix32)DeviceSettingConverter._dpi;
                    _twainSession.CurrentSource.Capabilities.ICapXResolution.SetValue(dpi);
                    _twainSession.CurrentSource.Capabilities.ICapYResolution.SetValue(dpi);
                }

                if (DeviceSettingConverter._pixelType != null)
                {
                    PixelType pixelType = (PixelType)DeviceSettingConverter._pixelType;
                    _twainSession.CurrentSource.Capabilities.ICapPixelType.SetValue(pixelType);
                }

                if (DeviceSettingConverter._bitDepth != null)
                {
                    int bitDepth = (int)DeviceSettingConverter._bitDepth;
                    _twainSession.CurrentSource.Capabilities.ICapBitDepth.SetValue(bitDepth);
                }

                if (DeviceSettingConverter._rotateDegree != null)
                {
                    TWFix32 rotateDegree = (TWFix32)DeviceSettingConverter._rotateDegree;
                    _twainSession.CurrentSource.Capabilities.ICapRotation.SetValue(rotateDegree);
                }

                if (DeviceSettingConverter._brightness != null)
                {
                    TWFix32 brightness = (TWFix32)DeviceSettingConverter._brightness;
                    _twainSession.CurrentSource.Capabilities.ICapBrightness.SetValue(brightness);
                }

                if (DeviceSettingConverter._contrast != null)
                {
                    TWFix32 contrast = (TWFix32)DeviceSettingConverter._contrast;
                    _twainSession.CurrentSource.Capabilities.ICapContrast.SetValue(contrast);
                }
            }
        }

        private void OpenBatch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BatchWindow batchWindow = new BatchWindow
                (
                    _context, 
                    _batchService, 
                    _documentService,
                    _imageService
                );
                batchWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                NotificationShow("error", ex.Message);
                return;
            }
        }

        public async void LoadDirectory(TreeViewItem parentItem, string folderPath)
        {
            if (_batchService.SelectedBatch == null)
            {
                return;
            }

            try
            {
                var parentInfo = new DirectoryInfo(folderPath);
                string parentName = parentInfo.Name;
                var directories = Directory.GetDirectories(folderPath).Select(d => new DirectoryInfo(d)).OrderBy(d => d.CreationTime).Select(d => d.FullName).ToArray();

                for (int i = 0; i < directories.Count(); i++)
                {
                    var directoryInfo = new DirectoryInfo(directories[i]);
                    string directoryName = directoryInfo.Name;

                    var listDocument = await _documentService.Get(x => x.BatchId == _batchService.SelectedBatch.Id);
                    string path = System.IO.Path.Combine(_batchService.SelectedBatch.BatchPath, directoryName);

                    if (!IsItemAlreadyExists(parentItem, directoryName) && CheckExistedInDatabase(listDocument, path))
                    {
                        var directoryItem = CreateTreeViewItem(directoryName, "document", $"Tài liệu {i + 1}");
                        parentItem.Items.Add(directoryItem);
                        LoadDirectory(directoryItem, directories[i]);
                    }
                }

                var files = Directory.GetFiles(folderPath).Select(d => new DirectoryInfo(d)).OrderBy(d => d.CreationTime).Select(d => d.FullName).ToArray();

                for (int i = 0; i < files.Count(); i++)
                {
                    var fileInfo = new FileInfo(files[i]);
                    string fileName = fileInfo.Name;

                    if (!IsItemAlreadyExists(parentItem, fileName))
                    {
                        var fileItem = CreateTreeViewItem(fileName, "image", $"Ảnh {i + 1}");
                        parentItem.Items.Add(fileItem);
                    }
                }

                ExpandTreeViewItem(folderPath);
            }
            catch (Exception ex)
            {
                NotificationShow("error", ex.Message);
            }
        }

        public void ReloadTreeViewItem()
        {
            trvBatchExplorer.Items.Clear();
            
            if (RootPath == null)
            {
                return;
            }

            if (!Directory.Exists(RootPath))
            {
                trvBatchExplorer.Items.Clear();
                return;
            }

            string name = System.IO.Path.GetFileName(RootPath);
            var directoryItem = CreateTreeViewItem(name, "folder", name);

            trvBatchExplorer.Items.Add(directoryItem);
            LoadDirectory(directoryItem, RootPath);
        }

        public TreeViewItem CreateTreeViewItem(string directoryName, string icon, string itemName)
        {
            string iconSource = "";

            switch (icon)
            {
                case "folder":
                    {
                        iconSource = "/Resource/Images/foldericon.png";
                        break;
                    }
                case "document":
                    {
                        iconSource = "/Resource/Images/documents.png";
                        break;
                    }
                case "image":
                    {
                        iconSource = "/Resource/Icons/crop.png";
                        break;
                    }
            }

            var item = new TreeViewItem()
            {
                Header = new StackPanel()
                {
                    Orientation = System.Windows.Controls.Orientation.Horizontal,
                    Children =
                    {
                        new System.Windows.Controls.Image()
                        {
                            Source = new BitmapImage(new Uri(iconSource, UriKind.Relative)),
                            Width = 16,
                            Height = 16,
                            Margin = new Thickness(0, 10, 5, 0)
                        },
                        new TextBlock()
                        {
                            Text = itemName,
                            Margin = new Thickness(0, 10, 0, 0)
                        }
                    }
                },
                Tag = directoryName
            };

            return item;
        }

        public bool CheckExistedInDatabase(IEnumerable<ScanApp.Data.Entities.Document> listDocument, string path)
        {
            foreach (ScanApp.Data.Entities.Document document in listDocument)
            {
                if (document.DocumentPath == path)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsItemAlreadyExists(TreeViewItem parentItem, string itemName)
        {
            if (trvBatchExplorer.Items.Count == 0)
            {
                return false;
            }

            if (parentItem.Tag != null && parentItem.Tag.ToString() == itemName)
            {
                return true;
            }

            foreach (TreeViewItem item in parentItem.Items)
            {
                if (item.Tag != null && item.Tag.ToString() == itemName)
                {
                    return true;
                }
            }
            return false;
        }

        public void ExpandTreeViewItem(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                    return;

                ExpandTreeViewItem(trvBatchExplorer.Items, folderPath);
            }
            catch (Exception ex)
            {
                NotificationShow("error", ex.Message);
            }
        }

        public async void UpdateDocNumOfSheets(int numOfSheets)
        {
            try
            {
                if (_documentService.SelectedDocument == null) return;
                var document = await _documentService.FirstOrDefault(e => e.Id == _documentService.SelectedDocument.Id);

                if (document == null) return;

                DocumentUpdateRequest request = new DocumentUpdateRequest()
                {
                    Id = _documentService.SelectedDocument.Id,
                    DocumentName = document.DocumentName,
                    Note = document.Note,
                    AgencyIdentifier = document.AgencyIdentifier,
                    DocumentIdentifier = document.DocumentIdentifier,
                    NumberOfSheets = numOfSheets,
                    StartDate = document.StartDate,
                    EndDate = document.EndDate,
                    StoragePeriod = document.StoragePeriod
                };

                var updateResult = await _documentService.Update(request);

                if (updateResult == 0)
                {
                    NotificationShow("error", "Cập nhật số tờ không thành công!");
                    return;
                }
                else
                {
                    DocumentModel documentModel = new DocumentModel()
                    {
                        Id = request.Id,
                        BatchId = _documentService.SelectedDocument.BatchId,
                        DocumentName = _documentService.SelectedDocument.DocumentName,
                        DocumentPath = _documentService.SelectedDocument.DocumentPath,
                        NumberOfSheets = _documentService.SelectedDocument.NumberOfSheets
                    };

                    _documentService.SetDocument(documentModel);
                }
            }
            catch(Exception ex)
            {
                NotificationShow("error", ex.Message);
                return;
            }
        }

        private bool IsTreeViewItemExpanded(TreeViewItem treeViewItem)
        {
            if (treeViewItem == null)
                return false;

            return treeViewItem.IsExpanded;
        }

        private void ExpandTreeViewItem(ItemCollection items, string folderPath)
        {
            foreach (var item in items)
            {
                if (item is TreeViewItem treeViewItem)
                {
                    if ((treeViewItem.Tag.ToString() == Path.GetFileName(folderPath)) || Path.GetFileName(folderPath) == FolderSetting.TempData)
                    {
                        if (!IsTreeViewItemExpanded(treeViewItem))
                        {
                            treeViewItem.IsExpanded = true;
                            LoadDirectory(treeViewItem, folderPath);
                        }
                        return;
                    }

                    ExpandTreeViewItem(treeViewItem.Items, folderPath);
                }
            }
        }

        private async void btnSaveImages_Click(object sender, EventArgs e)
        {
            try
            {
                BatchModel? currentBatch = _batchService.SelectedBatch;

                if (currentBatch == null)
                {
                    NotificationShow("warning", "Vui lòng chọn gói trước khi thực hiện lưu hình ảnh.");
                    return;
                }

                DocumentModel? currentDocument = _documentService.SelectedDocument;

                if (currentDocument == null)
                {
                    NotificationShow("warning", "Vui lòng chọn tài liệu trước khi thực hiện lưu hình ảnh.");
                    return;
                }

                string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string path = Path.Combine(userFolderPath, currentDocument.DocumentPath);

                ObservableCollection<ScannedImage> listImages = ListImagesMain;
                int totalImages = listImages.Count;

                if (totalImages == 0)
                {
                    NotificationShow("warning", "Không tìm thấy hình ảnh để thực hiện.");
                    return;
                }

                ObservableCollection<ScannedImage> listImagesNeedToSave = new ObservableCollection<ScannedImage>(listImages.Where(x => x.Id == 0));
                if(listImagesNeedToSave.Count() == 0)
                {
                    NotificationShow("warning", "Không tìm thấy hình ảnh để thực hiện.");
                    return;
                }

                totalImages = listImagesNeedToSave.Count();

                MessageBoxResult Result = System.Windows.MessageBox.Show($"Bạn muốn lưu {totalImages} ảnh vào tài liệu {currentDocument.DocumentName} của gói {currentBatch.BatchName}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (Result == MessageBoxResult.Yes)
                {
                    List<ScanApp.Data.Entities.Image> listSavedImage = new List<ScanApp.Data.Entities.Image>();

                    foreach (ScannedImage scannedImage in listImagesNeedToSave)
                    {
                        DateTime now = DateTime.Now;
                        string imagePath = Path.Combine(currentDocument.DocumentPath, scannedImage.ImageName);

                        ScanApp.Data.Entities.Image image = new ScanApp.Data.Entities.Image()
                        {
                            DocumentId = currentDocument.Id,
                            ImageName = scannedImage.ImageName,
                            ImagePath = imagePath,
                            CreatedDate = now.ToString(),
                            Order = scannedImage.Order
                        };

                        listSavedImage.Add(image);
                    }

                    await _imageService.AddRange(listSavedImage);
                    await _imageService.Save();

                    ListImagesMain.Clear();
                    ReloadTreeViewItem();

                    var count = await _imageService.CountByDocument(currentDocument.Id);
                    UpdateDocNumOfSheets(count);
                    NotificationShow("success", $"Lưu thành công {listSavedImage.Count} ảnh vào tài liệu {currentDocument.DocumentName}. Vui lòng mở lại để thực hiện các thao tác khác.");
                }
                else
                    return;
            }
            catch (Exception ex)
            {
                NotificationShow("error", ex.Message);
                return;
            }
        }

        private void ListImagesMain_Click(object sender, RoutedEventArgs e)
        {
            var clickedItem = (sender as System.Windows.Controls.ListView);
            if (clickedItem == null) return;

            ScannedImage? selectedImage = clickedItem.SelectedItem as ScannedImage;
            if (selectedImage == null) return;

            if(!selectedImage.IsSelected)
            {
                selectedImage.IsSelected = true;
                ListImagesSelected.Add(selectedImage);
            }
            else
            {
                selectedImage.IsSelected = false;
                ListImagesSelected.Remove(selectedImage);
            }
            clickedItem.SelectedItems.Clear();
        }

        private async void btnDeleteImages_Click(object sender, EventArgs e)
        {
            try
            {
                int totalDeleted = ListImagesSelected.Count;

                if (totalDeleted == 0)
                {
                    NotificationShow("warning", $"Vui lòng chọn hình ảnh để xóa.");
                    return;
                }

                List<string> isSavedImages = ListImagesSelected.Where(x => x.Id != 0).Select(x => x.ImageName).ToList();
                if(isSavedImages.Count() > 0)
                {
                    MessageBoxResult checkSavedResult = System.Windows.MessageBox.Show($"Bạn có {isSavedImages.Count} ảnh đã được lưu trữ theo tài liệu ({string.Join(", ", isSavedImages)}). Bạn có chắc chắn muốn tiếp tục?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (checkSavedResult != MessageBoxResult.Yes)
                        return;
                }

                MessageBoxResult confirmCheckResult = System.Windows.MessageBox.Show($"Bạn có chắc chắn muốn xóa {totalDeleted} ảnh?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirmCheckResult != MessageBoxResult.Yes)
                    return;

                List<int> listIdsDelete = new List<int>();
                foreach (var image in ListImagesSelected) 
                {
                    if (image.Id != 0)
                        listIdsDelete.Add(image.Id);

                    File.Delete(image.ImagePath);
                    ReloadTreeViewItem();
                }

                await _imageService.DeleteMultiById(listIdsDelete);

                foreach (var image in ListImagesSelected)
                {
                    ListImagesMain.Remove(image);
                }

                ListImagesSelected.Clear();

                if (_documentService.SelectedDocument != null)
                {
                    var count = await _imageService.CountByDocument(_documentService.SelectedDocument.Id);
                    UpdateDocNumOfSheets(count);
                }
                
                NotificationShow("success", $"Xóa thành công {totalDeleted} ảnh.");            
            }
            catch (Exception ex)
            {
                NotificationShow("error", $"Có lỗi: {ex.Message}");
                return;
            }
        }

        private async void trvBatchExplorer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (trvBatchExplorer.SelectedItem == null)
                {
                    return;
                }

                if (trvBatchExplorer.SelectedItem is TreeViewItem selectedItem)
                {
                    if (BatchPath != null && selectedItem.Tag != null)
                    {
                        string filePath = System.IO.Path.Combine(BatchPath, selectedItem.Tag.ToString()!);
                        string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        string path = System.IO.Path.Combine(userFolderPath, filePath);

                        if (Directory.Exists(path))
                        {
                            var selectedDocument = await _documentService.FirstOrDefault(e => e.DocumentPath == filePath);
                            
                            if (selectedDocument != null)
                            {
                                DocumentModel docModel = new DocumentModel()
                                {
                                    Id = selectedDocument.Id,
                                    BatchId = selectedDocument.BatchId,
                                    DocumentName = selectedDocument.DocumentName,
                                    DocumentPath = selectedDocument.DocumentPath,
                                    NumberOfSheets = selectedDocument.NumberOfSheets
                                };

                                _documentService.SetDocument(docModel);
                                GetImagesByDocument(selectedDocument.Id);
                            }
                        }

                        else
                        {
                            foreach (var img in ListImagesMain)
                            {
                                if (_documentService.SelectedDocument == null)
                                {
                                    return;
                                }
                                string imagePath = System.IO.Path.Combine(userFolderPath, _documentService.SelectedDocument.DocumentPath, selectedItem.Tag.ToString()!);

                                if (img.ImagePath == imagePath)
                                {
                                    if (!img.IsSelected)
                                    {
                                        img.IsSelected = true;
                                        ListImagesSelected.Add(img);
                                    }
                                    else
                                    {
                                        img.IsSelected = false;
                                        ListImagesSelected.Remove(img);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationShow("error", ex.Message);
            }
        }

        public async void GetImagesByDocument(int documentId)
        {
            ScanApp.Data.Entities.Document? document = await _documentService.FirstOrDefault(e => e.Id == documentId);

            if (document == null)
            {
                NotificationShow("warning", $"Không tìm thấy tài liệu với mã {documentId}.");
                return;
            }

            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string documentPath = System.IO.Path.Combine(userFolderPath, document.DocumentPath);

            if (!Directory.Exists(documentPath))
            {
                NotificationShow("warning", $"Không tìm thấy thư mục tài liệu theo đường dẫn {documentPath}.");
                return;
            }

            string[] filesInDocumentPath = Directory.GetFiles(documentPath);
            int totalImagesInPath = filesInDocumentPath.Count();
            IEnumerable<ScanApp.Data.Entities.Image> images = await _imageService.Get(x => x.DocumentId == documentId);
            images.OrderBy(x => x.Order).ToList();
            int totalImagesInDatabase = images.Count();

            ObservableCollection<ScannedImage> scannedImages = new ObservableCollection<ScannedImage>();

            foreach (var item in images)
            {
                string path = System.IO.Path.Combine(userFolderPath, item.ImagePath);
                BitmapImage bitmapImage = ImagePathToBitmap(path);

                ScannedImage scannedImage = new ScannedImage()
                {
                    Id = item.Id,
                    DocumentId = documentId,
                    ImageName = item.ImageName,
                    ImagePath = path,
                    IsSelected = false,
                    Order = item.Order,
                    bitmapImage = bitmapImage
                };

                scannedImages.Add(scannedImage);
            }

            int latestOrder = scannedImages.Count != 0
                ? scannedImages.Max(x => x.Order) + 1
                : 0;

            if (totalImagesInPath > totalImagesInDatabase)
            {
                int totalImageUnsaved = totalImagesInPath - totalImagesInDatabase;
                MessageBoxResult checkImageResult = System.Windows.MessageBox.Show($"Bạn có {totalImageUnsaved} ảnh chưa được lưu. Bạn có muốn hiển thị?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (checkImageResult == MessageBoxResult.Yes)
                {
                    List<string> savedImagesName = images.Select(x => x.ImageName).ToList();
                    List<string> imagesInPathFileName = filesInDocumentPath
                        .Select(x => System.IO.Path.GetFileName(x))
                        .ToList();

                    List<string> unsavedImagesName = imagesInPathFileName.Except(savedImagesName, StringComparer.OrdinalIgnoreCase).ToList();

                    foreach (var item in unsavedImagesName)
                    {
                        string path = System.IO.Path.Combine(documentPath, item);
                        BitmapImage bitmapImage = ImagePathToBitmap(path);

                        ScannedImage scannedImage = new ScannedImage()
                        {
                            Id = 0,
                            DocumentId = documentId,
                            ImageName = item,
                            ImagePath = path,
                            IsSelected = false,
                            Order = latestOrder,
                            bitmapImage = bitmapImage
                        };

                        scannedImages.Add(scannedImage);
                        latestOrder++;
                    }
                }
            }

            ListImagesMain.Clear();
            ListImagesMain = scannedImages;
        }

        private BitmapImage ImagePathToBitmap(string imagePath)
        {
            BitmapImage bitmapImage = new BitmapImage();

            try
            {
                BitmapImage bitmapImageFromPath = new BitmapImage(new Uri(imagePath));
                WriteableBitmap writeableBitmap = new WriteableBitmap(bitmapImageFromPath);
                bitmapImage = ConvertWriteableBitmapToBitmapImage(writeableBitmap);
                return bitmapImage;
            }
            catch (Exception)
            {
                return bitmapImage;
            }
        }

        private BitmapImage ConvertWriteableBitmapToBitmapImage(WriteableBitmap writeableBitmap)
        {
            BitmapImage bitmapImage = new BitmapImage();

            try
            {
                using (var stream = new System.IO.MemoryStream())
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));
                    encoder.Save(stream);
                    stream.Seek(0, System.IO.SeekOrigin.Begin);

                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                }

                return bitmapImage;
            }
            catch (Exception)
            {
                return bitmapImage;
            }
        }

        private ObservableCollection<ScannedImage> _listImagesMain { get; set; }

        public ObservableCollection<ScannedImage> ListImagesMain
        {
            get => _listImagesMain;
            set
            {
                _listImagesMain = value;
                OnPropertyChanged("ListImagesMain");
            }
        }

        private ObservableCollection<ScannedImage> _listImagesSelected { get; set; }

        public ObservableCollection<ScannedImage> ListImagesSelected
        {
            get => _listImagesSelected;
            set
            {
                _listImagesMain = value;
                OnPropertyChanged("ListImagesSelected");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}