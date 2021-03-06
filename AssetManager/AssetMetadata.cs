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
using Assets;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetManager
{
    static class AssetMetadata
    {
        static string MetadataPath
        {
            get
            {
                return Properties.Settings.Default.MetadataPath;
            }
        }


        static void deleteMetadata(Asset asset, string path)
        {
            if (File.Exists(MetadataPath + path + asset.Name + ".meta"))
            {
                File.Delete(MetadataPath + path + asset.Name + ".meta");
            }
        }

        static internal void createMeshMetadata(MeshAsset mesh)
        {
            var metadata = new StringBuilder();
            metadata.AppendLine("Description= " + mesh.Description)
                .AppendLine("VertexFormat= " + mesh.VertexFormat)
                .AppendLine("LastUpdated= " + mesh.LastUpdated.ToString("o"))
                .AppendLine("SourceFilename= " + mesh.SourceFilename)
                .AppendLine("Topology= " + mesh.Topology.ToString())
                .AppendLine("ImportedFilename= " + mesh.ImportedFilename)
                .AppendLine("ImporterVersion= " + mesh.ImporterVersion);

            File.WriteAllText(MetadataPath + "Meshes/" + mesh.Name + ".meta", metadata.ToString());
        }

        static internal void deleteMeshMetadata(MeshAsset mesh)
        {
            deleteMetadata(mesh, "Meshes/");
        }

        static internal void createTextureMetadata(TextureAsset texture)
        {
            var metadata = new StringBuilder();
            var mappings = texture.ChannelMappings.Aggregate("", (acc, c) => acc + c.Destination.ToString() + "," + c.Filename + "," + c.Source.ToString() + ";");
            mappings = mappings.Remove(mappings.LastIndexOf(';'));
            metadata.AppendLine("Description= " + texture.Description)
                .AppendLine("Width= " + texture.Width)
                .AppendLine("Height= " + texture.Height)
                .AppendLine("Format= " + texture.Format)
                .AppendLine("ChannelMappings=" + mappings)
                .AppendLine("LastUpdated= " + texture.LastUpdated.ToString("o"))
                .AppendLine("SourceFilenames= " + string.Join(",", texture.SourceFilenames))
                .AppendLine("ImportedFilename= " + texture.ImportedFilename)
                .AppendLine("ImporterVersion= " + texture.ImporterVersion);

            File.WriteAllText(MetadataPath + "Textures/" + texture.Name + ".meta", metadata.ToString());
        }

        static internal void deleteTextureMetadata(TextureAsset texture)
        {
            deleteMetadata(texture, "Textures/");
        }

        static internal void createShaderMetadata(ShaderAsset shader)
        {
            var metadata = new StringBuilder();
            metadata.AppendLine("Description= " + shader.Description)
                .AppendLine("Combination= " + shader.Combination.ToString())
                .AppendLine("LastUpdated= " + shader.LastUpdated.ToString("o"))
                .AppendLine("SourceFilename= " + shader.SourceFilename)
                .AppendLine("ImportedFilename= " + shader.ImportedFilename)
                .AppendLine("ImporterVersion= " + shader.ImporterVersion);

            File.WriteAllText(MetadataPath + "Shaders/" + shader.Name + ".meta", metadata.ToString());
        }

        static internal void deleteShaderMetadata(ShaderAsset shader)
        {
            deleteMetadata(shader, "Shaders/");
        }

        static internal void createMaterialMetadata(MaterialAsset material)
        {
            var metadata = new StringBuilder();
            metadata.AppendLine("Description= " + material.Description)
                .AppendLine("LastUpdated= " + material.LastUpdated.ToString("o"))
                .AppendLine("ImportedFilename= " + material.ImportedFilename)
                .AppendLine("ImporterVersion= " + material.ImporterVersion);

            var textures = new StringBuilder();

            //texture format: Textures= slot, sourceId; (texture 2); ...
            foreach(var texture in material.Textures)
            {
                textures.Append(texture.Binding)
                    .Append(",")
                    .Append(texture.SourceId)
                    .Append(";");
            }

            metadata.AppendLine("Textures= " + textures.ToString());

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

        static internal void createStateGroupMetadata(StateGroupAsset stateGroup)
        {
            var metadata = new StringBuilder();
            metadata.AppendLine("Description= " + stateGroup.Description)
                .AppendLine("LastUpdated= " + stateGroup.LastUpdated.ToString("o"))
                .AppendLine("VertexShader= " + stateGroup.VertexShaderId)
                .AppendLine("GeometryShader= " + stateGroup.GeometryShaderId)
                .AppendLine("PixelShader= " + stateGroup.PixelShaderId)
                .AppendLine("ShaderCombination= " + stateGroup.ShaderCombination.ToString())
                .AppendLine("ImportedFilename= " + stateGroup.ImportedFilename)
                .AppendLine("ImporterVersion= " + stateGroup.ImporterVersion);

            var samplers = new StringBuilder();

            //sampler format: Samplers= name, filter, addressU, addressV, addressW; (sampler 2); (sampler 3)
            foreach (var sampler in stateGroup.Samplers)
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

            //texture format: Textures= slot, sourceId; (texture 2); ...
            foreach (var texture in stateGroup.TextureBindings)
            {
                textures.Append(texture.Slot)
                    .Append(",")
                    .Append(texture.Binding)
                    .Append(";");
            }

            metadata.AppendLine("Textures= " + textures.ToString());

            var blendState = new StringBuilder();

            //blendstate format: BlendState= index, enabled, op, opAlpha, srcBlend, dstBlend, srcBlendAlpha, destBlendAlpha, writeMask; ...
            foreach (var renderTarget in stateGroup.BlendState.RenderTargets)
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

            File.WriteAllText(MetadataPath + "stateGroups/" + stateGroup.Name + ".meta", metadata.ToString());
        }

        static internal void deletestateGroupMetadata(StateGroupAsset stateGroup)
        {
            deleteMetadata(stateGroup, "stateGroups/");
        }
    }
}
