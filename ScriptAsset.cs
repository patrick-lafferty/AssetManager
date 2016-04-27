using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets
{
    /*
    A C# script that has been imported into the database.
    Its not actually storing the compiled result (yet),
    it just compiles to see if the script has any compile
    errors or not. The engine still loads the source code
    and compiles at runtime.
    */
    public class ScriptAsset : Asset
    {        
        public ScriptAsset()
        {
            Dependencies = new ObservableCollection<ScriptAsset>();
        }

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

        /*string lastUpdated;
        public string LastUpdated
        {
            get
            {
                return lastUpdated;
            }
            set
            {
                lastUpdated = value;
                NotifyPropertyChanged("LastUpdated");
            }
        }*/

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

        //the partial scripts that this script requires
        public ObservableCollection<ScriptAsset> Dependencies { get; set; }

        public string ImportedFilename { get; set; }      
        
    }
}
