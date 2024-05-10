using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OriginalScan.Models
{
    public class ScannedImage : BaseModel
    {
        private int _id { get; set; }

        public int Id
        {
            get => _id; 
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
        }

        private int _documentId { get; set; }

        public int DocumentId
        {
            get => _documentId;
            set
            {
                _documentId = value;
                OnPropertyChanged("DocumentId");
            }
        }

        private string _imageName { get; set; } = null!;

        public string ImageName
        {
            get => _imageName;
            set
            {
                _imageName = value;
                OnPropertyChanged("ImageName");
            }
        }

        private string _imagePath { get; set; } = null!;

        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                OnPropertyChanged("ImagePath");
            }
        }

        private bool _isSelected { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        private BitmapImage _bitmapImage { get; set; } = null!;

        public BitmapImage bitmapImage
        {
            get => _bitmapImage;
            set
            {
                _bitmapImage = value;
                OnPropertyChanged("BitmapImage");
            }
        }

        private int _order { get; set; }

        public int Order
        {
            get => _order;
            set
            {
                _order = value;
                OnPropertyChanged("Order");
            }
        }
    }
}
