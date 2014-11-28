using Assets;
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

namespace AssetManager
{
    /// <summary>
    /// Interaction logic for BaseMaterialOnStateGroup.xaml
    /// </summary>
    public partial class BaseMaterialOnStateGroup : Window
    {
        public BaseMaterialOnStateGroup(List<StateGroupAsset> stateGroups)
        {
            InitializeComponent();
            AvailableStateGroups = stateGroups;
            this.DataContext = this;
        }

        public StateGroupAsset SelectedStateGroup { get; set; }
        public List<StateGroupAsset> AvailableStateGroups { get; set; }

        private void CreateMaterial(object sender, RoutedEventArgs e)
        {
            if (SelectedStateGroup == null)
                return;

            this.DialogResult = true;
            this.Close();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
