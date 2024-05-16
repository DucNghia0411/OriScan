using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace OriginalScan
{
    class TreeViewLineConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            TreeViewItem? item = values[2] as TreeViewItem;
            ItemsControl ic = ItemsControl.ItemsControlFromItemContainer(item);
            bool isLastOne = ic.ItemContainerGenerator.IndexFromContainer(item) == ic.Items.Count - 1;

            System.Windows.Shapes.Rectangle? rectangle = values[3] as System.Windows.Shapes.Rectangle;
            if (rectangle == null)
            {
                return double.NaN;
            }
            if (isLastOne)
            {
                rectangle.VerticalAlignment = VerticalAlignment.Top;
                return 17.0;
            }
            else
            {
                rectangle.VerticalAlignment = VerticalAlignment.Stretch;
                return double.NaN;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
