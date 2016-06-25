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
using Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Drawing;
using System.IO;

namespace AssetManager
{
    public partial class ImportFont : Window
    {
        internal FontAsset asset = new FontAsset();

        public List<Font> AvailableFonts { get; set; }

        Action<string> updateStatusMessage;
        Action<int> updateProgressBar;
        Action<int> setProgressBarValue;
        Action<int> setProgressMaximum;

        internal bool isEditMode = false;

        public ImportFont()
        {
            InitializeComponent();

            AvailableFonts =
                    System.Drawing.FontFamily.Families
                    .Where(family => family.IsStyleAvailable(System.Drawing.FontStyle.Regular))
                    .Select(family => new Font(family.Name, 150)).ToList();

            this.DataContext = asset;

            Action<Action> invoke = f => this.Dispatcher.Invoke(f);
            updateStatusMessage = message => invoke(() => generationStatus.Text = message);
            updateProgressBar = amount => invoke(() => progress.Value += amount);
            setProgressBarValue = value => invoke(() => progress.Value = value);
            setProgressMaximum = max => invoke(() => progress.Maximum = max);
        }

        internal void setAsset(FontAsset newAsset)
        {
            asset = newAsset;
            this.DataContext = asset;
        }

        bool importInProgress = false;

        private void Import(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(asset.Name)
                || asset.FontName == null
                || string.IsNullOrEmpty(asset.Description))
            {
                MessageBox.Show("All fields must be filed");
                return;
            }

            if (importInProgress)
            {
                MessageBox.Show("Import in progress, stop trying to break things");
                return;
            }

            importInProgress = true;

            var fontsPath = System.IO.Path.Combine(Properties.Settings.Default.ImportedAssetsPath, "Fonts");
            var outputName = System.IO.Path.Combine(fontsPath, System.IO.Path.ChangeExtension(asset.Name, "font"));

            if (!Directory.Exists(fontsPath))
            {
                Directory.CreateDirectory(fontsPath);
            }

            asset.ImportedFilename = System.IO.Path.GetFullPath(outputName);

            if (!isEditMode && File.Exists(asset.ImportedFilename))
            {
                MessageBox.Show("An imported font with the same name already exists, stopping");
                return;
            }

        }
    }
}
