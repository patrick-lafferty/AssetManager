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

                //var success = MeshImporter.Import(mesh, out error);
                var success = true;

                if (success)
                {
                    successMessage = "Successfully updated mesh: " + mesh.Name;
                    /*var updateAsset = new ToolEvents.UpdateAssetEvent()
                    {
                        Asset = mesh,
                        AssetType = ToolEvents.AssetType.Mesh
                    };

                    processEvent(updateAsset);*/

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

                successMessage = "Successfully updated texture: " + texture.Name;

                /*var updateAsset = new ToolEvents.UpdateAssetEvent()
                {
                    Asset = texture,
                    AssetType = ToolEvents.AssetType.Texture
                };

                processEvent(updateAsset);*/

                return true;
            }
            /*else if (asset is ScriptAsset)
            {
                ScriptAsset script = asset as ScriptAsset;

                var result = ScriptImporter.Import(script);                

                if (result.Errors.HasErrors)
                {
                    var builder = new StringBuilder();

                    builder.AppendLine("ERROR: Updating script: " + script.Name + " failed!");

                    foreach (var compilerError in result.Errors)
                    {
                        builder.AppendLine(compilerError.ToString());
                    }

                    error = builder.ToString();
                }
                else
                {
                    successMessage = "Successfully updated script: " + script.Name;

                    var updateAsset = new ToolEvents.UpdateAssetEvent()
                    {
                        Asset = script,
                        AssetType = ToolEvents.AssetType.Script
                    };

                    processEvent(updateAsset);
                    return true;
                }
            }*/
            else if (asset is ShaderAsset)
            {
                ShaderAsset shader = asset as ShaderAsset;

                /*var result = ShaderImporter.Import(shader);

                if (result.hasError())
                {
                    error = "ERROR: Updating shader: " + shader.Name + " failed!" + Environment.NewLine + result.Error;
                }
                else
                {
                    successMessage = "Successfully updated shader: " + shader.Name;

                    /*var updateAsset = new ToolEvents.UpdateAssetEvent()
                    {
                        Asset = shader,
                        AssetType = ToolEvents.AssetType.Shader
                    };

                    processEvent(updateAsset);*/
                    //return true;
                //}
                ImportShader import = new ImportShader();
                import.asset = shader;
                import.Import(null, null);

                if (string.IsNullOrEmpty(import.Error))
                {
                    successMessage = "Successfully updated shader: " + shader.Name;
                    return true;
                }
                else
                {
                    error = "ERROR: Updating shader: " + shader.Name + " failed!" + Environment.NewLine + import.Error;
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
