using FontAwesome5;
using Notification.Wpf.Constants;
using Notification.Wpf.Controls;
using Notification.Wpf;
using OriginalScan.Converters;
using OriginalScan.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OriginalScan.Views.Pages
{
    /// <summary>
    /// Interaction logic for CutImageWindow.xaml
    /// </summary>
    public partial class CutImagePage : Page
    {
        private ScannedImage _image { get; }
        private readonly NotificationManager _notificationManager;

        private bool isDragging = false;
        private Point startPoint;
        private Rectangle currentRectangle;

        public double imageWidth { get; }
        private BitmapImage _bitmapImage;
        private double _scale = 0.5;

        private Stack<BitmapImage> _undoStack = new Stack<BitmapImage>();
        private Stack<BitmapImage> _redoStack = new Stack<BitmapImage>();
        private BitmapImage _displayedImage;
        private RenderTargetBitmap _mergeImageBitmap;

        public CutImagePage(ScannedImage image)
        {
            _notificationManager = new NotificationManager();
            InitializeComponent();

            _image = image;
            _bitmapImage = image.bitmapImage;
            _displayedImage = image.bitmapImage;
            mainImage.Source = image.bitmapImage;
            imageWidth = _bitmapImage.Width * _scale;

            mainImage.MouseLeftButtonDown += MainImage_MouseLeftButtonDown;
            mainImage.MouseMove += MainImage_MouseMove;
            overlayCanvas.MouseLeftButtonUp += MainImage_MouseLeftButtonUp;

            DataContext = this;
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

        private void MainImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (currentRectangle != null)
                overlayCanvas.Children.Remove(currentRectangle);

            startPoint = e.GetPosition(mainImage);
            currentRectangle = new Rectangle
            {
                Stroke = Brushes.Red,
                StrokeThickness = 2,
                Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0))
            };

            Canvas.SetLeft(currentRectangle, startPoint.X);
            Canvas.SetTop(currentRectangle, startPoint.Y);
            overlayCanvas.Children.Add(currentRectangle);
            isDragging = true;
        }

        private void MainImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point newPoint = e.GetPosition(mainImage);

            if (startPoint.X - newPoint.X >= 0)
            {
                startPoint.X = newPoint.X;
            }

            if (startPoint.Y - newPoint.Y >= 0)
            {
                startPoint.Y = newPoint.Y;
            }

            isDragging = false;
        }

        private void MainImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging || currentRectangle == null)
                return;

            Point newPoint = e.GetPosition(mainImage);
            double width = Math.Abs(newPoint.X - startPoint.X);
            double height = Math.Abs(newPoint.Y - startPoint.Y);

            double left = Math.Min(newPoint.X, startPoint.X);
            double top = Math.Min(newPoint.Y, startPoint.Y);

            Canvas.SetLeft(currentRectangle, left);
            Canvas.SetTop(currentRectangle, top);
            currentRectangle.Width = width;
            currentRectangle.Height = height;
        }

        private void DrawRectangleButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentRectangle != null)
            {
                try
                {
                    int bitmapWidth = (int)_displayedImage.Width;
                    int bitmapHeight = (int)_displayedImage.Height;
                    int pixelWidth = (int)_displayedImage.PixelWidth;
                    int pixelHeight = (int)_displayedImage.PixelHeight;

                    Color backgroundColor = GetBackgroundColor();
                    currentRectangle.Fill = new SolidColorBrush(backgroundColor);
                    DrawingVisual visual = new DrawingVisual();
                    using (DrawingContext context = visual.RenderOpen())
                    {
                        context.DrawImage(_displayedImage, new Rect(0, 0, bitmapWidth, bitmapHeight));
                        context.DrawRectangle(new SolidColorBrush(backgroundColor), null, new Rect(startPoint.X / _scale, startPoint.Y / _scale, currentRectangle.Width / _scale, currentRectangle.Height / _scale));
                    }
                    RenderTargetBitmap renderBitmap = new RenderTargetBitmap(pixelWidth, pixelHeight, _displayedImage.DpiX, _displayedImage.DpiY, PixelFormats.Pbgra32);
                    renderBitmap.Render(visual);
                    _mergeImageBitmap = renderBitmap;

                    BitmapImage bitmapImage = BitmapConverter.ConvertRenderTargetBitmapToBitmapImage(renderBitmap);
                    double scalePercent = 0.4;
                    ScaleTransform scaleTransform = new ScaleTransform(scalePercent, scalePercent);
                    TransformedBitmap transformedBitmap = new TransformedBitmap(bitmapImage, scaleTransform);

                    _undoStack.Push(_displayedImage);
                    _displayedImage = bitmapImage;
                    mainImage.Source = transformedBitmap;
                    _redoStack.Clear();
                }
                catch (Exception ex)
                {
                    NotificationShow("error", ex.Message);
                }
            }
        }

        public void CheckImageFormat(ScannedImage image)
        {
            System.Windows.Media.PixelFormat pixelFormat = image.bitmapImage.Format;

            if (pixelFormat == PixelFormats.BlackWhite)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show($"Định dạng của hình ảnh không phù hợp với chức năng, bạn có muốn cập nhật?", "Xác nhận", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        int bitmapWidth = (int)image.bitmapImage.Width;
                        int bitmapHeight = (int)image.bitmapImage.Height;
                        int pixelWidth = (int)image.bitmapImage.PixelWidth;
                        int pixelHeight = (int)image.bitmapImage.PixelHeight;

                        DrawingVisual drawingVisual = new DrawingVisual();
                        using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                        {
                            drawingContext.DrawImage(_displayedImage, new Rect(0, 0, bitmapWidth, bitmapHeight));
                        }
                        RenderTargetBitmap targetBitmap = new RenderTargetBitmap(pixelWidth, pixelHeight, image.bitmapImage.DpiX, image.bitmapImage.DpiY, PixelFormats.Pbgra32);
                        targetBitmap.Render(drawingVisual);

                        BitmapImage bitmapImage = BitmapConverter.ConvertRenderTargetBitmapToBitmapImage(targetBitmap);
                        double scalePercent = 0.4;
                        ScaleTransform scaleTransform = new ScaleTransform(scalePercent, scalePercent);
                        TransformedBitmap transformedBitmap = new TransformedBitmap(bitmapImage, scaleTransform);
                        _displayedImage = bitmapImage;
                        mainImage.Source = transformedBitmap;

                        MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;

                        if (mainWindow == null || mainWindow.SelectedImage == null) return;
                        mainWindow.SelectedImage.bitmapImage = BitmapConverter.ConvertBitmapSourceToBitmapImage(_displayedImage);
                        NotificationShow("success", "Cập nhật thành công!!");
                    }
                    catch (Exception ex)
                    {
                        NotificationShow("error", ex.Message);
                    }
                }
                else
                    return;

                overlayCanvas.Children.Remove(currentRectangle);
            }

            MainWindow currentWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
            if (currentWindow != null)
            {
                currentWindow.MainFrame.Navigate(new CutImagePage(image));
            }
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            SaveImage();
        }

        private void SaveImage()
        {
            try
            {
                BitmapConverter.SaveImageToOriginalPath(_mergeImageBitmap, _image.ImagePath);

                MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
                if (mainWindow != null && mainWindow.SelectedImage != null)
                {
                    mainWindow.SelectedImage.bitmapImage = BitmapConverter.ConvertBitmapSourceToBitmapImage(_displayedImage);
                    mainWindow.grEditImage.Visibility = Visibility.Collapsed;
                    mainWindow.lstvImages.Visibility = Visibility.Visible;
                }

                NotificationShow("success", "Lưu ảnh thành công!");
                _undoStack.Clear();
                _redoStack.Clear(); 
            }
            catch (Exception ex)
            {
                NotificationShow("error", ex.Message);
            }
        }

        private Color GetBackgroundColor()
        {
            BitmapSource bitmapSource = (BitmapSource)_bitmapImage;

            int pixelX = (int)(bitmapSource.PixelWidth / 2);
            int pixelY = (int)(bitmapSource.PixelHeight / 2);

            CroppedBitmap croppedBitmap = new CroppedBitmap(bitmapSource, new Int32Rect(pixelX, pixelY, 1, 1));
            byte[] pixel = new byte[4];
            croppedBitmap.CopyPixels(pixel, 4, 0);

            return Color.FromArgb(pixel[3], pixel[2], pixel[1], pixel[0]);
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            if (_undoStack.Count > 0)
            {
                _redoStack.Push(_displayedImage);
                _displayedImage = _undoStack.Pop();
                mainImage.Source = _displayedImage;
                overlayCanvas.Children.Remove(currentRectangle);
            }
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            if (_redoStack.Count > 0)
            {
                _undoStack.Push(_displayedImage);
                _displayedImage = _redoStack.Pop();
                mainImage.Source = _displayedImage;
                overlayCanvas.Children.Remove(currentRectangle);
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult Result = System.Windows.MessageBox.Show($"Bạn có muốn lưu những thay đổi?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (Result == MessageBoxResult.Yes)
            {
                SaveImage();
            }

            MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
            if (mainWindow != null)
            {
                mainWindow.lstvImages.Visibility = Visibility.Visible;
                mainWindow.grEditImage.Visibility = Visibility.Collapsed;
            }
        }
    }
}
