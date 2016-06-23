using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;

namespace Glitch2
{
    public class Dependency
    {
        public string Name { get; set; }
        public string AssetType { get; set; }
    }

    public partial class DependencyChecker : Window
    {
        public List<Dependency> Dependencies { get; set; }

        public DependencyChecker()
        {
            InitializeComponent();
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            var view = new ListCollectionView(Dependencies);
            view.GroupDescriptions.Add(new PropertyGroupDescription("AssetType"));

            datagrid.ItemsSource = view;
        }
    }
}
