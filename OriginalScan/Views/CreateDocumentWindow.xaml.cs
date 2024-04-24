using ScanApp.Data.Entities;
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
using System.Windows.Shapes;

namespace OriginalScan.Views
{
    /// <summary>
    /// Interaction logic for CreateDocumentWindow.xaml
    /// </summary>
    public partial class CreateDocumentWindow : Window
    {
        public CreateDocumentWindow()
        {
            InitializeComponent();
            this.ResizeMode = ResizeMode.NoResize;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void CreateDocument_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancelDocument_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }
    }
}
