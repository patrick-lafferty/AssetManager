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
