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
using System.Windows;
using System.IO;
using Assets;
using System.Diagnostics;

namespace AssetManager 
{
    public partial class ImportShader : Window
    {
        static readonly int version = 2;
        public static int ImporterVersion { get { return version; } }

        internal ShaderAsset asset = new ShaderAsset();

        public string Error { get; set; }

        internal bool isEditMode = false;

        public ImportShader()
        {
            InitializeComponent();

            this.DataContext = asset;
        }

        internal void setAsset(ShaderAsset newAsset)
        {
            asset = newAsset;
            this.DataContext = asset;
        }

        internal static string import(string input, string output)
        {
            var process = new Process();
            process.StartInfo.FileName = "ShaderImporter.exe"; //Path.Combine(Properties.Settings.Default.ImportersPath, "ShaderImporter.exe");
            process.StartInfo.Arguments = input + " " + output;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.Start();

            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return error;
        }

        internal void Import(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(asset.Name)
                || string.IsNullOrEmpty(asset.SourceFilename)
                || string.IsNullOrEmpty(asset.Description))
            {
                MessageBox.Show("All fields must be filled");
                return;
            }

            //TODO: hardcoded path
            var shadersPath = Path.Combine(Properties.Settings.Default.ImportedAssetsPath, "Shaders");
            var outputName = shadersPath + Path.GetFileNameWithoutExtension(asset.Name);

            if (!Directory.Exists(shadersPath))
            {               
                Directory.CreateDirectory(shadersPath);
            }

            asset.ImportedFilename = outputName;

            if (!isEditMode && File.Exists(shadersPath + asset.ImportedFilename))
            {
                MessageBox.Show("An imported shader with the same name already exists, stopping");
                return;
            }

            var result = import(asset.SourceFilename, asset.ImportedFilename); 

            if (!string.IsNullOrEmpty(result))
            {
                status.Text = result;
                Error = result;
            }
            else
            {
                asset.LastUpdated = DateTime.Now;
                asset.ImporterVersion = ImporterVersion;

                if (this.IsVisible)
                {
                    this.DialogResult = true;
                }
                
                this.Close();                
            }                       
        }

        private void Open(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.InitialDirectory = Path.GetFullPath(Path.Combine(Properties.Settings.Default.RawAssetsPath, "Shaders"));;
            dialog.Filter = "High level shading language (*.hlsl) | *.hlsl";

            var result = dialog.ShowDialog();

            if (result == true)
            {
                asset.SourceFilename = dialog.FileName;
            }
        }

    }
}
