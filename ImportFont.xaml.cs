using Assets;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Drawing;
using System.IO;

namespace Glitch2
{
    /// <summary>
    /// Interaction logic for ImportFont.xaml
    /// </summary>
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

            var fontsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\Assets\Fonts\");
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

            /*var importer = new FontImporter(updateStatusMessage, updateProgressBar, setProgressBarValue, setProgressMaximum);
            var result = await Task.Factory.StartNew(() => importer.Import(asset));

            if (result)
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                
            }

            importInProgress = false;*/
        }
    }
}
