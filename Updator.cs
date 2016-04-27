using Assets;
using Glitch2;
using Importers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManager
{
    public static class Updator
    {
        public static bool UpdateAsset(Object asset,
            out string error, out string successMessage)
        {
            error = "";
            successMessage = "";

            if (asset is MeshAsset)
            {
                MeshAsset mesh = asset as MeshAsset;

                var result = ImportMesh.import(mesh.SourceFilename, mesh.ImportedFilename);

                if (string.IsNullOrEmpty(result))
                {
                    successMessage = "Successfully updated mesh: " + mesh.Name;

                    mesh.LastUpdated = DateTime.Now;//.ToString();
                    AssetMetadata.createMeshMetadata(mesh);

                    return true;
                }
                else
                {
                    //error = "ERROR: Updating mesh: " + mesh.Name + " failed!";
                }
            }
            else if (asset is TextureAsset)
            {
                TextureAsset texture = asset as TextureAsset;

                var result = ImportTexture.import(texture.Format, texture.ChannelMappings, texture.ImportedFilename);

                if (string.IsNullOrEmpty(result))
                {
                    successMessage = "Successfully updated texture: " + texture.Name;
                    texture.LastUpdated = DateTime.Now;//.ToString();
                    AssetMetadata.createTextureMetadata(texture);

                    return true;
                }
                else
                {
                    error = "ERROR: Updating texture: " + texture.Name + " failed!" + Environment.NewLine + result;
                }
            }            
            else if (asset is ShaderAsset)
            {
                ShaderAsset shader = asset as ShaderAsset;
              
                var result = ImportShader.import(shader.SourceFilename, shader.ImportedFilename);

                if (string.IsNullOrEmpty(result))
                {
                    successMessage = "Successfully updated shader: " + shader.Name;
                    shader.LastUpdated = DateTime.Now;//.ToString();
                    AssetMetadata.createShaderMetadata(shader);

                    return true;
                }
                else
                {
                    error = "ERROR: Updating shader: " + shader.Name + " failed!" + Environment.NewLine + result;
                }
            }
            else if (asset is MaterialAsset)
            {
                MaterialAsset material = asset as MaterialAsset;

                var result = MaterialImporter.Import(material);

                if (result)
                {
                    successMessage = "Successfully updated material: " + material.Name;
                    material.LastUpdated = DateTime.Now;//.ToString();
                    AssetMetadata.createMaterialMetadata(material);

                    return true;
                }
                else
                {
                    error = "ERROR: Updating material: " + material.Name + " failed!" + Environment.NewLine;
                }
            }
            else if (asset is StateGroupAsset)
            {
                var stateGroup = asset as StateGroupAsset;

                var result = StateGroupImporter.Import(stateGroup);

                if (result)
                {
                    successMessage = "Successfully updated state group: " + stateGroup.Name;
                    stateGroup.LastUpdated = DateTime.Now;//.ToString();
                    AssetMetadata.createStateGroupMetadata(stateGroup);

                    return true;
                }
                else
                {
                    error = "ERROR: Updating state group: " + stateGroup.Name + " failed!" + Environment.NewLine + result;
                }
            }

            return false;
        }
    }
}
