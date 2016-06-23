using Assets;
using System.Collections.Generic;
using System.Windows;

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
