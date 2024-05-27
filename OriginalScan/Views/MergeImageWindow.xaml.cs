using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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

using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MessageBox = System.Windows.Forms.MessageBox;
using DragEventArgs = System.Windows.DragEventArgs;
using DataFormats = System.Windows.Forms.DataFormats;
using Notification.Wpf;
using ScanApp.Service.Constracts;
using OriginalScan.Models;
using ScanApp.Service.Services;
using FontAwesome5;
using PdfSharp.Drawing;
using OriginalScan.Converters;

namespace OriginalScan.Views
{
    /// <summary>
    /// Interaction logic for MergeImageWindow.xaml
    /// </summary>
    public partial class MergeImageWindow : Window
    {
        private bool isDragging = false;
        private Image? draggedImage;
        private Point offset;

        public ImageSource Source1 { get; }
        public ImageSource Source2 { get; }

        public double Image1Width { get;}
        public double Image1Height { get; }

        public double Image2Width { get; }
        public double Image2Height { get; }

        public double scale { get; }

        private bool isDraggingFinal = false;
        private Image? draggedImageFinal;
        private Point offsetFinal;

        private ScannedImage firstImage { get; set; }

        private ScannedImage _secondImage { get; }

        private RenderTargetBitmap? _mergeImageBitmap;

        private readonly IImageService _imageService;
        private readonly IDocumentService _documentService;
        private readonly NotificationManager _notificationManager;

        public MergeImageWindow(ScannedImage inputFirstImage, ScannedImage secondImage, IImageService imageService, IDocumentService documentService)
        {
            this._imageService = imageService;
            this._documentService = documentService;
            _notificationManager = new NotificationManager();

            InitializeComponent();

            firstImage = inputFirstImage;
            _secondImage = secondImage;

            scale = 0.4;
            Source1 = inputFirstImage.bitmapImage;
            Image1Height = Source1.Height * scale;
            Image1Width = Source1.Width * scale;

            Source2 = secondImage.bitmapImage;
            Image2Height = Source2.Height * scale;
            Image2Width = Source2.Width * scale;

            Loaded += MainWindow_Loaded;
            DataContext = this;
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

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && draggedImage != null)
            {   
                Point mousePos = e.GetPosition(canvas);
                double offsetX = mousePos.X - offset.X;
                double offsetY = mousePos.Y - offset.Y;

                // Di chuyển phần tử đang được kéo
                double newLeft = Canvas.GetLeft(draggedImage) + offsetX;
                double newTop = Canvas.GetTop(draggedImage) + offsetY;

                // Kiểm tra nếu di chuyển lên trên
                if (offsetY < 0)
                {
                    // Đảm bảo hình ảnh không bị mất khỏi canvas ở phía trên
                    if (newTop < 0)
                    {
                        newTop = 0;
                    }
                }

                // Kiểm tra nếu di chuyển sang trái
                if (offsetX < 0)
                {
                    // Đảm bảo hình ảnh không bị mất khỏi canvas ở phía trái
                    if (newLeft < 0)
                    {
                        newLeft = 0;
                    }
                }

                // Di chuyển hình ảnh
                Canvas.SetLeft(draggedImage, newLeft);
                Canvas.SetTop(draggedImage, newTop);

                // Tự động điều chỉnh kích thước của canvas để xuất hiện thanh cuộn
                double rightEdge = newLeft + draggedImage.ActualWidth;
                double bottomEdge = newTop + draggedImage.ActualHeight;

                if (rightEdge > canvas.ActualWidth)
                {
                    canvas.Width = rightEdge;
                }

                if (bottomEdge > canvas.ActualHeight)
                {
                    canvas.Height = bottomEdge;
                }

                offset = mousePos;
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            offset = e.GetPosition(canvas);
            draggedImage = (Image)sender;
            draggedImage.CaptureMouse();
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            if(draggedImage != null)
            {
                draggedImage.ReleaseMouseCapture();
            }
        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    // Load the image and set it as the source of the draggedImage
                    draggedImage.Source = new System.Windows.Media.Imaging.BitmapImage(new System.Uri(files[0]));
                }
            }
        }

        private void MergeButton_Click(object sender, RoutedEventArgs e)
        {
            MergeImage();
        }

        private void MergeImage()
        {
            double originalWidth1 = image1.Source.Width;
            double originalHeight1 = image1.Source.Height;
            double originalWidth2 = image2.Source.Width;
            double originalHeight2 = image2.Source.Height;

            double scale = image1.ActualWidth / originalWidth1;

            Point image1Position = image1.TranslatePoint(new Point(0, 0), this);
            Point image2Position = image2.TranslatePoint(new Point(0, 0), this);

            double relativeX = (image2Position.X - image1Position.X) / scale;
            double relativeY = (image2Position.Y - image1Position.Y) / scale;

            DrawingVisual visual = new DrawingVisual();

            using (DrawingContext context = visual.RenderOpen())
            {
                context.DrawImage(image1.Source, new Rect(0, 0, originalWidth1, originalHeight1));

                context.DrawImage(image2.Source, new Rect(relativeX, relativeY, originalWidth2, originalHeight2));
            }

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)(Math.Max(originalWidth1, relativeX + originalWidth2)),
                                                            (int)(Math.Max(originalHeight1, relativeY + originalHeight2)),
                                                            96, 96, PixelFormats.Pbgra32);

            rtb.Render(visual);
            _mergeImageBitmap = rtb;
            BitmapImage bitmapImage = BitmapConverter.ConvertRenderTargetBitmapToBitmapImage(rtb);

            double scalePercent = 0.8;
            ScaleTransform scaleTransform = new ScaleTransform(scalePercent, scalePercent);
            TransformedBitmap transformedBitmap = new TransformedBitmap(bitmapImage, scaleTransform);
            mergedImage.Source = transformedBitmap;
        }

        private void ImageFinal_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingFinal && draggedImageFinal != null)
            {
                Point mousePos = e.GetPosition(canvasFinal);
                double offsetX = mousePos.X - offsetFinal.X;
                double offsetY = mousePos.Y - offsetFinal.Y;

                // Di chuyển phần tử đang được kéo
                double newLeft = Canvas.GetLeft(draggedImageFinal) + offsetX;
                double newTop = Canvas.GetTop(draggedImageFinal) + offsetY;

                // Kiểm tra nếu di chuyển lên trên
                if (offsetY < 0)
                {
                    // Đảm bảo hình ảnh không bị mất khỏi canvas ở phía trên
                    if (newTop < 0)
                    {
                        newTop = 0;
                    }
                }

                // Kiểm tra nếu di chuyển sang trái
                if (offsetX < 0)
                {
                    // Đảm bảo hình ảnh không bị mất khỏi canvas ở phía trái
                    if (newLeft < 0)
                    {
                        newLeft = 0;
                    }
                }

                // Di chuyển hình ảnh
                Canvas.SetLeft(draggedImageFinal, newLeft);
                Canvas.SetTop(draggedImageFinal, newTop);

                // Tự động điều chỉnh kích thước của canvas để xuất hiện thanh cuộn
                double rightEdge = newLeft + draggedImageFinal.ActualWidth;
                double bottomEdge = newTop + draggedImageFinal.ActualHeight;

                if (rightEdge > canvasFinal.ActualWidth)
                {
                    canvasFinal.Width = rightEdge;
                }

                if (bottomEdge > canvasFinal.ActualHeight)
                {
                    canvasFinal.Height = bottomEdge;
                }

                offsetFinal = mousePos;
            }
        }

        private void ImageFinal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDraggingFinal = true;
            offsetFinal = e.GetPosition(canvasFinal);
            draggedImageFinal = (Image)sender;
            draggedImageFinal.CaptureMouse();
        }

        private void ImageFinal_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDraggingFinal = false;
            if (draggedImageFinal != null)
            {
                draggedImageFinal.ReleaseMouseCapture();
            }
        }

        private void CanvasFinal_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    // Load the image and set it as the source of the draggedImage
                    draggedImage.Source = new System.Windows.Media.Imaging.BitmapImage(new System.Uri(files[0]));
                }
            }
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            image1.Width *= 1.2;
            image1.Height *= 1.2;
            image2.Width *= 1.2;
            image2.Height *= 1.2;
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            image1.Width /= 1.2;
            image1.Height /= 1.2;
            image2.Width /= 1.2;
            image2.Height /= 1.2;
        }

        private void ZoomInFinalButton_Click(object sender, RoutedEventArgs e)
        {
            double currentScale = GetScaleTransformValue(mergedImage);
            double newScale = currentScale * 1.1; 

            ApplyScaleTransform(newScale);
        }

        private void ZoomOutFinalButton_Click(object sender, RoutedEventArgs e)
        {
            double currentScale = GetScaleTransformValue(mergedImage);
            double newScale = currentScale * 0.9; 

            ApplyScaleTransform(newScale);
        }

        private double GetScaleTransformValue(UIElement element)
        {
            if (element.RenderTransform is ScaleTransform scaleTransform)
            {
                return scaleTransform.ScaleX; 
            }
            return 1.0;
        }

        private void ApplyScaleTransform(double newScale)
        {
            ScaleTransform scaleTransform = new ScaleTransform(newScale, newScale);
            mergedImage.RenderTransform = scaleTransform;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas.SetLeft(image2, Canvas.GetLeft(image1) + image1.ActualWidth);
            Canvas.SetTop(image2, Canvas.GetTop(image1));
        }

        private void btnConfirmMerge_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show($"Bạn chắc chắn có muốn ghép 2 hình ảnh này lại, thông tin cũ sẽ không được lưu trữ ?", "Xác nhận", MessageBoxButton.OKCancel);
            if(result == MessageBoxResult.OK)
            {
                SaveMergeImage();
            }
            else
            {
                return;
            }
        }

        private async void SaveMergeImage()
        {
            if (_mergeImageBitmap == null)
            {
                NotificationShow("warning", $"Vui lòng ghép hình ảnh trước khi lưu!");
                return;
            }

            ScannedImage replaceImage;
            ScannedImage deleteImage;

            if (firstImage.Id == 0 && _secondImage.Id != 0)
            {
                replaceImage = _secondImage;
                deleteImage = firstImage;
            }
            else if (firstImage.Id != 0 && _secondImage.Id == 0)
            {
                replaceImage = firstImage;
                deleteImage = _secondImage;
            }
            else
            {
                replaceImage = firstImage.Order < _secondImage.Order ? firstImage : _secondImage;
                deleteImage = firstImage.Order > _secondImage.Order ? firstImage : _secondImage;
            }

            if (deleteImage.Id != 0)
            {
                var resortResult = await _imageService.ReSort(deleteImage.Id);
                if (!resortResult)
                {
                    NotificationShow("error", $"Có lỗi xảy ra trong quá trình xử lý!");
                    return;
                }

                var deleteResult = await _imageService.Delete(deleteImage.Id);
                if (!deleteResult)
                {
                    NotificationShow("error", $"Có lỗi xảy ra trong quá trình xử lý!");
                    return;
                }
            }
            else
            {
                MessageBoxResult pdfConfirm = System.Windows.MessageBox.Show("Tồn tại ảnh chưa được lưu, vẫn tiến hành ghép?", "Thông báo!", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (pdfConfirm == MessageBoxResult.No)
                {
                    return;
                }
            }

            RenderTargetBitmap rtb = _mergeImageBitmap;
            replaceImage.bitmapImage = BitmapConverter.ConvertRenderTargetBitmapToBitmapImage(rtb);

            MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;

            if (_documentService.SelectedDocument == null)
            {
                NotificationShow("warning", $"Vui lòng truy cập lại tài liệu bạn muốn thực hiện!");
                return;
            }

            var listImage = mainWindow.ListImagesMain;

            if (deleteImage.Id != 0)
                ReSort(deleteImage.Order);

            firstImage = replaceImage;
            listImage.Remove(_secondImage);

            string filePath = replaceImage.ImagePath;
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));
                encoder.Save(stream);
            }

            try
            {
                File.Delete(deleteImage.ImagePath);
            }
            catch (Exception)
            {
                NotificationShow("warning", $"Có lỗi xảy ra nhưng quá trình vẫn tiếp tục!");
            }

            mainWindow.GetImagesByDocument(_documentService.SelectedDocument.Id, false);
            mainWindow.ListImagesSelected.Clear();
            NotificationShow("success", $"Ghép ảnh thành công!");
            this.Close();
        }

        private void ReSort(int order)
        {
            MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
            var listImage = mainWindow.ListImagesMain;

            int orderToRemove  = order - 1;
            listImage.RemoveAt(orderToRemove);

            for (int i = orderToRemove; i < listImage.Count; i++)
            {
                listImage[i].Order = listImage[i].Order - 1;
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            if (_mergeImageBitmap == null) return;
            MessageBoxResult Result = System.Windows.MessageBox.Show($"Bạn có muốn thoát mà không lưu những thay đổi?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (Result == MessageBoxResult.Yes)
            {
                this.Close();
            }

            else return;
        }
    }
}
