using System;
using System.ComponentModel;

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

        DateTime lastUpdated;
        public DateTime LastUpdated
        {
            get { return lastUpdated; }
            set
            {
                lastUpdated = value;
                NotifyPropertyChanged("LastUpdated");
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
