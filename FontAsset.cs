namespace Assets
{
    public class FontAsset : Asset
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

        public string FontName { get; set; }

        public string ImportedFilename { get; set; }
        
    }
}
