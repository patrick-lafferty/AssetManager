/*
MIT License

Copyright (c) 2016 Patrick Lafferty

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Assets;
using System.IO;
using Importers;

namespace AssetManager
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

        public ParameterGroup SelectedParameterGroup { get; set; }
        public ObservableCollection<TextureAsset> AvailableTextures { get; set; }

        public MaterialAsset asset { get; set; }

        public Texture SelectedTexture { get; set; }

        public MaterialEditor(MaterialAsset material)
        {
            InitializeComponent();

            asset = material;

            InEditMode = true;
            this.DataContext = this;
        }

        public MaterialEditor(StateGroupAsset basedStateGroup)
        {
            InitializeComponent();

            asset = new MaterialAsset();

            foreach(var binding in basedStateGroup.TextureBindings)
            {
                asset.Textures.Add(new Texture() { Binding = binding.Binding });
            }
            
            InEditMode = false;
            this.DataContext = this;
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(asset.Name)
                || string.IsNullOrEmpty(asset.Description))
            {
                MessageBox.Show("Name/Description can't be empty!");
                return;
            }

            foreach(var texture in asset.Textures)
            {
                if (texture.Source == null && !texture.IsProcedural)
                {
                    MessageBox.Show("Must specify texture source, its currently blank");
                    return;
                }
            }

            var materialPath = Path.Combine(Properties.Settings.Default.ImportedAssetsPath, "Materials");
            var outputName = Path.Combine(materialPath, Path.ChangeExtension(asset.Name, "mat"));

            if (!Directory.Exists(materialPath))
            {
                Directory.CreateDirectory(materialPath);
            }

            asset.ImportedFilename = Path.GetFullPath(outputName);

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
    }
}
