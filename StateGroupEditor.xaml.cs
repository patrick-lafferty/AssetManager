using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Assets;
using System.IO;
using Importers;

namespace Glitch2
{

    public partial class StateGroupEditor : Window, INotifyPropertyChanged
    {
        public bool InEditMode { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        void NotifyPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }

        public ParameterGroup SelectedParameterGroup { get; set; }

        public StateGroupAsset asset { get; set; }
        public ObservableCollection<ShaderAsset> AvailableVertexShaders { get; set; }
        public ObservableCollection<ShaderAsset> AvailableGeometryShaders { get; set; }
        public ObservableCollection<ShaderAsset> AvailablePixelShaders { get; set; }

        public List<DepthWriteMask> AvailableDepthWriteMasks { get; set; }
        public List<ComparisonFunc> AvailableDepthFuncs { get; set; }

        public TextureBinding SelectedTexture { get; set; }
        public Sampler SelectedSampler { get; set; }
        public RenderTarget SelectedRenderTarget { get; set; }

        Visibility vertexPixelVisible;
        public Visibility VertexPixelVisible
        {
            get { return vertexPixelVisible; }
            set
            {
                vertexPixelVisible = value;
                NotifyPropertyChanged("VertexPixelVisible");
            }
        }

        Visibility vertexGeometryPixelVisible;
        public Visibility VertexGeometryPixelVisible
        {
            get { return vertexGeometryPixelVisible; }
            set
            {
                vertexGeometryPixelVisible = value;
                NotifyPropertyChanged("VertexGeometryPixelVisible");
            }
        }

        Visibility vertexGeometryVisible;
        public Visibility VertexGeometryVisible
        {
            get { return vertexGeometryVisible; }
            set
            {
                vertexGeometryVisible = value;
                NotifyPropertyChanged("VertexGeometryVisible");
            }
        }

        bool vertexEnabled;
        public bool VertexEnabled
        {
            get
            {
                return vertexEnabled;
            }
            set
            {
                vertexEnabled = value;
                NotifyPropertyChanged("VertexEnabled");
            }

        }

        bool geometryEnabled;
        public bool GeometryEnabled
        {
            get
            {
                return geometryEnabled;
            }
            set
            {
                geometryEnabled = value;
                NotifyPropertyChanged("GeometryEnabled");
            }

        }

        bool pixelEnabled;
        public bool PixelEnabled
        {
            get
            {
                return pixelEnabled;
            }
            set
            {
                pixelEnabled = value;
                NotifyPropertyChanged("PixelEnabled");
            }

        }

        public StateGroupEditor(StateGroupAsset stateGroup)
        {
            InitializeComponent();

            asset = stateGroup;

            switch (stateGroup.ShaderCombination)
            {
                case ShaderCombination.VertexPixel:
                    {
                        VertexPixel.IsChecked = true;
                        break;
                    }
                case ShaderCombination.VertexGeometryPixel:
                    {
                        VertexGeometryPixel.IsChecked = true;
                        break;
                    }
                case ShaderCombination.VertexGeometry:
                    {
                        VertexGeometry.IsChecked = true;
                        break;
                    }
            }

            CombinationSelected(null, null);

            InEditMode = true;
            this.DataContext = this;

            AvailableDepthWriteMasks = Enum.GetValues(typeof(DepthWriteMask)).Cast<DepthWriteMask>().ToList();
            AvailableDepthFuncs = Enum.GetValues(typeof(ComparisonFunc)).Cast<ComparisonFunc>().ToList();
        }

        public StateGroupEditor()
        {
            InitializeComponent();

            asset = new StateGroupAsset();
            VertexEnabled = true;
            PixelEnabled = true;
            VertexPixel.IsChecked = true;
            InEditMode = false;
            this.DataContext = this;

            AvailableDepthWriteMasks = Enum.GetValues(typeof(DepthWriteMask)).Cast<DepthWriteMask>().ToList();
            AvailableDepthFuncs = Enum.GetValues(typeof(ComparisonFunc)).Cast<ComparisonFunc>().ToList();
        }

        private void RemoveSampler(object sender, RoutedEventArgs e)
        {

        }

        private void CombinationSelected(object sender, RoutedEventArgs e)
        {
            if (VertexGeometry.IsChecked == true)
            {
                VertexEnabled = true;
                GeometryEnabled = true;
                PixelEnabled = false;

                VertexPixelVisible = Visibility.Collapsed;
                VertexGeometryVisible = Visibility.Visible;
                VertexGeometryPixelVisible = Visibility.Collapsed;
            }
            else if (VertexGeometryPixel.IsChecked == true)
            {
                VertexEnabled = true;
                GeometryEnabled = true;
                PixelEnabled = true;

                VertexPixelVisible = Visibility.Collapsed;
                VertexGeometryVisible = Visibility.Collapsed;
                VertexGeometryPixelVisible = Visibility.Visible;
            }
            else if (VertexPixel.IsChecked == true)
            {
                VertexEnabled = true;
                GeometryEnabled = false;
                PixelEnabled = true;

                VertexPixelVisible = Visibility.Visible;
                VertexGeometryVisible = Visibility.Collapsed;
                VertexGeometryPixelVisible = Visibility.Collapsed;
            }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(asset.Name)
                || string.IsNullOrEmpty(asset.Description))
            {
                MessageBox.Show("Name/Description can't be empty!");
                return;
            }

            if (VertexGeometry.IsChecked == true)
            {
                if (asset.VertexShader == null || asset.GeometryShader == null)
                {
                    MessageBox.Show("Vertex/Geometry shader needs to be selected");
                    return;
                }
            }
            else if (VertexGeometryPixel.IsChecked == true)
            {
                if (asset.VertexShader == null || asset.GeometryShader == null || asset.PixelShader == null)
                {
                    MessageBox.Show("Vertex/Geometry/Pixel shader needs to be selected");
                    return;
                }
            }
            else if (VertexPixel.IsChecked == true)
            {
                if (asset.VertexShader == null || asset.PixelShader == null)
                {
                    MessageBox.Show("Vertex/Pixel shader needs to be selected");
                    return;
                }
            }

            foreach (var texture in asset.TextureBindings)
            {
                if (texture.Binding == null)
                {
                    MessageBox.Show("Must specify texture binding, its currently blank");
                    return;
                }
            }

            foreach (var sampler in asset.Samplers)
            {
                if (sampler.Name == "UNNAMED" || string.IsNullOrEmpty(sampler.Name))
                {
                    MessageBox.Show("a sampler wasn't given a name, please go back and name it");
                    return;
                }
            }

            var stateGroupPath = @"C:\ProjectStacks\ImportedAssets\StateGroups\";
            var outputName = System.IO.Path.Combine(stateGroupPath, System.IO.Path.ChangeExtension(asset.Name, "stgp"));

            if (!Directory.Exists(stateGroupPath))
            {
                Directory.CreateDirectory(stateGroupPath);
            }

            asset.ImportedFilename = System.IO.Path.GetFullPath(outputName);

            if (!InEditMode)
            {
                if (File.Exists(asset.ImportedFilename))
                {
                    MessageBox.Show("An imported state group with the same name already exists, stopping");
                    return;
                }
            }

            StateGroupImporter.Import(asset);

            this.DialogResult = true;
            this.Close();
        }

        private void AddTexture(object sender, RoutedEventArgs e)
        {
            asset.TextureBindings.Add(new TextureBinding());
        }

        private void RemoveSelectedTexture(object sender, RoutedEventArgs e)
        {
            if (SelectedTexture != null)
            {
                asset.TextureBindings.Remove(SelectedTexture);
                SelectedTexture = null;
            }
        }

        private void AddSampler(object sender, RoutedEventArgs e)
        {
            asset.Samplers.Add(new Sampler() { Name = "UNNAMED" });
        }

        private void RemoveSelectedSampler(object sender, RoutedEventArgs e)
        {
            if (SelectedSampler != null)
            {
                asset.Samplers.Remove(SelectedSampler);
                SelectedSampler = null;
            }
        }

        private void AddRenderTarget(object sender, RoutedEventArgs e)
        {
            asset.BlendState.RenderTargets.Add(new RenderTarget());
        }

        private void RemoveSelectedRenderTarget(object sender, RoutedEventArgs e)
        {
            if (SelectedRenderTarget != null)
            {
                asset.BlendState.RenderTargets.Remove(SelectedRenderTarget);
                SelectedRenderTarget = null;
            }
        }
    }
}
