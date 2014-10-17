using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets
{
    /*
    A mesh that was created in blender/max/maya whatever, and converted to
    the engine's binary format.
    */
    public enum Topology
    {
        PointList = 1, LineList, LineStrip, TriangleList, TriangleStrip        
    }

    public class MeshAsset : Asset
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

        string vertexFormat;
        public string VertexFormat
        {
            get { return vertexFormat; }
            set
            {
                vertexFormat = value;
                NotifyPropertyChanged("VertexFormat");
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

        Topology topology;
        public Topology Topology
        {
            get { return topology; }
            set
            {
                topology = value;
                NotifyPropertyChanged("Topology");
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
