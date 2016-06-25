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
using System.Windows;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Windows.Forms;
using System.ComponentModel;

namespace AssetManager
{
    public partial class SetupProperties : Window, INotifyPropertyChanged
    {
        string importedAssetsPath;
        public string ImportedAssetsPath
        {
            get { return importedAssetsPath; }
            set
            {
                importedAssetsPath = value;
                NotifyPropertyChanged("ImportedAssetsPath");
            }
        }

        string metadataPath;
        public string MetadataPath
        {
            get { return metadataPath; }
            set
            {
                metadataPath = value;
                NotifyPropertyChanged("MetadataPath");
            }
        }

        string rawAssetsPath;
        public string RawAssetsPath
        {
            get { return rawAssetsPath; }
            set
            {
                rawAssetsPath = value;
                NotifyPropertyChanged("RawAssetsPath");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SetupProperties()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        protected void NotifyPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }              

        private void SetImportedAssetsPath(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();

            var result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ImportedAssetsPath = dialog.SelectedPath;
            }
        }

        private void SetMetadataPath(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();

            var result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                MetadataPath = dialog.SelectedPath;
            }
        }

        private void SetRawAssetsPath(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();

            var result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                RawAssetsPath = dialog.SelectedPath;
            }
        }

        private void SaveProperties(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ImportedAssetsPath)
                || string.IsNullOrEmpty(MetadataPath)
                || string.IsNullOrEmpty(RawAssetsPath))
            {
                System.Windows.MessageBox.Show("Some properties were not configured, please fill all the textboxes");
            }
            else
            {
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}
