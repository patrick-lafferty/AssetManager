using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Assets
{
    /*
    Filter, TextureAddressMode, Sampler, BlendOperation, BlendOption, WriteMask,
    RenderTarget, and BlendState correspond to similarly named Direct3D11 things.
    See the DXSDK.
    */
    public enum Filter
    {
        //TODO: these are in wrong order, check d3d docs
        ComparisonAnisotropic, ComparisonMinMagMipLinear, ComparisonMinMagLinearMipPoint,
        ComparisonMinLinearMagPointMipLinear, ComparisonMinLinearMagMipPoint, ComparisonMinPointMagMipLinear,
        ComparisonMinPointMagLinearMipPoint, ComparisonMinMagPointMipLinear, ComparisonMinMagMipPoint,
        Anisotropic, MinMagMipLinear, MinMagLinearMipPoint, MinLinearMagPointMipLinear, MinLinearMagMipPoint,
        MinPointMagMipLinear, MinPointMagLinearMipPoint, MinMagPointMipLinear, MinMagMipPoint
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
        //Add, Maximum, Minimum, Subtract, ReverseSubtract
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
        //
        /*BlendFactor, DestinationAlpha, DestinationColor,
        InverseBlendFactor, InverseDestinationAlpha, InverseDestinationColor, InverseSecondarySourceAlpha,
        InverseSecondarySourceColor, InverseSourceAlpha, InverseSourceColor, One,
        SecondarySourceAlpha, SecondarySourceColor, SourceAlpha, SourceAlphaSaturate,
        SourceColor, Zero*/
    }

    public enum WriteMask
    {
        //All, Alpha, Blue, Green, Red, None
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

        string lastUpdated;
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
