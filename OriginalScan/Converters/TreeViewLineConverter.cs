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
    public class TreeViewLineConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[2] is not TreeViewItem item)
            {
                return double.NaN;
            }

            ItemsControl ic = ItemsControl.ItemsControlFromItemContainer(item);
            if (ic == null)
            {
                return double.NaN;
            }

            bool isLastOne = ic.ItemContainerGenerator.IndexFromContainer(item) == ic.Items.Count - 1;

            if (values[3] is not System.Windows.Shapes.Rectangle rectangle)
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
