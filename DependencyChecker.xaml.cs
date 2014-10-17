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

namespace Glitch2
{
    /// <summary>
    /// Interaction logic for DependencyChecker.xaml
    /// </summary>

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
