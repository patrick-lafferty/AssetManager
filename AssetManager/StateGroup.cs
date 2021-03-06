﻿/*
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
using System.Collections.ObjectModel;

namespace Assets
{
    /*
    Filter, TextureAddressMode, Sampler, BlendOperation, BlendOption, WriteMask,
    RenderTarget, and BlendState correspond to similarly named Direct3D11 things.
    See the DXSDK.
    */
    public enum Filter
    {
        MinMagMipPoint = 0,
        MinMagPointMipLinear = 0x1,
        MinPointMagLinearMipPoint = 0x4,
        MinPointMagMipLinear = 0x5,
        MinLinearMagMipPoint = 0x10,
        MinLinearMagPointMipLinear = 0x11,
        MinMagLinearMipPoint = 0x14,
        MinMagMipLinear = 0x15,
        Anisotropic = 0x55,
        ComparisonMinMagMipPoint = 0x80,
        ComparisonMinMagPointMipLinear = 0x81,
        ComparisonMinPointMagLinearMipPoint = 0x84,
        ComparisonMinPointMagMipLinear = 0x85,
        ComparisonMinLinearMagMipPoint = 0x90,
        ComparisonMinLinearMagPointMipLinear = 0x91,
        ComparisonMinMagLinearMipPoint = 0x94,
        ComparisonMinMagMipLinear = 0x95,
        ComparisonAnisotropic = 0xd5,
        
    }

    public enum TextureAddressMode
    {
        Wrap = 1,
        Mirror,
        Clamp,
        Border,
        MirrorOnce
    }

    public class Sampler
    {
        public string Name { get; set; }
        public Filter Filter { get; set; }
        public TextureAddressMode AddressU { get; set; }
        public TextureAddressMode AddressV { get; set; }
        public TextureAddressMode AddressW { get; set; }
    }

    public enum BlendOperation
    {        
        Add = 1, Subtract, ReverseSubtract, Minimum, Maximum
    }

    public enum BlendOption
    {
        Zero = 1,
        One,
        SourceColor,
        InverseSourceColor,
        SourceAlpha,
        InverseSourceAlpha,
        DestinationAlpha,
        InverseDestinationAlpha,
        DestinationColor,
        InverseDestinationColor,
        SourceAlphaSaturate,
        BlendFactor,
        InverseBlendFactor,
        Source,
        SecondarySourceColor,
        InverseSecondarySourceColor,
        SecondarySourceAlpha,
        InverseSecondarySourceAlpha        
    }

    public enum WriteMask
    {       
        Red = 1,
        Green = 2,
        Blue = 4,
        Alpha = 8,
        All = Red | Green | Blue | Alpha
    }

    public class RenderTarget
    {
        public int Index { get; set; }
        public bool BlendEnabled { get; set; }
        public BlendOperation BlendOperation { get; set; }
        public BlendOperation BlendOperationAlpha { get; set; }
        public BlendOption SourceBlend { get; set; }
        public BlendOption DestinationBlend { get; set; }
        public BlendOption SourceBlendAlpha { get; set; }
        public BlendOption DestinationBlendAlpha { get; set; }
        public WriteMask RenderTargetWriteMask { get; set; }
    }

    public class BlendState
    {
        public BlendState()
        {
            RenderTargets = new ObservableCollection<RenderTarget>();
        }

        public ObservableCollection<RenderTarget> RenderTargets { get; set; }
    }

    public enum DepthWriteMask
    {
        Zero = 0,
        All
    }

    public enum ComparisonFunc
    {
        Never = 1,
        Less,
        Equal,
        LessEqual,
        Greater,
        NotEqual,
        GreaterEqual,
        Always
    }

    public class DepthStencilState
    {
        public DepthStencilState()
        {
            DepthEnable = true;
            DepthWriteMask = DepthWriteMask.All;
            DepthFunc = ComparisonFunc.LessEqual;
        }

        public bool DepthEnable { get; set; }
        public DepthWriteMask DepthWriteMask { get; set; }
        public ComparisonFunc DepthFunc { get; set; }
        /*public bool StencilEnable { get; set; }
        public byte StencilReadMask { get; set; }
        public byte StencilWriteMask { get; set; }*/

    }    

    public class TextureBinding
    {
        public int Slot { get; set; }
        public string Binding { get; set; }
    }

    /*
    StateGroups contain all the bindings necessary to render something.
    */
    public class StateGroupAsset : Asset
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
                
        public StateGroupAsset()
        {
            Samplers = new ObservableCollection<Sampler>();
            TextureBindings = new ObservableCollection<TextureBinding>();
            BlendState = new BlendState();
            DepthStencilState = new DepthStencilState();
        }

        private void updateShaderCombination()
        {
            if (VertexShader != null)
            {
                if (GeometryShader != null)
                {
                    if (PixelShader != null)
                    {
                        ShaderCombination = ShaderCombination.VertexGeometryPixel;
                    }
                    else
                    {
                        ShaderCombination = ShaderCombination.VertexGeometry;
                    }
                }
                else if (PixelShader != null)
                {
                    ShaderCombination = ShaderCombination.VertexPixel;
                }
            }
        }

        public string ImportedFilename { get; set; }

        ShaderAsset vertexShader, geometryShader, pixelShader;

        public ShaderAsset VertexShader
        {
            get
            {
                return vertexShader;
            }
            set
            {
                vertexShader = value;
                VertexShaderId = value.Name;
                updateShaderCombination();
            }
        }

        public string VertexShaderId { get; set; }

        public ShaderAsset GeometryShader
        {
            get
            {
                return geometryShader;
            }
            set
            {
                geometryShader = value;
                GeometryShaderId = value.Name;
                updateShaderCombination();
            }
        }

        public string GeometryShaderId { get; set; }

        public ShaderAsset PixelShader
        {
            get
            {
                return pixelShader;
            }
            set
            {
                pixelShader = value;
                PixelShaderId = value.Name;
                updateShaderCombination();
            }
        }

        public string PixelShaderId { get; set; }

        public ShaderCombination ShaderCombination { get; set; }

        public ObservableCollection<Sampler> Samplers { get; set; }

        public ObservableCollection<TextureBinding> TextureBindings { get; set; }

        public BlendState BlendState { get; set; }

        public DepthStencilState DepthStencilState { get; set; }
    }
}
