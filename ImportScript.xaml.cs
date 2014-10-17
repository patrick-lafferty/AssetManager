using Microsoft.CSharp;
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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using Assets;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;


namespace Glitch2
{
    /// <summary>
    /// Interaction logic for ImportScript.xaml
    /// </summary>
    public partial class ImportScript : Window
    {
        //internal ScriptAsset asset = new ScriptAsset();
        public ScriptAsset asset { get; set; }

        public string Errors { get; set; }

        public ObservableCollection<ScriptAsset> AvailableScripts { get; set; }
        public ScriptAsset SelectedDependencyToAdd { get; set; }
        public ScriptAsset SelectedDependency { get; set; }
        //public List<string> AvailableReferences { get; set; }
        //public string SelectedReference { get; set;}

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

            string scriptsPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\Assets\Scripts\"));
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

            /*var result = ScriptImporter.Import(asset);

            if (!result.Errors.HasErrors)
            {                
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                status.ItemsSource = result.Errors;
                
                var builder = new StringBuilder();
                
                builder.AppendLine("ERROR: Importing script: " + asset.Name + " failed!");

                foreach (var error in result.Errors)
                {
                    builder.AppendLine(error.ToString());
                }

                Errors = builder.ToString();
            }*/
        }

        private void Open(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.InitialDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\RawAssets\Scripts\")); ; ;
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

        /*private void AddReference(object sender, RoutedEventArgs e)
        {
            if (SelectedReference == null)
            {
                return;
            }

            asset.References.Add(SelectedReference);
        }*/

        private void ClearLog(object sender, RoutedEventArgs e)
        {
            status.ItemsSource = null;
        }
        
    }
}
