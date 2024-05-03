using System.Windows;
using System.Collections.ObjectModel;

namespace OriginalScan
{
    public class TreeViewItemViewModel
    {
        public string Name { get; set; }
        public string Icon { get; set; }

        public ObservableCollection<TreeViewItemViewModel> Children { get; set; }

        public TreeViewItemViewModel(string name, string icon)
        {
            Name = name;
            Icon = icon;
            Children = new ObservableCollection<TreeViewItemViewModel>();
        }
    }
}
