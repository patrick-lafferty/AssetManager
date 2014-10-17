using Assets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManager
{
    static class AssetMetadata
    {
        static string MetadataPath = @"C:\ProjectStacks\RawAssets\Metadata\";

        static internal void createShaderMetadata(ShaderAsset shader)
        {
            var metadata = new StringBuilder();
            metadata.AppendLine("Description= " + shader.Description)
                .AppendLine("Combination= " + shader.Combination.ToString())
                .AppendLine("LastUpdated= " + shader.LastUpdated)
                .AppendLine("SourceFilename= " + shader.SourceFilename)
                .AppendLine("ImportedFilename= " + shader.ImportedFilename);

            File.WriteAllText(MetadataPath + "Shaders/" + shader.Name + ".meta", metadata.ToString());
        }

        static void deleteMetadata(Asset asset, string path)
        {
            if (File.Exists(MetadataPath + path + asset.Name + ".meta"))
            {
                File.Delete(MetadataPath + path + asset.Name + ".meta");
            }
        }

        static internal void deleteShaderMetadata(ShaderAsset shader)
        {
            deleteMetadata(shader, "Shaders/");
        }

        static internal void createMaterialMetadata(MaterialAsset material)
        {
            var metadata = new StringBuilder();
            metadata.AppendLine("Description= " + material.Description)
                .AppendLine("LastUpdated= " + material.LastUpdated)
                .AppendLine("VertexShader= " + material.VertexShaderId)
                .AppendLine("GeometryShader= " + material.GeometryShaderId)
                .AppendLine("PixelShader= " + material.PixelShaderId)
                .AppendLine("ShaderCombination= " + material.ShaderCombination.ToString())
                .AppendLine("ImportedFilename= " + material.ImportedFilename);

            var samplers = new StringBuilder();

            //sampler format: Samplers= name, filter, addressU, addressV, addressW; (sampler 2); (sampler 3)
            foreach(var sampler in material.Samplers)
            {
                samplers.Append(sampler.Name)
                    .Append(",")
                    .Append(sampler.Filter.ToString())
                    .Append(",")
                    .Append(sampler.AddressU.ToString())
                    .Append(",")
                    .Append(sampler.AddressV.ToString())
                    .Append(",")
                    .Append(sampler.AddressW.ToString())
                    .Append(";");
            }

            metadata.AppendLine("Samplers= " + samplers.ToString());

            var textures = new StringBuilder();

            //texture format: Textures= srvName, sourceId; (texture 2); ...
            foreach(var texture in material.Textures)
            {
                textures.Append(texture.ShaderResourceViewName)
                    .Append(",")
                    .Append(texture.SourceId)
                    .Append(";");
            }

            metadata.AppendLine("Textures= " + textures.ToString());

            var blendState = new StringBuilder();

            //blendstate format: BlendState= index, enabled, op, opAlpha, srcBlend, dstBlend, srcBlendAlpha, destBlendAlpha, writeMask; ...
            foreach(var renderTarget in material.BlendState.RenderTargets)
            {
                blendState.Append(renderTarget.Index)
                    .Append(",")
                    .Append(renderTarget.BlendEnabled)
                    .Append(",")
                    .Append(renderTarget.BlendOperation.ToString())
                    .Append(",")
                    .Append(renderTarget.BlendOperationAlpha.ToString())
                    .Append(",")
                    .Append(renderTarget.SourceBlend.ToString())
                    .Append(",")
                    .Append(renderTarget.DestinationBlend.ToString())
                    .Append(",")
                    .Append(renderTarget.SourceBlendAlpha.ToString())
                    .Append(",")
                    .Append(renderTarget.DestinationBlendAlpha.ToString())
                    .Append(",")
                    .Append(renderTarget.RenderTargetWriteMask.ToString())
                    .Append(";");
            }

            metadata.AppendLine("BlendState= " + blendState.ToString());

            //group format: name# parameters
            foreach(var parameterGroup in material.ParameterGroups)
            {
                var group = new StringBuilder();

                group.Append(parameterGroup.Name) 
                    .Append("#"); 

                //parameter format: name, type, value
                //note: vector elements separated by *
                //note: array elements separated by $
                foreach(var parameter in parameterGroup.Parameters)
                {
                    group.Append(parameter.Name)
                        .Append(",")
                        .Append(parameter.Type)
                        .Append(",");
            
                    parameter.WriteoutValue(group);        
                   
                    group.Append(";");
                }

                metadata.AppendLine("ParameterGroup= " + group.ToString());
            }

            File.WriteAllText(MetadataPath + "Materials/" + material.Name + ".meta", metadata.ToString());
        }

        static internal void deleteMaterialMetadata(MaterialAsset material)
        {
            deleteMetadata(material, "Materials/");
        }
    }
}
