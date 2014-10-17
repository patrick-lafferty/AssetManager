using Assets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Importers
{
    /*
    imported format:

    [mate - 77 65 84 69 - 4 bytes]
    [format version - 4 bytes]

    shaders:
    [combination - 1 byte] {0 = vertex/pixel, 1 = vertex/geometry/pixel, 2 = vertex/geometry}
    [string filename - x bytes?]*

    samplers:
    [count - 1 byte]

    [string name - x bytes]
    [filter - 1 byte]
    [addressU - 1 byte]
    [addressV - 1 byte]
    [addressW - 1 byte]


    blend state:         indented because theres other properties of blend state besides render target, not yet implemented
        [render target count - 1 byte]

        render targets:
        [blendEnabled - 1 byte]
        [blendOperation - 1 byte]
        [blendOperationAlpha - 1 byte]
        [sourceBlend - 1 byte]
        [destinationBlend - 1 byte]
        [sourceBlendAlpha - 1 byte]
        [destinationBlendAlpha - 1 byte]
        [renderTargetWriteMask - 1 byte]
        
    textures:
    [count - 1 byte]
    [string textureName - x bytes]
    [string destinationName - x bytes]
    [real or generated - 1 byte]          is this texture an offline asset, or one generated at runtime

    default parameter values:
    [parameter group count - 1 byte]

    parameter group:
		[string name - x bytes]
		[parameter count - 1 byte]

		parameter:
			[string name - x bytes]
			[type - 1 byte]  {0 = float, 1 = int, 2 = bool}
			[dimension - 1 byte] { 0 = scalar, 1 = vector, 2 = array}
			
			scalar?
				[value - 4 bytes]

			vector?
				[values - 16 bytes]

			array?

				[element count - 2 bytes]
				[elements - scalar: 4 bytes, vector: 16 bytes, array: 16 bytes * count]
    
    */

    public class MaterialImporter
    {
        const int version = 1;

        static char[] getAsciiString(string s)
        {
            Encoding ascii = Encoding.ASCII;
            Encoding unicode = Encoding.Unicode;

            byte[] unicodeBytes = unicode.GetBytes(s);

            byte[] asciiBytes = Encoding.Convert(unicode, ascii, unicodeBytes);

            char[] asciiChars = new char[ascii.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
            ascii.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);

            return asciiChars;
        }

        static void writeString(BinaryWriter writer, string s)
        {
            var ascii = getAsciiString(s);

            writer.Write(s.Length);
            writer.Write(ascii);
        }

        public static bool Import(MaterialAsset asset)
        {
            using (var stream = File.Open(asset.ImportedFilename, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write((byte)77);
                    writer.Write((byte)65);
                    writer.Write((byte)84);
                    writer.Write((byte)69);

                    writer.Write(version);

                    if (asset.ShaderCombination == ShaderCombination.VertexPixel)
                    {
                        writer.Write((int)0);

                        writeString(writer, asset.VertexShader.ImportedFilename + ".cvs");
                        writeString(writer, asset.PixelShader.ImportedFilename + ".cps");

                        //writer.Write(asset.VertexShader.ImportedFilename);
                        //writer.Write(asset.PixelShader.ImportedFilename);
                    }
                    else if (asset.ShaderCombination == ShaderCombination.VertexGeometryPixel)
                    {
                        writer.Write((int)1);
                        writer.Write(asset.VertexShader.ImportedFilename);
                        writer.Write(asset.GeometryShader.ImportedFilename);
                        writer.Write(asset.PixelShader.ImportedFilename);
                    }
                    else if (asset.ShaderCombination == ShaderCombination.VertexGeometry)
                    {
                        writer.Write((int)2);
                        writer.Write(asset.VertexShader.ImportedFilename);
                        writer.Write(asset.GeometryShader.ImportedFilename);                        
                    }

                    writer.Write((int)asset.Samplers.Count);

                    foreach (var sampler in asset.Samplers)
                    {
                        writer.Write(sampler.Name);
                        writer.Write((int)sampler.Filter);
                        writer.Write((int)sampler.AddressU);
                        writer.Write((int)sampler.AddressV);
                        writer.Write((int)sampler.AddressW);
                    }

                    writer.Write((int)asset.BlendState.RenderTargets.Count);

                    foreach (var renderTarget in asset.BlendState.RenderTargets)
                    {
                        writer.Write((int)Convert.ToInt32(renderTarget.BlendEnabled));
                        writer.Write((int)renderTarget.BlendOperation);
                        writer.Write((int)renderTarget.BlendOperationAlpha);
                        writer.Write((int)renderTarget.SourceBlend);
                        writer.Write((int)renderTarget.DestinationBlend);
                        writer.Write((int)renderTarget.SourceBlendAlpha);
                        writer.Write((int)renderTarget.DestinationBlendAlpha);
                        writer.Write((int)renderTarget.RenderTargetWriteMask);
                    }

                    writer.Write((int)Convert.ToInt32(asset.DepthStencilState.DepthEnable));
                    writer.Write((int)asset.DepthStencilState.DepthWriteMask);
                    writer.Write((int)asset.DepthStencilState.DepthFunc);

                    writer.Write((byte)asset.Textures.Count);

                    foreach (var texture in asset.Textures)
                    {
                        writer.Write(texture.ShaderResourceViewName);
                        writer.Write(texture.Source.ImportedFilename);
                        writer.Write((byte)0); //todo: NOT IMPLEMENTED YET
                    }

                    writer.Write((byte)asset.ParameterGroups.Count);

                    foreach (var group in asset.ParameterGroups)
                    {
                        writer.Write(group.Name);
                        writer.Write((byte)group.Parameters.Count);

                        foreach (var parameter in group.Parameters)
                        {
                            writer.Write(parameter.Name);
                            writer.Write((byte)parameter.PodType);
                            writer.Write((byte)parameter.ParameterType);
                            parameter.WriteoutValue(writer);
                            
                        }
                    }
                }
            }

            asset.LastUpdated = DateTime.Now.ToString();

            return true;
        }
    }
}
