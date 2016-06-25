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
using Assets;
using AssetManager;
using Importers;
using System;

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

                    mesh.LastUpdated = DateTime.Now;
                    AssetMetadata.createMeshMetadata(mesh);

                    return true;
                }
            }
            else if (asset is TextureAsset)
            {
                TextureAsset texture = asset as TextureAsset;

                var result = ImportTexture.import(texture.Format, texture.ChannelMappings, texture.ImportedFilename);

                if (string.IsNullOrEmpty(result))
                {
                    successMessage = "Successfully updated texture: " + texture.Name;
                    texture.LastUpdated = DateTime.Now;
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
                    shader.LastUpdated = DateTime.Now;
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
                    material.LastUpdated = DateTime.Now;
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
                    stateGroup.LastUpdated = DateTime.Now;
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
