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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Importers;

namespace AssetManager
{
    internal class MetadataHandler
    {
        string metadataPath
        {
            get { return Properties.Settings.Default.MetadataPath; }
        }

        RawAssetWatcher assetWatcher;
        AssetViewmodel viewmodel;

        Action<string> displayError;

        internal MetadataHandler(Action<Action> invoke, AssetViewmodel viewmodel)
        {
            this.viewmodel = viewmodel;
            
            displayError = error => invoke(() => viewmodel.LogEvent(error));

            assetWatcher = new RawAssetWatcher(viewmodel,
                error =>
                {
                    invoke(() =>
                    {
                        viewmodel.LogEvent(error);
                    });
                },
                success =>
                {
                    invoke(() => viewmodel.LogEvent(success));
                },
                asset =>
                {
                    invoke(() => updateDependents(asset));
                }
            );
        }

        internal void stopWatchingAssets()
        {
            assetWatcher.stopWatching();
        }
        
        DateTime parseLastUpdatedDate(string lastUpdatedString, string assetType, string assetName)
        {
            DateTime lastUpdatedDate = new DateTime();

            Func<string, DateTime?> parseWeirdFormatDate =
               date =>
               {
                   var splitSpaces = date.Split(' ');
                   var splitSlashes = splitSpaces[0].Split('/');
                   var fixedDate = string.Format("{0}-{1}-{2} {3} {4}", splitSlashes[2], splitSlashes[1], splitSlashes[0], splitSpaces[1], splitSpaces[2]);

                   DateTime parsedDate;

                   if (!DateTime.TryParse(fixedDate, out parsedDate))
                   {
                       return null;
                   }
                   else
                   {
                       return parsedDate;
                   }
               };

            if (lastUpdatedString.Contains("/") || !DateTime.TryParse(lastUpdatedString, null, System.Globalization.DateTimeStyles.RoundtripKind, out lastUpdatedDate))
            {
                var date = parseWeirdFormatDate(lastUpdatedString);
                if (date.HasValue)
                {
                    lastUpdatedDate = date.Value;
                }
                else
                {
                    displayError("Unable to parse " + assetType + ".LastUpdated for " + assetName);
                }
            }

            return lastUpdatedDate;
        }

        //see ToolEvents\AssetMetadata.fs for metadata file format
        List<MeshAsset> loadMeshes()
        {            
            var meshes = new List<MeshAsset>();

            if (!Directory.Exists(metadataPath + "Meshes"))
            {
                Directory.CreateDirectory(metadataPath + "Meshes");
            }

            foreach(var filename in Directory.EnumerateFiles(metadataPath + "Meshes"))
            {
                var metadata = File.ReadAllLines(filename);

                var mesh = new MeshAsset();

                mesh.Name = System.IO.Path.GetFileNameWithoutExtension(filename);
                mesh.Description = metadata[0].Split('=')[1].Trim();
                mesh.VertexFormat = metadata[1].Split('=')[1].Trim();

                mesh.LastUpdated = parseLastUpdatedDate(metadata[2].Split('=')[1].Trim(), "mesh", mesh.Name);
                mesh.SourceFilename = metadata[3].Split('=')[1].Trim();
                mesh.Topology = (Topology)Enum.Parse(typeof(Topology), metadata[4].Split('=')[1].Trim());
                mesh.ImportedFilename = metadata[5].Split('=')[1].Trim();
                mesh.ImporterVersion = int.Parse(metadata[6].Split('=')[1].Trim());

                meshes.Add(mesh);
            }

            return meshes;
        }

        List<TextureAsset> loadTextures()
        {
            var textures = new List<TextureAsset>();

            if (!Directory.Exists(metadataPath + "Textures"))
            {
                Directory.CreateDirectory(metadataPath + "Textures");
            }

            foreach (var filename in Directory.EnumerateFiles(metadataPath + "Textures"))
            {
                var metadata = File.ReadAllLines(filename);

                var texture = new TextureAsset();

                texture.Name = System.IO.Path.GetFileNameWithoutExtension(filename);
                texture.Description = metadata[0].Split('=')[1].Trim();
                texture.Width = metadata[1].Split('=')[1].Trim();
                texture.Height = metadata[2].Split('=')[1].Trim();
                texture.Format = (DXGI_FORMAT)Enum.Parse(typeof(DXGI_FORMAT), metadata[3].Split('=')[1].Trim());
                texture.ChannelMappings =
                    metadata[4].Split('=')[1]
                    .Split(';')                    
                    .Select(s =>
                    {
                        var c = s.Split(',');

                        return new ImportTexture.ChannelMapping()
                        {
                            Destination = (ImportTexture.Channel)Enum.Parse(typeof(ImportTexture.Channel), c[0]),
                            Filename = c[1],
                            Source = (ImportTexture.Channel)Enum.Parse(typeof(ImportTexture.Channel), c[2])
                        };
                    }).ToList();
                texture.LastUpdated = parseLastUpdatedDate(metadata[5].Split('=')[1].Trim(), "texture", texture.Name);
                texture.SourceFilenames = metadata[6].Split('=')[1].Trim().Split(',').ToList();                
                texture.ImportedFilename = metadata[7].Split('=')[1].Trim();
                texture.ImporterVersion = int.Parse(metadata[8].Split('=')[1].Trim());

                textures.Add(texture);
            }

            return textures;
        }

        Dictionary<ScriptAsset, string[]> loadScripts()
        {
            var scriptsAndDependencies = new Dictionary<ScriptAsset, string[]>();

            if (!Directory.Exists(metadataPath + "Scripts"))
            {
                Directory.CreateDirectory(metadataPath + "Scripts");
            }

            foreach (var filename in Directory.EnumerateFiles(metadataPath + "Scripts"))
            {
                var metadata = File.ReadAllLines(filename);

                var script = new ScriptAsset();

                script.Name = System.IO.Path.GetFileNameWithoutExtension(filename);
                script.Description = metadata[0].Split('=')[1].Trim();
                script.LastUpdated = parseLastUpdatedDate(metadata[1].Split('=')[1].Trim(), "script", script.Name);
                script.SourceFilename = metadata[2].Split('=')[1].Trim();

                var dependencies = metadata[3].Split('=')[1].Trim().Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries);
                
                script.ImportedFilename = metadata[4].Split('=')[1].Trim();

                scriptsAndDependencies.Add(script, dependencies);
            }

            return scriptsAndDependencies;
        }

        List<ShaderAsset> loadShaders()
        {
            var shaders = new List<ShaderAsset>();

            if (!Directory.Exists(metadataPath + "Shaders"))
            {
                Directory.CreateDirectory(metadataPath + "Shaders");
            }

            foreach (var filename in Directory.EnumerateFiles(metadataPath + "Shaders"))
            {
                var metadata = File.ReadAllLines(filename);

                var shader = new ShaderAsset();

                shader.Name = System.IO.Path.GetFileNameWithoutExtension(filename);
                shader.Description = metadata[0].Split('=')[1].Trim();
                shader.Combination = (ShaderCombination)Enum.Parse(typeof(ShaderCombination), metadata[1].Split('=')[1].Trim());
                shader.LastUpdated = parseLastUpdatedDate(metadata[2].Split('=')[1].Trim(), "shader", shader.Name);
                shader.SourceFilename = metadata[3].Split('=')[1].Trim();
                shader.ImportedFilename = metadata[4].Split('=')[1].Trim();
                shader.ImporterVersion = int.Parse(metadata[5].Split('=')[1].Trim());

                shaders.Add(shader);
            }

            return shaders;
        }
        
        object parseValue<T>(string data)
        {
            if (typeof(T) == typeof(float))
            {
                return float.Parse(data);
            }
            else if (typeof(T) == typeof(int))
            {
                return int.Parse(data);
            }            
            else if (typeof(T) == typeof(bool))
            {
                return bool.Parse(data);
            }
            else 
            {
                return null;
            }
        }

        Vector2Parameter<T> parseVector2<T>(string name, string data)
        {
            var values = data.Trim().Split('*');
            var x = parseValue<T>(values[0]);
            var y = parseValue<T>(values[1]);

            return new Vector2Parameter<T>() { Name = name, X = (T)x, Y = (T)y };
        }

        Vector3Parameter<T> parseVector3<T>(string name, string data)
        {
            var values = data.Trim().Split('*');
            var x = parseValue<T>(values[0]);
            var y = parseValue<T>(values[1]);
            var z = parseValue<T>(values[2]);

            return new Vector3Parameter<T>() { Name = name, X = (T)x, Y = (T)y, Z = (T)z};
        }

        Vector4Parameter<T> parseVector4<T>(string name, string data)
        {
            var values = data.Trim().Split('*');
            var x = parseValue<T>(values[0]);
            var y = parseValue<T>(values[1]);
            var z = parseValue<T>(values[2]);
            var w = parseValue<T>(values[3]);

            return new Vector4Parameter<T>() { Name = name, X = (T)x, Y = (T)y, Z = (T)z, W = (T)w};
        }

        List<MaterialAsset> loadMaterials()
        {
            var materials = new List<MaterialAsset>();

            if (!Directory.Exists(metadataPath + "Materials"))
            {
                Directory.CreateDirectory(metadataPath + "Materials");
            }

            foreach (var filename in Directory.EnumerateFiles(metadataPath + "Materials"))
            {
                var metadata = File.ReadAllLines(filename);

                var material = new MaterialAsset();

                material.Name = System.IO.Path.GetFileNameWithoutExtension(filename);
                material.Description = metadata[0].Split('=')[1].Trim();
                material.LastUpdated = parseLastUpdatedDate(metadata[1].Split('=')[1].Trim(), "material", material.Name);
                material.ImportedFilename = metadata[2].Split('=')[1].Trim();
                material.ImporterVersion = int.Parse(metadata[3].Split('=')[1].Trim());

                var textureLine = metadata[4].Split('=')[1].Trim();
                var textureConfigs = textureLine.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach(var config in textureConfigs)
                {
                    var data = config.Split(',');
                    var texture = new Texture()
                    {
                        Binding = data[0].Trim(),
                        SourceId = data[1].Trim()
                    };

                    material.Textures.Add(texture);
                }                

                for (int i = 5; i < metadata.Length; i++)
                {
                    var parameterGroupLine = metadata[i].Split('=')[1].Trim();
                    var groupConfigs = parameterGroupLine.Split('#');

                    var parameterGroup = new ParameterGroup() { Name = groupConfigs[0].Trim() };

                    var parameterConfigs = groupConfigs[1].Trim().Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);

                    foreach(var config in parameterConfigs)
                    {
                        var data = config.Split(',');

                        var parameterType = data[1].Trim();

                        if (parameterType == "float")
                        {
                            var parameter = new ScalarParameter<float>() { Name = data[0].Trim(), Value = float.Parse(data[2].Trim()) };
                            parameterGroup.Parameters.Add(parameter);
                        }
                        else if (parameterType == "int")
                        {
                            var parameter = new ScalarParameter<int>() { Name = data[0].Trim(), Value = int.Parse(data[2].Trim()) };
                            parameterGroup.Parameters.Add(parameter);
                        }
                        else if (parameterType == "bool")
                        {
                            var parameter = new ScalarParameter<bool>() { Name = data[0].Trim(), Value = bool.Parse(data[2].Trim()) };
                            parameterGroup.Parameters.Add(parameter);
                        }
                        else if (parameterType == "float2")
                        {
                            var parameter = parseVector2<float>(data[0].Trim(), data[2]);
                            parameterGroup.Parameters.Add(parameter);
                        }
                        else if (parameterType == "int2")
                        {
                            var parameter = parseVector2<int>(data[0].Trim(), data[2]);
                            parameterGroup.Parameters.Add(parameter);
                        }
                        else if (parameterType == "bool2")
                        {
                            var parameter = parseVector2<bool>(data[0].Trim(), data[2]);
                            parameterGroup.Parameters.Add(parameter);
                        }
                        else if (parameterType == "float3")
                        {
                            var parameter = parseVector3<float>(data[0].Trim(), data[2]);
                            parameterGroup.Parameters.Add(parameter);
                        }
                        else if (parameterType == "int3")
                        {
                            var parameter = parseVector3<int>(data[0].Trim(), data[2]);
                            parameterGroup.Parameters.Add(parameter);
                        }
                        else if (parameterType == "bool3")
                        {
                            var parameter = parseVector3<bool>(data[0].Trim(), data[2]);
                            parameterGroup.Parameters.Add(parameter);
                        }
                        else if (parameterType == "float4")
                        {
                            var parameter = parseVector4<float>(data[0].Trim(), data[2]);
                            parameterGroup.Parameters.Add(parameter);
                        }
                        else if (parameterType == "int4")
                        {
                            var parameter = parseVector4<int>(data[0].Trim(), data[2]);
                            parameterGroup.Parameters.Add(parameter);
                        }
                        else if (parameterType == "bool4")
                        {
                            var parameter = parseVector4<bool>(data[0].Trim(), data[2]);
                            parameterGroup.Parameters.Add(parameter);
                        }
                        else if (parameterType == "float4 array")
                        {
                            var elements = data[2].Split('$');

                            var array = new ArrayParameter<float>();
                            array.Name = data[0].Trim();                            

                            foreach(var element in elements)
                            {
                                var elementData = element.Split(',');
                                var parameter = parseVector4<float>(elementData[0].Trim(), elementData[2]);
                                array.Elements.Add(parameter);                                
                            }

                            parameterGroup.Parameters.Add(array);
                        }
                        else if (parameterType == "int4 array")
                        {
                            var elements = data[2].Split('$');

                            var array = new ArrayParameter<int>();
                            array.Name = data[0].Trim();

                            foreach (var element in elements)
                            {
                                var elementData = element.Split(',');
                                var parameter = parseVector4<int>(elementData[0].Trim(), elementData[2]);
                                array.Elements.Add(parameter);
                            }

                            parameterGroup.Parameters.Add(array);
                        }
                        else if (parameterType == "bool4 array")
                        {
                            var elements = data[2].Split('$');

                            var array = new ArrayParameter<bool>();
                            array.Name = data[0].Trim();

                            foreach (var element in elements)
                            {
                                var elementData = element.Split(',');
                                var parameter = parseVector4<bool>(elementData[0].Trim(), elementData[2]);
                                array.Elements.Add(parameter);
                            }

                            parameterGroup.Parameters.Add(array);
                        }
                    }

                    material.ParameterGroups.Add(parameterGroup);

                }

                materials.Add(material);
            }

            return materials;
        }

        List<StateGroupAsset> loadStateGroups()
        {
            var stateGroups = new List<StateGroupAsset>();

            if (!Directory.Exists(metadataPath + "StateGroups"))
            {
                Directory.CreateDirectory(metadataPath + "StateGroups");
            }

            foreach (var filename in Directory.EnumerateFiles(metadataPath + "StateGroups"))
            {
                var metadata = File.ReadAllLines(filename);

                var stateGroup = new StateGroupAsset();

                stateGroup.Name = System.IO.Path.GetFileNameWithoutExtension(filename);
                stateGroup.Description = metadata[0].Split('=')[1].Trim();
                stateGroup.LastUpdated = parseLastUpdatedDate(metadata[1].Split('=')[1].Trim(), "stateGroup", stateGroup.Name);
                stateGroup.VertexShaderId = metadata[2].Split('=')[1].Trim();
                stateGroup.GeometryShaderId = metadata[3].Split('=')[1].Trim();
                stateGroup.PixelShaderId = metadata[4].Split('=')[1].Trim();
                stateGroup.ShaderCombination = (ShaderCombination)Enum.Parse(typeof(ShaderCombination), metadata[5].Split('=')[1].Trim());
                stateGroup.ImportedFilename = metadata[6].Split('=')[1].Trim();
                stateGroup.ImporterVersion = int.Parse(metadata[7].Split('=')[1].Trim());

                var samplerLine = metadata[8].Split('=')[1].Trim();
                var samplerConfigs = samplerLine.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var config in samplerConfigs)
                {
                    var data = config.Split(',');
                    var sampler = new Sampler()
                    {
                        Name = data[0].Trim(),
                        Filter = (Filter)Enum.Parse(typeof(Filter), data[1].Trim()),
                        AddressU = (TextureAddressMode)Enum.Parse(typeof(TextureAddressMode), data[2].Trim()),
                        AddressV = (TextureAddressMode)Enum.Parse(typeof(TextureAddressMode), data[3].Trim()),
                        AddressW = (TextureAddressMode)Enum.Parse(typeof(TextureAddressMode), data[4].Trim())
                    };

                    stateGroup.Samplers.Add(sampler);

                }

                var textureLine = metadata[9].Split('=')[1].Trim();
                var textureConfigs = textureLine.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var config in textureConfigs)
                {
                    var data = config.Split(',');
                    var texture = new TextureBinding()
                    {
                        Slot = Int32.Parse(data[0].Trim()),
                        Binding = data[1].Trim()
                    };

                    stateGroup.TextureBindings.Add(texture);
                }

                var blendStateLine = metadata[10].Split('=')[1].Trim();
                var blendConfigs = blendStateLine.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var config in blendConfigs)
                {
                    var data = config.Split(',');

                    var renderTarget = new RenderTarget()
                    {
                        Index = int.Parse(data[0].Trim()),
                        BlendEnabled = bool.Parse(data[1].Trim()),
                        BlendOperation = (BlendOperation)Enum.Parse(typeof(BlendOperation), data[2].Trim()),
                        BlendOperationAlpha = (BlendOperation)Enum.Parse(typeof(BlendOperation), data[3].Trim()),
                        SourceBlend = (BlendOption)Enum.Parse(typeof(BlendOption), data[4].Trim()),
                        DestinationBlend = (BlendOption)Enum.Parse(typeof(BlendOption), data[5].Trim()),
                        SourceBlendAlpha = (BlendOption)Enum.Parse(typeof(BlendOption), data[6].Trim()),
                        DestinationBlendAlpha = (BlendOption)Enum.Parse(typeof(BlendOption), data[7].Trim()),
                        RenderTargetWriteMask = (WriteMask)Enum.Parse(typeof(WriteMask), data[8].Trim())
                    };

                    stateGroup.BlendState.RenderTargets.Add(renderTarget);
                }
                
                stateGroups.Add(stateGroup);
            }

            return stateGroups;
        }

        List<FontAsset> loadFonts()
        {
            var fonts = new List<FontAsset>();

            if (!Directory.Exists(metadataPath + "Fonts"))
            {
                Directory.CreateDirectory(metadataPath + "Fonts");
            }

            foreach (var filename in Directory.EnumerateFiles(metadataPath + "Fonts"))
            {
                var metadata = File.ReadAllLines(filename);

                var font = new FontAsset();

                font.Name = System.IO.Path.GetFileNameWithoutExtension(filename);
                font.Description = metadata[0].Split('=')[1].Trim();
                font.LastUpdated = parseLastUpdatedDate(metadata[1].Split('=')[1].Trim(), "font", font.Name);
                font.FontName = metadata[2].Split('=')[1].Trim();
                font.ImportedFilename = metadata[3].Split('=')[1].Trim();

                fonts.Add(font);
            }

            return fonts;
        }

        List<UIAsset> loadUIs()
        {
            var uis = new List<UIAsset>();

            if (!Directory.Exists(metadataPath + "UIs"))
            {
                Directory.CreateDirectory(metadataPath + "UIs");
            }

            foreach (var filename in Directory.EnumerateFiles(metadataPath + "UIs"))
            {
                var metadata = File.ReadAllLines(filename);

                var ui = new UIAsset();

                ui.Name = System.IO.Path.GetFileNameWithoutExtension(filename);
                ui.Description = metadata[0].Split('=')[1].Trim();
                ui.LastUpdated = parseLastUpdatedDate(metadata[1].Split('=')[1].Trim(), "ui", ui.Name);
                ui.ImportedFilename = metadata[2].Split('=')[1].Trim();
                ui.DefaultMesh = metadata[3].Split('=')[1].Trim();
                ui.DefaultMaterial = metadata[4].Split('=')[1].Trim();
                ui.DefaultFont = metadata[5].Split('=')[1].Trim();                

                uis.Add(ui);
            }

            return uis;
        }

        internal Dictionary<ScriptAsset, string[]> PopulateDatagrids()
        {
            loadMeshes().ForEach(mesh => viewmodel.Meshes.Add(mesh));

            loadTextures().ForEach(texture => viewmodel.Textures.Add(texture));

            var scriptDependencies = loadScripts();
            foreach(var kvp in scriptDependencies)
            {
                viewmodel.Scripts.Add(kvp.Key);
            }

            loadShaders().ForEach(shader => viewmodel.Shaders.Add(shader));

            loadMaterials().ForEach(material => viewmodel.Materials.Add(material));

            loadStateGroups().ForEach(stateGroup => viewmodel.StateGroups.Add(stateGroup));

            loadFonts().ForEach(font => viewmodel.Fonts.Add(font));

            loadUIs().ForEach(ui => viewmodel.UIs.Add(ui));

            return scriptDependencies;
        }

        /*
             * if a raw asset was modified when glitch wasn't running, it wouldn't have been updated.
             * so on startup, check all asset write times to see if raw time matches imported time
             */
        internal void CheckForOfflineAssetUpdates()
        {
            foreach (var mesh in viewmodel.Meshes)
            {
                var writeTime = System.IO.File.GetLastWriteTime(mesh.SourceFilename);

                if (writeTime > mesh.LastUpdated)
                {
                    //this asset was updated while glitch was offline, update it
                    if (assetWatcher.TryUpdateAsset(mesh))
                    {
                    }
                }
            }

            foreach (var texture in viewmodel.Textures)
            {
                var needsUpdate = texture.SourceFilenames.Any(s => System.IO.File.GetLastWriteTime(s) > texture.LastUpdated);//lastKnownTime);

                if (needsUpdate)
                {
                    //this asset was updated while glitch was offline, update it
                    if (assetWatcher.TryUpdateAsset(texture))
                    {
                    }
                }
            }

            foreach (var script in viewmodel.Scripts)
            {
                var writeTime = System.IO.File.GetLastWriteTime(script.SourceFilename);

                if (writeTime > script.LastUpdated)
                {
                    //this asset was updated while glitch was offline, update it
                    if (assetWatcher.TryUpdateAsset(script))
                    {

                        updateDependents(script);
                    }
                }
            }

            foreach (var shader in viewmodel.Shaders)
            {
                var writeTime = System.IO.File.GetLastWriteTime(shader.SourceFilename);
                
                if (writeTime > shader.LastUpdated)
                {
                    //this asset was updated while glitch was offline, update it
                    if (assetWatcher.TryUpdateAsset(shader))
                    {
                    }
                }
            }

            //TODO: importerversion for remainder of assets

            foreach(var stateGroup in viewmodel.StateGroups)
            {
                if (stateGroup.ImporterVersion != StateGroupImporter.ImporterVersion)
                {
                    if (assetWatcher.TryUpdateAsset(stateGroup))
                    {

                    }
                }
            }

            foreach(var material in viewmodel.Materials)
            {
                if (material.ImporterVersion != MaterialImporter.ImporterVersion)
                {
                    if (assetWatcher.TryUpdateAsset(material))
                    {

                    }
                }
            }
        }

        internal void updateDependents(Object asset)
        {
            if (asset is ScriptAsset)
            {
                var script = asset as ScriptAsset;

                if (viewmodel.ScriptDependencies.ContainsKey(script.Name))
                {
                    foreach (var dependent in viewmodel.ScriptDependencies[script.Name])
                    {
                        if (assetWatcher.TryUpdateAsset(dependent))
                        {
                            /*var updateAssetEvent = new ToolEvents.UpdateAssetEvent() { Asset = dependent, AssetType = ToolEvents.AssetType.Script };
                            client.ProcessEvent(updateAssetEvent);*/
                        }
                    }
                }
            }
            else if (asset is ShaderAsset)
            {
                var shader = asset as ShaderAsset;
                var stateGroupsUsingThis = viewmodel.StateGroups.Where(
                           m => m.VertexShader == shader
                               || m.GeometryShader == shader
                               || m.PixelShader == shader);

                foreach (var material in stateGroupsUsingThis)
                {
                    /*var update = new ToolEvents.UpdateAssetEvent() { Asset = material, AssetType = ToolEvents.AssetType.Material };
                    client.ProcessEvent(update);*/
                }
            }
            else if (asset is TextureAsset)
            {
                var texture = asset as TextureAsset;

                var materialsUsingThis = viewmodel.Materials.Where(
                    m => m.Textures.Any(t => t.Source == texture));

                foreach (var material in materialsUsingThis)
                {
                    /*var update = new ToolEvents.UpdateAssetEvent() { Asset = material, AssetType = ToolEvents.AssetType.Material };
                    client.ProcessEvent(update);*/
                }
            }
        }

        /*
        Materials depend on: shaders, textures. Materials only store shader/texture ids in mongo,
        so once everythings loaded, the actual ShaderAsset/TextureAsset properties of material
        needs to be filled
        */
        internal void ResolveDependencies(Dictionary<ScriptAsset, string[]> scriptDependencies)
        {
            foreach (var stateGroup in viewmodel.StateGroups)
            {
                if (!string.IsNullOrEmpty(stateGroup.VertexShaderId))
                {
                    var vertexShader = viewmodel.Shaders.First(s => s.Name == stateGroup.VertexShaderId);

                    Debug.Assert(vertexShader != null, "StateGroup: " + stateGroup.Name + " depends on vertex shader: " +
                        stateGroup.VertexShaderId + ", but it is no longer in the database! (this shouldnt happen!");

                    stateGroup.VertexShader = vertexShader;
                }

                if (!string.IsNullOrEmpty(stateGroup.GeometryShaderId))
                {
                    var geometryShader = viewmodel.Shaders.First(s => s.Name == stateGroup.GeometryShaderId);

                    Debug.Assert(geometryShader != null, "StateGroup: " + stateGroup.Name + " depends on geometry shader: " +
                        stateGroup.GeometryShaderId + ", but it is no longer in the database! (this shouldnt happen!");

                    stateGroup.GeometryShader = geometryShader;
                }

                if (!string.IsNullOrEmpty(stateGroup.PixelShaderId))
                {
                    var pixelShader = viewmodel.Shaders.First(s => s.Name == stateGroup.PixelShaderId);

                    Debug.Assert(pixelShader != null, "StateGroup: " + stateGroup.Name + " depends on pixel shader: " +
                        stateGroup.PixelShaderId + ", but it is no longer in the database! (this shouldnt happen!");

                    stateGroup.PixelShader = pixelShader;
                }
            }

            foreach(var material in viewmodel.Materials)
            {
                foreach (var texture in material.Textures)
                {
                    var source = viewmodel.Textures.FirstOrDefault(t => t.Name == texture.SourceId);

                    if (source == null)
                        continue;

                    Debug.Assert(source != null, "Material: " + material.Name + " depends on texture: " +
                        texture.SourceId + ", but it is no longer in the database! (this shouldnt happen!");

                    texture.Source = source;
                }
            }

            foreach(var kvp in scriptDependencies)
            {
                var script = kvp.Key;
                var dependencyNames = kvp.Value;

                foreach(var name in dependencyNames)
                {
                    script.Dependencies.Add(viewmodel.Scripts.First(s => s.Name == name));
                }
            }

            foreach (var script in viewmodel.Scripts)
            {
                foreach (var dependency in script.Dependencies)
                {
                    if (!viewmodel.ScriptDependencies.ContainsKey(dependency.Name))
                    {
                        viewmodel.ScriptDependencies.Add(dependency.Name, new List<ScriptAsset>());
                    }

                    viewmodel.ScriptDependencies[dependency.Name].Add(script);
                }
            }
        }
    }
}