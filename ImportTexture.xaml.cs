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
using Microsoft.Win32;
using Assets;
using System.Diagnostics;

namespace Glitch2
{
    /// <summary>
    /// Interaction logic for ImportTexture.xaml
    /// </summary>
    public partial class ImportTexture : Window
    {
        internal TextureAsset asset = new TextureAsset();        

        internal bool isEditMode = false;

        public ImportTexture()
        {
            InitializeComponent();

            this.DataContext = asset;
        }

        internal void setAsset(TextureAsset newAsset)
        {
            asset = newAsset;
            this.DataContext = asset;
        }

        int convertBigToLittleEndian(int data)
        {
            var bytes = BitConverter.GetBytes(data);
            Array.Reverse(bytes);

            return BitConverter.ToInt32(bytes, 0);
        }

        string import(string input, string output)
        {
            var process = new Process();
            process.StartInfo.FileName = @"C:\ProjectStacks\Tools\Debug\ImageConverter.exe";
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
                || string.IsNullOrEmpty(asset.Description))
            {
                MessageBox.Show("All fields must be filled");
                return;
            }

            string texturesPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\ImportedAssets\Textures\"));
            texturesPath += new DirectoryInfo(asset.SourceFilename).Parent.Name;
            var outputName = System.IO.Path.Combine(texturesPath, System.IO.Path.ChangeExtension(asset.Name, "dds"));

            if (!Directory.Exists(texturesPath))
            {
                Directory.CreateDirectory(texturesPath);
            }

            asset.ImportedFilename = System.IO.Path.GetFullPath(outputName);

            if (!isEditMode && File.Exists(asset.ImportedFilename))
            {
                MessageBox.Show("An imported texture with the same name already exists, stopping");
                return;
            }

            var result = import(asset.SourceFilename, asset.ImportedFilename);

            if (!string.IsNullOrEmpty(result))
            {
                status.Text = result;                
            }
            else
            {
                asset.LastUpdated = DateTime.Now.ToString();

                using (var stream = File.OpenRead(asset.ImportedFilename))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        reader.BaseStream.Position = 12;

                        asset.Height = reader.ReadInt32().ToString();
                        asset.Width = reader.ReadInt32().ToString();

                        reader.BaseStream.Position = 128;
                        asset.Format = (DXGI_FORMAT)reader.ReadInt32();
                    }
                }

                this.DialogResult = true;
                this.Close();
            }            
        }

        private void Open(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.InitialDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\RawAssets\Textures\"));;
            dialog.Filter = "Portable network graphics (*.png) | *.png";

            var result = dialog.ShowDialog();

            if (result == true)
            {
                asset.SourceFilename = dialog.FileName;
            }
        }
    }
}
