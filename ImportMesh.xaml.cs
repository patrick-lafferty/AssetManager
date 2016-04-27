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
using System.Collections.ObjectModel;
using System.Diagnostics;


namespace Glitch2
{
    /// <summary>
    /// Interaction logic for ImportMesh.xaml
    /// </summary>
    public partial class ImportMesh : Window
    {
        static readonly int version = 4;
        public static int ImporterVersion { get { return version; } }

        internal MeshAsset asset = new MeshAsset();

        public string Error { get; set; }

        public ObservableCollection<Topology> AvailableTopologies { get; set; }        

        internal bool isEditMode = false;

        public ImportMesh()
        {
            InitializeComponent();

            this.DataContext = asset;
        }

        internal void setAsset(MeshAsset newAsset)
        {
            asset = newAsset;
            this.DataContext = asset;
        }

        internal static string import(string input, string output)
        {
            var process = new Process();
            process.StartInfo.FileName = @"C:\ProjectStacks\Tools\Debug\MeshImporter.exe";
            //process.StartInfo.Arguments = asset.SourceFilename + " " + (@"C:\ProjectStacks\ImportedAssets\Meshes\" + Path.GetFileNameWithoutExtension(asset.SourceFilename)) + ".mesh";
            process.StartInfo.Arguments = input + " " + output;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.Start();

            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return error;
        }

        private void Import(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(asset.Name)
                || string.IsNullOrEmpty(asset.SourceFilename)
                || string.IsNullOrEmpty(asset.Description)
                || string.IsNullOrEmpty(asset.VertexFormat)
                || (int)asset.Topology < 1)
            {
                MessageBox.Show("All fields must be filled");
                return;
            }

            string meshesPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\ImportedAssets\Meshes\"));
            var outputName = System.IO.Path.Combine(meshesPath, System.IO.Path.ChangeExtension(asset.Name, "mesh"));

            if (!Directory.Exists(meshesPath))
            {
                Directory.CreateDirectory(meshesPath);
            }

            asset.ImportedFilename = System.IO.Path.GetFullPath(outputName);

            if (!isEditMode && File.Exists(asset.ImportedFilename))
            {
                MessageBox.Show("An imported mesh with the same name already exists, stopping");
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
                asset.LastUpdated = DateTime.Now;//.ToString();
                asset.ImporterVersion = ImporterVersion;

                if (this.IsVisible)
                {
                    this.DialogResult = true;
                }
                
                this.Close();                
            }      
            /*var result = MeshImporter.Import(asset, out error);

            if (result)
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                status.Text = "ERROR: could not import mesh: " + asset.Name
                    + Environment.NewLine + error;
                Error = status.Text;
            }*/
        }

        private void Open(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.InitialDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\RawAssets\Meshes\"));;
            dialog.Filter = "Wavefront (*.obj) | *.obj";
            
            var result = dialog.ShowDialog();

            if (result == true)
            {
                asset.SourceFilename = dialog.FileName;
            }
        }
    }
}
