using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets;
using System.ComponentModel;

namespace Glitch2
{
    internal enum AssetType
    {
        Mesh, Texture, Script, Shader, Material
    }   

    class SP
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    class AssetViewmodel : INotifyPropertyChanged
    {
        internal AssetViewmodel()
        {
            Meshes = new ObservableCollection<MeshAsset>();
            Textures = new ObservableCollection<TextureAsset>();
            Scripts = new ObservableCollection<ScriptAsset>();
            Shaders = new ObservableCollection<ShaderAsset>();
            Materials = new ObservableCollection<MaterialAsset>();
            StateGroups = new ObservableCollection<StateGroupAsset>();
            Fonts = new ObservableCollection<FontAsset>();
            UIs = new ObservableCollection<UIAsset>();

            LogEvent = message => Log += message + Environment.NewLine;
        }

        string log;
        public string Log
        {
            get
            {
                return log;
            }
            set
            {
                log = value;
                NotifyPropertyChanged("Log");
            }
        }

        public MeshAsset SelectedMesh { get; set; }
        public ObservableCollection<MeshAsset> Meshes { get; set; }

        public TextureAsset SelectedTexture { get; set; }
        public ObservableCollection<TextureAsset> Textures { get; set; }

        public ScriptAsset SelectedScript { get; set; }
        public ObservableCollection<ScriptAsset> Scripts { get; set; }

        public ShaderAsset SelectedShader { get; set; }
        public ObservableCollection<ShaderAsset> Shaders { get; set; }

        public MaterialAsset SelectedMaterial { get; set; }
        public ObservableCollection<MaterialAsset> Materials { get; set; }

        public StateGroupAsset SelectedStateGroup { get; set; }
        public ObservableCollection<StateGroupAsset> StateGroups { get; set; }

        public FontAsset SelectedFont { get; set; }
        public ObservableCollection<FontAsset> Fonts { get; set; }

        public UIAsset SelectedUI { get; set; }
        public ObservableCollection<UIAsset> UIs { get; set; }

        internal bool TryGetAsset(string name, out Object asset)
        {                
            asset = Meshes.FirstOrDefault(m => m.SourceFilename == name);

            if (asset != null)
            {
                return true;
            }
            else
            {
                asset = Textures.FirstOrDefault(t => t.SourceFilenames.Contains(name));

                if (asset != null)
                {
                    return true;
                }
                else
                {
                    asset = Scripts.FirstOrDefault(s => s.SourceFilename == name);

                    if (asset != null)
                    {
                        return true;
                    }
                    else
                    {
                        asset = Shaders.FirstOrDefault(s => s.SourceFilename == name);

                        if (asset != null)
                        {
                            return true;
                        }                        
                    }
                }
            }

            return false;
        }

        public Action<string> LogEvent { get; private set; }

        internal Dictionary<string, List<ScriptAsset>> ScriptDependencies = new Dictionary<string, List<ScriptAsset>>();

        public event PropertyChangedEventHandler PropertyChanged;

        void NotifyPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}
