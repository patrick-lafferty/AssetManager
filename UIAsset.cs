using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets
{    
    public class UIAsset : Asset
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

        public string ImportedFilename { get; set; }

        public string DefaultMesh { get; set; }
        public string DefaultMaterial { get; set; }
        public string DefaultFont { get; set; }

        
    }
}
