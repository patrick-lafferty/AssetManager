using Assets;
using Glitch2;
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

                    mesh.LastUpdated = DateTime.Now.ToString();
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

                //TextureImporter.Import(texture);
                var result = ImportTexture.import(texture.SourceFilename, texture.ImportedFilename);

                if (string.IsNullOrEmpty(result))
                {
                    successMessage = "Successfully updated texture: " + texture.Name;
                    texture.LastUpdated = DateTime.Now.ToString();
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
                            
                /*ImportShader import = new ImportShader();
                import.asset = shader;
                import.Import(null, null);
                 */
                var result = ImportShader.import(shader.SourceFilename, shader.ImportedFilename);

                if (string.IsNullOrEmpty(result))
                {
                    successMessage = "Successfully updated shader: " + shader.Name;
                    //shader.LastUpdated
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
                //this will never happen, since materials aren't stored in raw format
            }

            return false;
        }
    }
}
