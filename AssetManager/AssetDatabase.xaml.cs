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
using AssetManager;
using Assets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace AssetManager
{
    public partial class AssetManagerUI : Window
    {        
        AssetViewmodel viewmodel;        
        MetadataHandler metadataHandler;

        internal AssetManagerUI()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.ImportedAssetsPath))
            {

                var setupProperties = new SetupProperties();
                var result = setupProperties.ShowDialog();

                Func<string, string> ensureTrailingSlash = (s) => { if (s.EndsWith("\\")) return s; else return s + "\\"; };

                if (result == true)
                {
                    Properties.Settings.Default.ImportedAssetsPath = ensureTrailingSlash(setupProperties.ImportedAssetsPath);        
                    Properties.Settings.Default.MetadataPath = ensureTrailingSlash(setupProperties.MetadataPath);
                    Properties.Settings.Default.RawAssetsPath = ensureTrailingSlash(setupProperties.RawAssetsPath);
                    Properties.Settings.Default.Save();
                }
                else
                {
                    MessageBox.Show("Properties were not setup, please run again and fill all the textboxes");
                    Application.Current.Shutdown();
                    return;
                }
            }

            InitializeComponent();
            viewmodel = new AssetViewmodel();

            this.DataContext = viewmodel;
            
            Action<Action> invoke = f => this.Dispatcher.Invoke(() => f());

            metadataHandler = new MetadataHandler(invoke, viewmodel);
            setup();

            this.Closed += (a, b) =>
            {
                metadataHandler.stopWatchingAssets();                
            };
        }

        public void setup()
        {      
            var scriptDependencies = metadataHandler.PopulateDatagrids();
            metadataHandler.ResolveDependencies(scriptDependencies);
            metadataHandler.CheckForOfflineAssetUpdates();
        }   

        AssetViewmodel Context { get { return viewmodel; } }
                
        private void ImportMesh(object sender, RoutedEventArgs e)
        {
            ImportMesh import = new ImportMesh();

            import.AvailableTopologies = new ObservableCollection<Topology>(typeof(Topology).GetEnumValues().Cast<Topology>().ToList());

            var result = import.ShowDialog();

            if (result == true)
            {                
                AssetMetadata.createMeshMetadata(import.asset);
                viewmodel.Meshes.Add(import.asset);
            }
            else
            {
                if (!string.IsNullOrEmpty(import.Error))
                {
                    viewmodel.LogEvent(import.Error);
                }
            }

        }

        private void EditMesh(object sender, RoutedEventArgs e)
        {
            if (viewmodel.SelectedMesh == null)
                return;

            string oldName = viewmodel.SelectedMesh.Name;
            string oldImported = viewmodel.SelectedMesh.ImportedFilename;
            string oldSource = viewmodel.SelectedMesh.SourceFilename;

            ImportMesh import = new ImportMesh();
            import.setAsset(viewmodel.SelectedMesh);
            import.isEditMode = true;
            import.AvailableTopologies = new ObservableCollection<Topology>(typeof(Topology).GetEnumValues().Cast<Topology>().ToList());

            var result = import.ShowDialog();

            if (result == true)
            {
                if (oldName != import.asset.Name)
                {
                    var newName = import.asset.Name;

                    import.asset.Name = oldName;

                    //we changed the name, delete the old asset then create a new one
                    System.IO.File.Delete(oldImported);

                    import.asset.Name = newName;
                }
            }
        }   

        private void RemoveMesh(object sender, RoutedEventArgs e)
        {
            if (viewmodel.SelectedMesh == null)
                return;

            var result = MessageBox.Show("This will remove the mesh from the asset database, and the exported binary mesh file from GameRepos\\Assets\\Meshes. Are you sure? (Do this only if its saved in mercurial)",
                "WARNING! MESH DELETION IMMINENT!",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {  
                AssetMetadata.deleteMeshMetadata(viewmodel.SelectedMesh);
                System.IO.File.Delete(viewmodel.SelectedMesh.ImportedFilename);
                viewmodel.Meshes.Remove(viewmodel.SelectedMesh);
            }
        }

        private void ImportTexture(object sender, RoutedEventArgs e)
        {
            ImportTexture import = new ImportTexture();

            var result = import.ShowDialog();

            if (result == true)
            {
                AssetMetadata.createTextureMetadata(import.asset);
                viewmodel.Textures.Add(import.asset);
            }
        }

        private void EditTexture(object sender, RoutedEventArgs e)
        {
            if (viewmodel.SelectedTexture == null)
                return;

            string oldName = viewmodel.SelectedTexture.Name;
            string oldImported = viewmodel.SelectedTexture.ImportedFilename;

            ImportTexture import = new ImportTexture();
            import.setAsset(viewmodel.SelectedTexture);
            import.isEditMode = true;
            
            var result = import.ShowDialog();

            if (result == true)
            {
                if (oldName != import.asset.Name)
                {
                    var newName = import.asset.Name;

                    import.asset.Name = oldName;

                    //we changed the name, delete the old asset then create a new one
                    System.IO.File.Delete(oldImported);

                    import.asset.Name = newName;
                }
            }
        }

        private void RemoveTexture(object sender, RoutedEventArgs e)
        {
            if (viewmodel.SelectedTexture == null)
                return;

            var result = MessageBox.Show("This will remove the texture from the asset database, and the exported texture file from GameRepos\\Assets\\Textures. Are you sure? (Do this only if its saved in mercurial)",
               "WARNING! TEXTURE DELETION IMMINENT!",
               MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                //check to see if any material depends on this shader
                var dependencyChecker = new DependencyChecker();
                dependencyChecker.Dependencies =
                       viewmodel.Materials.Where(
                           m => m.Textures.Where(t => t.SourceId == viewmodel.SelectedTexture.Name).ToList().Count > 0)
                               .Select(m => new Dependency { Name = m.Name, AssetType = "Texture" }).ToList();

                if (dependencyChecker.Dependencies.Count > 0)
                {
                    dependencyChecker.Show();
                    MessageBox.Show("Can't remove this texture, there are materials that depend on it. See the dependency window", "ERROR!", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }
                else
                {
                    AssetMetadata.deleteTextureMetadata(viewmodel.SelectedTexture);
                    System.IO.File.Delete(viewmodel.SelectedTexture.ImportedFilename);
                    viewmodel.Textures.Remove(viewmodel.SelectedTexture);
                }

                dependencyChecker.Close();
            }
        }

        private void ImportScript(object sender, RoutedEventArgs e)
        {
            ImportScript import = new ImportScript();

            import.AvailableScripts = viewmodel.Scripts;            

            var result = import.ShowDialog();

            if (result == true)
            {
                foreach (var dependency in import.asset.Dependencies)
                {
                    if (!viewmodel.ScriptDependencies.ContainsKey(dependency.Name))
                    {
                        viewmodel.ScriptDependencies.Add(dependency.Name, new List<ScriptAsset>());
                    }

                    viewmodel.ScriptDependencies[dependency.Name].Add(import.asset);
                }

                viewmodel.Scripts.Add(import.asset);
            }
            else
            {
                if (!string.IsNullOrEmpty(import.Errors))
                {
                    viewmodel.LogEvent(import.Errors);
                }
            }
        }

        private void EditScript(object sender, RoutedEventArgs e)
        {
            if (viewmodel.SelectedScript == null)
                return;

            string oldName = viewmodel.SelectedScript.Name;
            string oldImported = viewmodel.SelectedScript.ImportedFilename;
            string oldSource = viewmodel.SelectedScript.SourceFilename;

            ImportScript import = new ImportScript();
            import.setAsset(viewmodel.SelectedScript);
            import.isEditMode = true;

            import.AvailableScripts = viewmodel.Scripts;            

            var result = import.ShowDialog();

            if (result == true)
            {
                //remove all dependencies then add the current/new ones
                //simpler than checking which dependency was recently removed/added
                foreach (var dependency in import.asset.Dependencies)
                {
                    if (viewmodel.ScriptDependencies.ContainsKey(dependency.Name))
                    {
                        viewmodel.ScriptDependencies[dependency.Name].Remove(import.asset);
                    }
                }

                if (oldName != import.asset.Name)
                {
                    var newName = import.asset.Name;
                    
                    import.asset.Name = oldName;

                    //we changed the name, delete the old asset then create a new one
                    System.IO.File.Delete(oldImported);
                    System.IO.File.Delete(System.IO.Path.ChangeExtension(oldImported, "pdb"));

                    import.asset.Name = newName;
                }
                else
                {
                    metadataHandler.updateDependents(viewmodel.SelectedScript);
                }


                foreach (var dependency in import.asset.Dependencies)
                {
                    if (!viewmodel.ScriptDependencies.ContainsKey(dependency.Name))
                    {
                        viewmodel.ScriptDependencies.Add(dependency.Name, new List<ScriptAsset>());
                    }

                    viewmodel.ScriptDependencies[dependency.Name].Add(import.asset);
                }
                
            }
            else
            {
                if (!string.IsNullOrEmpty(import.Errors))
                {
                    viewmodel.LogEvent(import.Errors);
                }
            }
        }

        private void RemoveScript(object sender, RoutedEventArgs e)
        {
            if (viewmodel.SelectedScript == null)
                return;

            var result = MessageBox.Show("This will remove the script from the asset database, and the exported script file from GameRepos\\Assets\\Scripts. Are you sure? (Do this only if its saved in mercurial)",
               "WARNING! TEXTURE DELETION IMMINENT!",
               MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                if (viewmodel.ScriptDependencies.ContainsKey(viewmodel.SelectedScript.Name))
                {
                    viewmodel.ScriptDependencies.Remove(viewmodel.SelectedScript.Name);
                }

                foreach (var kvp in viewmodel.ScriptDependencies)
                {
                    kvp.Value.RemoveAll(script => script == viewmodel.SelectedScript);
                }

                System.IO.File.Delete(viewmodel.SelectedScript.ImportedFilename);
                System.IO.File.Delete(System.IO.Path.ChangeExtension(viewmodel.SelectedScript.ImportedFilename, ".pdb"));
                viewmodel.Scripts.Remove(viewmodel.SelectedScript);                
            }
        }

        private void ImportShader(object sender, RoutedEventArgs e)
        {
            ImportShader import = new ImportShader();

            var result = import.ShowDialog();

            if (result == true)
            {
                AssetMetadata.createShaderMetadata(import.asset);
                
                viewmodel.Shaders.Add(import.asset);
            }
            else
            {
                if (!string.IsNullOrEmpty(import.Error))
                {
                    viewmodel.LogEvent(import.Error);
                }
            }
        }

        private void EditShader(object sender, RoutedEventArgs e)
        {
            if (viewmodel.SelectedShader == null)
                return;

            string oldName = viewmodel.SelectedShader.Name;
            string oldImported = viewmodel.SelectedShader.ImportedFilename;
            string oldSource = viewmodel.SelectedShader.SourceFilename;

            ImportShader import = new ImportShader();
            import.setAsset(viewmodel.SelectedShader);
            import.isEditMode = true;
            
            var result = import.ShowDialog();

            if (result == true)
            {
                if (oldName != import.asset.Name)
                {
                    var newName = import.asset.Name;

                    import.asset.Name = oldName;

                    //we changed the name, delete the old asset then create a new one
                    System.IO.File.Delete(oldImported);

                    import.asset.Name = newName;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(import.Error))
                {
                    viewmodel.LogEvent(import.Error);
                }
            }
        }

        private void RemoveShader(object sender, RoutedEventArgs e)
        {
            if (viewmodel.SelectedShader == null)
                return;

            var result = MessageBox.Show("This will remove the shader from the asset database, and the exported shader file from GameRepos\\Assets\\Shaders. Are you sure? (Do this only if its saved in mercurial)",
               "WARNING! SHADER DELETION IMMINENT!",
               MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                //check to see if any state group depends on this shader
                var dependencyChecker = new DependencyChecker();
                dependencyChecker.Dependencies =
                       viewmodel.StateGroups.Where(
                           m => m.VertexShader == viewmodel.SelectedShader
                               || m.GeometryShader == viewmodel.SelectedShader
                               || m.PixelShader == viewmodel.SelectedShader)
                               .Select(m => new Dependency { Name = m.Name, AssetType = "StateGroup" }).ToList();

                if (dependencyChecker.Dependencies.Count > 0)
                {
                    dependencyChecker.Show();
                    MessageBox.Show("Can't remove this shader, there are state groups that depend on it. See the dependency window", "ERROR!", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }
                else
                {
                    AssetMetadata.deleteShaderMetadata(viewmodel.SelectedShader);

                    System.IO.File.Delete(viewmodel.SelectedShader.ImportedFilename);
                    viewmodel.Shaders.Remove(viewmodel.SelectedShader);
                }
            }
        }

        private void CreateMaterial(object sender, RoutedEventArgs e)
        {
            var basedStateGroup = new BaseMaterialOnStateGroup(viewmodel.StateGroups.ToList());
            var result = basedStateGroup.ShowDialog();

            if (result == false)
                return;

            var editor = new MaterialEditor(basedStateGroup.SelectedStateGroup);

            editor.AvailableTextures = viewmodel.Textures;

            result = editor.ShowDialog();

            if (result == true)
            {
                AssetMetadata.createMaterialMetadata(editor.asset);

                viewmodel.Materials.Add(editor.asset);
            }
        }

        private void EditMaterial(object sender, RoutedEventArgs e)
        {
            if (viewmodel.SelectedMaterial == null)
                return;

            string oldName = viewmodel.SelectedMaterial.Name;
            string oldImported = viewmodel.SelectedMaterial.ImportedFilename;

            MaterialEditor editor = new MaterialEditor(viewmodel.SelectedMaterial);

            editor.AvailableTextures = viewmodel.Textures;

            var result = editor.ShowDialog();

            if (result == true)
            {
                if (oldName != editor.asset.Name)
                {
                    var newName = editor.asset.Name;

                    editor.asset.Name = oldName;

                    //we changed the name, delete the old asset then create a new one
                    System.IO.File.Delete(oldImported);

                    editor.asset.Name = newName;
                }
                else
                {
                    AssetMetadata.createMaterialMetadata(viewmodel.SelectedMaterial);
                }
            }
        }

        private void CopyMaterial(object sender, RoutedEventArgs e)
        {
            if (viewmodel.SelectedMaterial == null)
                return;

            var copy = new MaterialAsset()
            {
                Description = viewmodel.SelectedMaterial.Description,
                Name = viewmodel.SelectedMaterial.Name + "_copy",
                Textures = new ObservableCollection<Texture>(viewmodel.SelectedMaterial.Textures.ToList()),
                ParameterGroups = new ObservableCollection<ParameterGroup>(viewmodel.SelectedMaterial.ParameterGroups.ToList())
            };

            MaterialEditor editor = new MaterialEditor(copy);

            editor.AvailableTextures = viewmodel.Textures;

            var result = editor.ShowDialog();

            if (result == true)
            {                
                AssetMetadata.createMaterialMetadata(copy);
                viewmodel.Materials.Add(copy);
            } 
        }

        private void RemoveMaterial(object sender, RoutedEventArgs e)
        {
            if (viewmodel.SelectedMaterial == null)
                return;

            var result = MessageBox.Show("This will remove the material from the asset database, and the exported material file from GameRepos\\Assets\\Materials. Are you sure? (Do this only if its saved in mercurial)",
              "WARNING! MATERIAL DELETION IMMINENT!",
              MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                AssetMetadata.deleteMaterialMetadata(viewmodel.SelectedMaterial);

                System.IO.File.Delete(viewmodel.SelectedMaterial.ImportedFilename);
                viewmodel.Materials.Remove(viewmodel.SelectedMaterial);
            }
        }

        private void CreateUserInterface(object sender, RoutedEventArgs e)
        {
            /*UIEditor editor = new UIEditor();

            editor.viewmodel.AvailableScripts = viewmodel.Scripts;
            editor.viewmodel.AvailableFonts = viewmodel.Fonts;
            editor.viewmodel.AvailableMaterials = viewmodel.Materials;
            editor.viewmodel.AvailableMeshes = viewmodel.Meshes;

            var result = editor.ShowDialog();

            if (result == true)
            {
                var newAssetEvent = new ToolEvents.NewAssetEvent() { Asset = editor.asset, AssetType = ToolEvents.AssetType.UI };

                client.ProcessEvent(newAssetEvent);

                viewmodel.UIs.Add(editor.asset);
            }*/
        }

        private void EditUI(object sender, RoutedEventArgs e)
        {
            /*if (viewmodel.SelectedUI == null)
                return;

            string oldName = viewmodel.SelectedUI.Name;
            string oldImported = viewmodel.SelectedUI.ImportedFilename;

            UIEditor editor = new UIEditor(viewmodel.SelectedUI);

            editor.viewmodel.AvailableScripts = viewmodel.Scripts;
            editor.viewmodel.AvailableFonts = viewmodel.Fonts;
            editor.viewmodel.AvailableMaterials = viewmodel.Materials;
            editor.viewmodel.AvailableMeshes = viewmodel.Meshes;

            var result = editor.ShowDialog();

            if (result == true)
            {
                if (oldName != editor.asset.Name)
                {
                    var newName = editor.asset.Name;

                    editor.asset.Name = oldName;

                    //we changed the name, delete the old asset then create a new one
                    /*var deleteAssetEvent = new ToolEvents.DeleteAssetEvent { Asset = editor.asset, AssetType = ToolEvents.AssetType.UI };
                    client.ProcessEvent(deleteAssetEvent);*/
                    /*System.IO.File.Delete(oldImported);

                    editor.asset.Name = newName;

                    /*var newAssetEvent = new ToolEvents.NewAssetEvent() { Asset = editor.asset, AssetType = ToolEvents.AssetType.UI };
                    client.ProcessEvent(newAssetEvent);*/
                /*}
                else
                {
                    /*var updateAssetEvent = new ToolEvents.UpdateAssetEvent { Asset = editor.asset, AssetType = ToolEvents.AssetType.UI };
                    client.ProcessEvent(updateAssetEvent);*/
                /*}   
            }*/
        }

        private void RemoveUI(object sender, RoutedEventArgs e)
        {
            if (viewmodel.SelectedUI == null)
                return;

            var result = MessageBox.Show("This will remove the ui from the asset database, and the exported binary ui file from GameRepos\\Assets\\UI. Are you sure? (Do this only if its saved in mercurial)",
                "WARNING! UI DELETION IMMINENT!",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                System.IO.File.Delete(viewmodel.SelectedUI.ImportedFilename);
                viewmodel.UIs.Remove(viewmodel.SelectedUI);
            }
        }

        private void ImportFont(object sender, RoutedEventArgs e)
        {
            ImportFont import = new ImportFont();

            var result = import.ShowDialog();
            
            if (result == true)
            {                

                viewmodel.Fonts.Add(import.asset);
            }
        }

        private void EditFont(object sender, RoutedEventArgs e)
        {
            if (viewmodel.SelectedFont == null)
                return;

            string oldName = viewmodel.SelectedFont.Name;
            string oldImported = viewmodel.SelectedFont.ImportedFilename;

            ImportFont import = new ImportFont();
            import.setAsset(viewmodel.SelectedFont);
            import.isEditMode = true;
            
            var result = import.ShowDialog();

            if (result == true)
            {
                if (oldName != import.asset.Name)
                {
                    var newName = import.asset.Name;

                    import.asset.Name = oldName;

                    //we changed the name, delete the old asset then create a new one
                    System.IO.File.Delete(oldImported);

                    import.asset.Name = newName;
                }
            }
        }

        private void RemoveFont(object sender, RoutedEventArgs e)
        {
            if (viewmodel.SelectedFont == null)
                return;

            var result = MessageBox.Show("This will remove the font from the asset database, and the exported binary font file from GameRepos\\Assets\\Fonts. Are you sure? (Do this only if its saved in mercurial)",
                "WARNING! FONT DELETION IMMINENT!",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {                
                System.IO.File.Delete(viewmodel.SelectedFont.ImportedFilename);
                viewmodel.Fonts.Remove(viewmodel.SelectedFont);
            }
        }

        private void CreateStateGroup(object sender, RoutedEventArgs e)
        {
            StateGroupEditor editor = new StateGroupEditor();
            editor.AvailableVertexShaders = viewmodel.Shaders;

            editor.AvailableGeometryShaders = new ObservableCollection<ShaderAsset>(viewmodel.Shaders.Where(s =>
                s.Combination == ShaderCombination.VertexGeometry || s.Combination == ShaderCombination.VertexGeometryPixel));

            editor.AvailablePixelShaders = new ObservableCollection<ShaderAsset>(viewmodel.Shaders.Where(s =>
                s.Combination == ShaderCombination.VertexPixel || s.Combination == ShaderCombination.VertexGeometryPixel));

            var result = editor.ShowDialog();

            if (result == true)
            {
                AssetMetadata.createStateGroupMetadata(editor.asset);

                viewmodel.StateGroups.Add(editor.asset);
            }
        }

        private void EditStateGroup(object sender, RoutedEventArgs e)
        {
            if (viewmodel.SelectedStateGroup == null)
                return;

            string oldName = viewmodel.SelectedStateGroup.Name;
            string oldImported = viewmodel.SelectedStateGroup.ImportedFilename;

            var editor = new StateGroupEditor(viewmodel.SelectedStateGroup);

            editor.AvailableVertexShaders = viewmodel.Shaders;

            editor.AvailableGeometryShaders = new ObservableCollection<ShaderAsset>(viewmodel.Shaders.Where(s =>
                s.Combination == ShaderCombination.VertexGeometry || s.Combination == ShaderCombination.VertexGeometryPixel));

            editor.AvailablePixelShaders = new ObservableCollection<ShaderAsset>(viewmodel.Shaders.Where(s =>
                s.Combination == ShaderCombination.VertexPixel || s.Combination == ShaderCombination.VertexGeometryPixel));

            var result = editor.ShowDialog();

            if (result == true)
            {
                if (oldName != editor.asset.Name)
                {
                    var newName = editor.asset.Name;

                    editor.asset.Name = oldName;

                    //we changed the name, delete the old asset then create a new one
                    System.IO.File.Delete(oldImported);

                    editor.asset.Name = newName;
                }
                else
                {
                    AssetMetadata.createStateGroupMetadata(viewmodel.SelectedStateGroup);
                }
            }
        }

        private void RemoveStateGroup(object sender, RoutedEventArgs e)
        {
            if (viewmodel.SelectedStateGroup == null)
                return;

            var result = MessageBox.Show("This will remove the state group from the asset database, and the exported state group file from ImportedAssets\\StateGroups\\. Are you sure? (Do this only if its saved in mercurial)",
              "WARNING! STATE GROUP DELETION IMMINENT!",
              MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                AssetMetadata.deletestateGroupMetadata(viewmodel.SelectedStateGroup);

                System.IO.File.Delete(viewmodel.SelectedStateGroup.ImportedFilename);
                viewmodel.StateGroups.Remove(viewmodel.SelectedStateGroup);
            }
        }
    }
}
