using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets
{
    /*
    There are only a few valid shader combinations for a material.
    
    VertexPixel - used for rendering meshes/post processing
    VertexGeometryPixel - same as above, with the addition of 
        geometry manipulation done at face level
    VertexGeometry - geometry/general data is streamed out by
        the geometry shader into a buffer

    You cannot have a material with a combination not listed here.
    Its not possible to draw without a vertex shader, or 
    with a vertex shader but without a pixel shader, or 
    just a geometry shader.

    */
    public enum ShaderCombination
    {
        VertexPixel, VertexGeometryPixel, VertexGeometry, Invalid
    }

    /*
    A compiled hlsl file. All shaders present in the sourcecode
    gets compiled into one object file. Shaders must use a common
    naming convention: main_vertex, main_geometry, main_pixel.
    */
    public class ShaderAsset : Asset
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

        ShaderCombination combination;
        public ShaderCombination Combination
        {
            get { return combination; }
            set
            {
                combination = value;
                NotifyPropertyChanged("Combination");
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

        public string ImportedFilename { get; set; }       
    }
}
