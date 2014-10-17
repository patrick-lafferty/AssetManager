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

        private void Import(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(asset.Name)
                || string.IsNullOrEmpty(asset.SourceFilename)
                || string.IsNullOrEmpty(asset.Description))
            {
                MessageBox.Show("All fields must be filled");
                return;
            }

            string texturesPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\Assets\Textures\"));
            var outputName = System.IO.Path.Combine(texturesPath, System.IO.Path.ChangeExtension(asset.Name, "png"));

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

            //TextureImporter.Import(asset);

            using (var stream = File.OpenRead(asset.ImportedFilename))
            {
                using (var reader = new BinaryReader(stream))
                {
                    reader.ReadBytes(16);

                    int width = convertBigToLittleEndian(reader.ReadInt32());
                    int height = convertBigToLittleEndian(reader.ReadInt32());

                    asset.Dimensions = width.ToString() + " x " + height.ToString();
                }
            }

            this.DialogResult = true;

            this.Close();
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
