using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Assets
{    
    public class Texture
    {
        public string Binding { get; set; }

        TextureAsset source;        
        public TextureAsset Source
        {
            get
            {
                return source;
            }
            set
            {
                source = value;

                if (source != null)
                    SourceId = source.Name;
            }
        }

        public string SourceId { get; set; }

        public bool IsProcedural { get; set; }
    }

    /*
    Materials are the data used to evaluate a brdf. They contain textures and constant buffer values
    */
    public class MaterialAsset : Asset
    {

        string description;
        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                NotifyPropertyChanged("Description");
            }
        }
     
        public MaterialAsset()
        {
            Textures = new ObservableCollection<Texture>();
            ParameterGroups = new ObservableCollection<ParameterGroup>();
        }
        
        public string ImportedFilename { get; set; }
                
        public ObservableCollection<Texture> Textures {get; set;}

        public ObservableCollection<ParameterGroup> ParameterGroups { get; set; }
    }
}
