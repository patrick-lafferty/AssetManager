using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets
{
    /*
    Its just a texture, nothing to say. Its not converted, just plain
    copied.
    */
    public class TextureAsset : Asset
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

        string dimensions;
        public string Dimensions
        {
            get { return dimensions; }
            set
            {
                dimensions = value;
                NotifyPropertyChanged("Dimensions");
            }
        }

        string lastUpdated;
        public string LastUpdated
        {
            get { return lastUpdated; }
            set
            {
                lastUpdated = value;
                NotifyPropertyChanged("LastUpdated");
            }
        }

        string filename;
        public string SourceFilename
        {
            get
            {
                return filename;
            }
            set
            {
                filename = value;
                NotifyPropertyChanged("SourceFilename");
            }
        }

        string importedFilename;
        public string ImportedFilename
        {
            get { return importedFilename; }
            set
            {
                importedFilename = value;
                NotifyPropertyChanged("ImportedFilename");
            }
        }        
    }
}
