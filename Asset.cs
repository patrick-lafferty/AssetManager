using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Assets
{    
    public class Asset : INotifyPropertyChanged
    {      
        string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged("Name");
            }
        }

        int importerVersion;
        public int ImporterVersion
        {
            get { return importerVersion; }
            set
            {
                importerVersion = value;
                NotifyPropertyChanged("ImporterVersion");
            }
        }

        DateTime lastUpdateDate;
        public DateTime LastUpdateDate
        {
            get { return lastUpdateDate; }
            set
            {
                lastUpdateDate = value;
                NotifyPropertyChanged("LastUpdateDate");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}
