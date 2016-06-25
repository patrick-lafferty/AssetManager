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
