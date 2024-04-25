using FontAwesome5;
using Notification.Wpf;
using ScanApp.Data.Entities;
using ScanApp.Model.Models;
using ScanApp.Service.Constracts;
using ScanApp.Service.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;

namespace OriginalScan.Views
{
    /// <summary>
    /// Interaction logic for BatchDetailWindow.xaml
    /// </summary>
    public partial class BatchDetailWindow : Window
    {
        private readonly IBatchService _batchService;
        private readonly NotificationManager _notificationManager;

        public BatchDetailWindow(IBatchService batchService)
        {
            _batchService = batchService;

            _notificationManager = new NotificationManager();
            InitializeComponent();
            GetBatch();
        }

        public async void GetBatch()
        {
            try
            {
                var batchModel = _batchService.SelectedBatch;

                var batch = await _batchService.FirstOrDefault(e => e.Id == batchModel.Id);
                

                if (batch != null)
                {
                    txtBatchName.Text = batch.BatchName;
                    txtNote.Text = batch.Note;
                    txtCreatedDate.Text = batch.CreatedDate;
                    txtPath.Text = batch.BatchPath;
                }
            }
            catch(Exception ex)
            {
                var errorNoti = new NotificationContent
                {
                    Title = "Lỗi!",
                    Message = $"Có lỗi: {ex.Message}",
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void btnCancelDocument_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }
    }
}
