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

    [stgp - 83 84 71 80 - 4 bytes]
    [format version - 4 bytes]

    textures:
    [count - 1 byte]
    [texture slot - 4 bytes]
    [texture binding - x bytes]

    isTransparent: [4 bytes]
      
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
    */

    public class StateGroupImporter
    {
        static readonly int version = 3;

        public static int ImporterVersion { get { return version; } }

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

        public static bool Import(StateGroupAsset asset)
        {
            using (var stream = File.Open(asset.ImportedFilename, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write((byte)83);
                    writer.Write((byte)84);
                    writer.Write((byte)71);
                    writer.Write((byte)80);

                    writer.Write(version);

                    writer.Write((int)asset.TextureBindings.Count);

                    foreach (var binding in asset.TextureBindings)
                    {
                        writer.Write((int)binding.Slot);
                        writeString(writer, binding.Binding);
                    }

                    var isTransparent = asset.BlendState.RenderTargets.Any(r => r.BlendEnabled);

                    writer.Write((bool)isTransparent);

                    if (asset.ShaderCombination == ShaderCombination.VertexPixel)
                    {
                        writer.Write((int)0);

                        writeString(writer, Path.GetFileName(asset.VertexShader.ImportedFilename) + ".cvs");
                        writeString(writer, Path.GetFileName(asset.PixelShader.ImportedFilename) + ".cps");

                    }
                    else if (asset.ShaderCombination == ShaderCombination.VertexGeometryPixel)
                    {
                        writer.Write((int)1);
                        
                        writeString(writer, asset.VertexShader.ImportedFilename + ".cvs");
                        writeString(writer, asset.GeometryShader.ImportedFilename + ".cgs");
                        writeString(writer, asset.PixelShader.ImportedFilename + ".cps");
                    }
                    else if (asset.ShaderCombination == ShaderCombination.VertexGeometry)
                    {
                        writer.Write((int)2);
                        //writer.Write(asset.VertexShader.ImportedFilename);
                        //writer.Write(asset.GeometryShader.ImportedFilename);                        
                        writeString(writer, asset.VertexShader.ImportedFilename + ".cvs");
                        writeString(writer, asset.GeometryShader.ImportedFilename + ".cgs");
                    }                                        

                    writer.Write((int)asset.Samplers.Count);

                    foreach (var sampler in asset.Samplers)
                    {
                        //writer.Write(sampler.Name);
                        writeString(writer, sampler.Name);
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
                }
            }

            asset.LastUpdated = DateTime.Now;//.ToString();
            asset.ImporterVersion = ImporterVersion;

            return true;
        }
    }
}
