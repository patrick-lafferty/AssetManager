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
using System.Collections.ObjectModel;

namespace Glitch2
{
    /// <summary>
    /// Interaction logic for ImportTexture.xaml
    /// </summary>
    public partial class ImportTexture : Window
    {
        static readonly int version = 1;
        public static int ImporterVersion { get { return version; } }

        public enum Channel
        {
            R, G, B, A
        }

        public class ChannelMapping : Asset
        {
            public Channel Destination { get; set; }
            string filename;
            public string Filename
            {
                get { return filename; }
                set
                {
                    filename = value;

                    NotifyPropertyChanged("Filename");
                }
            }
            public Channel Source { get; set; }
        }

        public TextureAsset asset {get; set;}        

        internal bool isEditMode = false;

        public int ChannelCount {get;set;}
        //public DXGI_FORMAT Format {get; set;}
        public List<DXGI_FORMAT> AvailableFormats {get; set;}
        public List<Channel> AvailableChannels { get; set; }
        public List<int> AvailableChannelCounts { get; set; }
        public ObservableCollection<ChannelMapping> Channels { get; set; }
        public ChannelMapping SelectedChannel { get; set; }
        
        public ImportTexture()
        {
            InitializeComponent();
            asset = new TextureAsset();
            AvailableFormats = new List<DXGI_FORMAT>(typeof(DXGI_FORMAT).GetEnumValues().Cast<DXGI_FORMAT>().ToList());
            AvailableChannels = new List<Channel>(typeof(Channel).GetEnumValues().Cast<Channel>().ToList());
            AvailableChannelCounts = new List<int> { 1, 2, 4 };
            Channels = new ObservableCollection<ChannelMapping>();

            this.DataContext = this;// asset;
        }

        internal void setAsset(TextureAsset newAsset)
        {
            asset = newAsset;
            this.DataContext = this; //asset;
            Channels = new ObservableCollection<ChannelMapping>(asset.ChannelMappings);
        }

        int convertBigToLittleEndian(int data)
        {
            var bytes = BitConverter.GetBytes(data);
            Array.Reverse(bytes);

            return BitConverter.ToInt32(bytes, 0);
        }

        internal static string import(DXGI_FORMAT format, List<ChannelMapping> channels, string output)
        {
            var groupedChannels = channels.GroupBy(c => c.Filename, c => new { Source = c.Source, Destination = c.Destination }, (key, group) => new { Filename = key, Mapping = group });
            var merged = groupedChannels.Select(group =>
            {
                var source = "";
                var destination = "";

                foreach (var mapping in group.Mapping)
                {
                    source += mapping.Source.ToString();
                    destination += mapping.Destination.ToString();
                }

                return source + "," + destination + " " + group.Filename;
            });

            var process = new Process();
            process.StartInfo.FileName = @"C:\ProjectStacks\Tools\Debug\ImageConverter.exe";
            process.StartInfo.Arguments = "DXGI_FORMAT_" + format.ToString() + " " + string.Join(" ", merged) + " " + output;
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
                //|| string.IsNullOrEmpty(asset.SourceFilename)
                //|| asset.SourceFilenames.Count == 0
                //|| asset.SourceFilenames.Any(s => string.IsNullOrEmpty(s))
                || Channels.Count == 0
                || Channels.Any(c => string.IsNullOrEmpty(c.Filename))
                || string.IsNullOrEmpty(asset.Description))
            {
                MessageBox.Show("All fields must be filled");
                return;
            }

            asset.ChannelMappings = Channels.ToList();
            asset.SourceFilenames = asset.ChannelMappings.Select(c => c.Filename).Distinct().Where(s => s != "null").ToList();

            string texturesPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\ImportedAssets\Textures\"));
            texturesPath += new DirectoryInfo(asset.SourceFilenames[0]).Parent.Name;
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

            var result = import(asset.Format, asset.ChannelMappings, asset.ImportedFilename);

            if (!string.IsNullOrEmpty(result))
            {
                status.Text = result;                
            }
            else
            {
                asset.LastUpdated = DateTime.Now.ToString();
                asset.ImporterVersion = ImporterVersion;

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
                //asset.SourceFilename = dialog.FileName;
                SelectedChannel.Filename = dialog.FileName;
            }
        }

        private void ChannelCountChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChannelCount < Channels.Count)
            {
                while(Channels.Count > ChannelCount)
                {
                    Channels.Remove(Channels[Channels.Count - 1]);
                }
            }
            else if (ChannelCount > Channels.Count)
            {
                for (int i = Channels.Count; i < ChannelCount; i++)
                {
                    var channel = new ChannelMapping { Destination = (Channel)i, Filename = "", Source = Channel.R };
                    Channels.Add(channel);
                }
            }
        }
    }
}
