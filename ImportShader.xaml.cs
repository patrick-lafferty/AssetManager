using Microsoft.Win32;
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
using System.IO;
using Assets;
using System.Diagnostics;

namespace Glitch2
{
    /// <summary>
    /// Interaction logic for ImportShader.xaml
    /// </summary>
    public partial class ImportShader : Window
    {
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

        string import(string source)
        {
            var process = new Process();
            process.StartInfo.FileName = @"C:\ProjectStacks\Tools\Debug\ShaderImporter.exe";            
            process.StartInfo.Arguments = asset.SourceFilename + " " + (@"C:\ProjectStacks\ImportedAssets\Shaders\" + Path.GetFileNameWithoutExtension(asset.SourceFilename));
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

            //string shadersPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\Assets\Shaders\"));
            string shadersPath = @"C:\ProjectStacks\ImportedAssets\Shaders\";
            //var outputName = System.IO.Path.Combine(shadersPath, System.IO.Path.ChangeExtension(asset.Name, ".csg"));
            var outputName = /*shadersPath + */Path.GetFileNameWithoutExtension(asset.Name);

            if (!Directory.Exists(shadersPath))
            {               
                Directory.CreateDirectory(shadersPath);
            }

            asset.ImportedFilename = outputName;//System.IO.Path.GetFullPath(outputName);

            if (!isEditMode && File.Exists(shadersPath + asset.ImportedFilename))
            {
                MessageBox.Show("An imported shader with the same name already exists, stopping");
                return;
            }

            var result = import(asset.ImportedFilename); //ShaderImporter.Import(asset);

            if (!string.IsNullOrEmpty(result))
            {
                status.Text = result;
                Error = result;
            }
            else
            {
                asset.LastUpdated = DateTime.Now.ToString();

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
            dialog.InitialDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\RawAssets\Shaders\"));;
            dialog.Filter = "High level shading language (*.hlsl) | *.hlsl";

            var result = dialog.ShowDialog();

            if (result == true)
            {
                asset.SourceFilename = dialog.FileName;
            }
        }

    }
}
