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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Assets;
using System.ComponentModel;

namespace AssetManager
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
