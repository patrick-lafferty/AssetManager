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
using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.IO;
using Assets;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace AssetManager
{
    public partial class ImportScript : Window
    {
        public ScriptAsset asset { get; set; }

        public string Errors { get; set; }

        public ObservableCollection<ScriptAsset> AvailableScripts { get; set; }
        public ScriptAsset SelectedDependencyToAdd { get; set; }
        public ScriptAsset SelectedDependency { get; set; }

        internal bool isEditMode = false;
        
        public ImportScript()
        {
            asset = new ScriptAsset();
            InitializeComponent();
            this.DataContext = this;
        }

        internal void setAsset(ScriptAsset newAsset)
        {
            asset = newAsset;
            this.DataContext = this;
        }

        private void Import(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(asset.Name)
                || string.IsNullOrEmpty(asset.SourceFilename)
                || string.IsNullOrEmpty(asset.Description))
            {
                MessageBox.Show("All fields must be filled");
                return;
            }

            if (asset.Name.Contains(' '))
            {
                asset.Name = Regex.Replace(asset.Name, @"\s+", "");
            }

            var scriptsPath = System.IO.Path.Combine(Properties.Settings.Default.ImportedAssetsPath, "Scripts");
            var outputName = System.IO.Path.Combine(scriptsPath, System.IO.Path.ChangeExtension(asset.Name, "dll"));

            if (!Directory.Exists(scriptsPath))
            {
                Directory.CreateDirectory(scriptsPath);
            }

            asset.ImportedFilename = System.IO.Path.GetFullPath(outputName);

            if (!isEditMode && File.Exists(asset.ImportedFilename))
            {
                MessageBox.Show("An imported script with the same name already exists, stopping");
                return;
            }
        }

        private void Open(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.InitialDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(Properties.Settings.Default.RawAssetsPath, "Scripts"));
            dialog.Filter = "C# source (*.cs) | *.cs";

            var result = dialog.ShowDialog();

            if (result == true)
            {
                asset.SourceFilename = dialog.FileName;
            }
        }

        private void AddDependency(object sender, RoutedEventArgs e)
        {
            if (SelectedDependencyToAdd == null)
            {
                return;
            }

            asset.Dependencies.Add(SelectedDependencyToAdd);
        }

        private void RemoveDependency(object sender, RoutedEventArgs e)
        {
            if (SelectedDependency == null)
                return;

            asset.Dependencies.Remove(SelectedDependency);
            SelectedDependency = null;
        }

        private void ClearLog(object sender, RoutedEventArgs e)
        {
            status.ItemsSource = null;
        }
        
    }
}
