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
    
    public partial class MaterialEditor : Window, INotifyPropertyChanged
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

        public List<ParameterGroup> Test { get; set; }
        public ParameterGroup SelectedParameterGroup { get; set; }

        public MaterialAsset asset { get; set; }
        public ObservableCollection<ShaderAsset> AvailableVertexShaders { get; set; }
        public ObservableCollection<ShaderAsset> AvailableGeometryShaders { get; set; }
        public ObservableCollection<ShaderAsset> AvailablePixelShaders { get; set; }
        public ObservableCollection<TextureAsset> AvailableTextures { get; set; }

        public List<DepthWriteMask> AvailableDepthWriteMasks { get; set; }
        public List<ComparisonFunc> AvailableDepthFuncs { get; set; }

        public Texture SelectedTexture { get; set; }
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

        public MaterialEditor(MaterialAsset material)
        {
            InitializeComponent();

            asset = material;

            switch(material.ShaderCombination)
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

        public MaterialEditor()
        {
            InitializeComponent();

            asset = new MaterialAsset();
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

            foreach(var texture in asset.Textures)
            {
                if (texture.Source == null)
                {
                    MessageBox.Show("Must specify texture source, its currently blank");
                    return;
                }
            }

            foreach(var sampler in asset.Samplers)
            {
                if (sampler.Name == "UNNAMED" || string.IsNullOrEmpty(sampler.Name))
                {
                    MessageBox.Show("a sampler wasn't given a name, please go back and name it");
                    return;
                }
            }

            var materialPath = @"C:\ProjectStacks\ImportedAssets\Materials\";
            var outputName = System.IO.Path.Combine(materialPath, System.IO.Path.ChangeExtension(asset.Name, "mat"));

            if (!Directory.Exists(materialPath))
            {
                Directory.CreateDirectory(materialPath);
            }

            asset.ImportedFilename = System.IO.Path.GetFullPath(outputName);

            if (!InEditMode)
            {
                if (File.Exists(asset.ImportedFilename))
                {
                    MessageBox.Show("An imported material with the same name already exists, stopping");
                    return;
                }
            }

            MaterialImporter.Import(asset);
            
            this.DialogResult = true;
            this.Close();
        }

        private void AddArrayParameterElementFloat4(object sender, RoutedEventArgs e)
        {
            if (SelectedParameterGroup == null
                || SelectedParameterGroup.SelectedParameter == null)
                return;
            
            var array = SelectedParameterGroup.SelectedParameter as ArrayParam;

            if (array != null)
            {
                array.AddElement(new Vector4Parameter<float> { Name = "new" });
            }                            
        }

        private void AddArrayParameterElementInt4(object sender, RoutedEventArgs e)
        {
            if (SelectedParameterGroup == null
                || SelectedParameterGroup.SelectedParameter == null)
                return;

            var array = SelectedParameterGroup.SelectedParameter as ArrayParam;

            if (array != null)
            {
                array.AddElement(new Vector4Parameter<int> { Name = "new" });
            }   
        }

        private void AddArrayParameterElementBool4(object sender, RoutedEventArgs e)
        {
            if (SelectedParameterGroup == null
                || SelectedParameterGroup.SelectedParameter == null)
                return;

            var array = SelectedParameterGroup.SelectedParameter as ArrayParam;

            if (array != null)
            {
                array.AddElement(new Vector4Parameter<bool> { Name = "new" });
            }   
        }

        private void RemoveArrayParameterElement(object sender, RoutedEventArgs e)
        {
            if (SelectedParameterGroup == null
                || SelectedParameterGroup.SelectedParameter == null)
                return;

            var array = SelectedParameterGroup.SelectedParameter as ArrayParam;

            if (array != null) 
            {
                array.RemoveSelectedElement();
            }
             
        }

        private void AddParameter(object sender, RoutedEventArgs e)
        {
            if (SelectedParameterGroup != null)
            {
                var parameterCreator = new NewParameter();                

                var result = parameterCreator.ShowDialog();
                
                if (result == true)
                {
                    SelectedParameterGroup.Parameters.Add(parameterCreator.Parameter);
                }
            }
        }

        private void RemoveParameter(object sender, RoutedEventArgs e)
        {
            if (SelectedParameterGroup != null)
            {
                if (SelectedParameterGroup.SelectedParameter != null)
                {
                    SelectedParameterGroup.Parameters.Remove(SelectedParameterGroup.SelectedParameter);
                }
            }
        }

        private void NewParameterGroup(object sender, RoutedEventArgs e)
        {
            asset.ParameterGroups.Add(new ParameterGroup {  Name = "new"});
        }

        private void RemoveParameterGroup(object sender, RoutedEventArgs e)
        {
            if (SelectedParameterGroup != null)
            {
                asset.ParameterGroups.Remove(SelectedParameterGroup);
            }
        }

        private void AddTexture(object sender, RoutedEventArgs e)
        {
            asset.Textures.Add(new Texture());
        }

        private void RemoveSelectedTexture(object sender, RoutedEventArgs e)
        {
            if (SelectedTexture != null)
            {
                asset.Textures.Remove(SelectedTexture);
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
