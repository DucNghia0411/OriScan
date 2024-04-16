using NTwain;
using System;
using System.Collections.Generic;
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

namespace OriginalScan.Views
{
    /// <summary>
    /// Interaction logic for DeviceWindow.xaml
    /// </summary>
    public partial class DeviceWindow : Window
    {
        public TwainSession? twainSession;
        public MainWindow? mainWindow;
        public DataSource? dataSource;

        public IEnumerable<DataSource> dataSources = Enumerable.Empty<DataSource>();

        public DeviceWindow()
        {
            InitializeComponent();
        }

        public void GetListDevice()
        {
            try
            {
                if (twainSession != null)
                {
                    dataSources = twainSession.GetSources().ToList();
                    lbDevice.ItemsSource = dataSources;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Có lỗi: {ex.Message}", "Thông báo!!", MessageBoxButtons.OK);
                return;
            }   
        }

        private void SettingDevice(DataSource dataSource)
        {
            try
            {
                if (dataSource == null)
                {
                    System.Windows.Forms.MessageBox.Show($"Bạn chưa chọn máy Scan", "Thông báo!!", MessageBoxButtons.OK);
                    return;
                }
                if (mainWindow != null)
                {
                    mainWindow.dataSource = dataSource;
                    txtCurrenDevice.Text = dataSource.Name;
                    mainWindow.dataSource = dataSource;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Có lỗi: {ex.Message}", "Thông báo!!", MessageBoxButtons.OK);
                return;
            }
        }

        private void lblDevice_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DataSource dataSource = (DataSource)lbDevice.SelectedItem;
                SettingDevice(dataSource);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Có lỗi: {ex.Message}", "Thông báo!!", MessageBoxButtons.OK);
                return;
            }
        }
    }
}
