using AssetManager;
using Assets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Glitch2
{
    /// <summary>
    /// Interaction logic for AssetDatabase.xaml
    /// </summary>
    public partial class AssetManager : Window
    {        
        AssetViewmodel viewmodel;        
        MetadataHandler metadataHandler;

        internal AssetManager()
        {
            InitializeComponent();
            viewmodel = new AssetViewmodel();

            this.DataContext = viewmodel;
            
            Action<Action> invoke = f => this.Dispatcher.Invoke(() => f());

            metadataHandler = new MetadataHandler(invoke, viewmodel);
            Connect();
        }


        public void Connect()
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
                /*var newAssetEvent = new ToolEvents.NewAssetEvent() { Asset = import.asset, AssetType = ToolEvents.AssetType.Mesh };
                client.ProcessEvent(newAssetEvent);*/

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
                    /*var deleteAssetEvent = new ToolEvents.DeleteAssetEvent { Asset = import.asset, AssetType = ToolEvents.AssetType.Mesh };
                    client.ProcessEvent(deleteAssetEvent);*/
                    System.IO.File.Delete(oldImported);

                    import.asset.Name = newName;

                    /*var newAssetEvent = new ToolEvents.NewAssetEvent() { Asset = import.asset, AssetType = ToolEvents.AssetType.Mesh };
                    client.ProcessEvent(newAssetEvent);*/
                }
                else
                {
                    /*var updateAssetEvent = new ToolEvents.UpdateAssetEvent { Asset = import.asset, AssetType = ToolEvents.AssetType.Mesh };
                    client.ProcessEvent(updateAssetEvent);*/
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
                /*var deleteAssetEvent = new ToolEvents.DeleteAssetEvent() { Asset = viewmodel.SelectedMesh, AssetType = ToolEvents.AssetType.Mesh };

                client.ProcessEvent(deleteAssetEvent);*/

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
                /*var newAssetEvent = new ToolEvents.NewAssetEvent() { Asset = import.asset, AssetType = ToolEvents.AssetType.Texture };

                client.ProcessEvent(newAssetEvent);*/

                viewmodel.Textures.Add(import.asset);
            }
        }

        private void EditTexture(object sender, RoutedEventArgs e)
        {
            if (viewmodel.SelectedTexture == null)
                return;

            string oldName = viewmodel.SelectedTexture.Name;
            string oldImported = viewmodel.SelectedTexture.ImportedFilename;
            string oldSource = viewmodel.SelectedTexture.SourceFilename;

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
                    /*var deleteAssetEvent = new ToolEvents.DeleteAssetEvent { Asset = import.asset, AssetType = ToolEvents.AssetType.Texture };
                    client.ProcessEvent(deleteAssetEvent);*/
                    System.IO.File.Delete(oldImported);

                    import.asset.Name = newName;

                    /*var newAssetEvent = new ToolEvents.NewAssetEvent() { Asset = import.asset, AssetType = ToolEvents.AssetType.Texture };
                    client.ProcessEvent(newAssetEvent);*/
                }
                else
                {
                    /*var updateAssetEvent = new ToolEvents.UpdateAssetEvent { Asset = import.asset, AssetType = ToolEvents.AssetType.Texture };
                    client.ProcessEvent(updateAssetEvent);*/
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
                    /*var deleteAssetEvent = new ToolEvents.DeleteAssetEvent() { Asset = viewmodel.SelectedTexture, AssetType = ToolEvents.AssetType.Texture };

                    client.ProcessEvent(deleteAssetEvent);*/

                    System.IO.File.Delete(viewmodel.SelectedTexture.ImportedFilename);
                    viewmodel.Textures.Remove(viewmodel.SelectedTexture);
                }

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

                /*var newAssetEvent = new ToolEvents.NewAssetEvent() { Asset = import.asset, AssetType = ToolEvents.AssetType.Script };

                client.ProcessEvent(newAssetEvent);*/

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
                    /*var deleteAssetEvent = new ToolEvents.DeleteAssetEvent { Asset = import.asset, AssetType = ToolEvents.AssetType.Script };
                    client.ProcessEvent(deleteAssetEvent);*/
                    System.IO.File.Delete(oldImported);
                    System.IO.File.Delete(System.IO.Path.ChangeExtension(oldImported, "pdb"));

                    import.asset.Name = newName;

                    /*var newAssetEvent = new ToolEvents.NewAssetEvent() { Asset = import.asset, AssetType = ToolEvents.AssetType.Script };
                    client.ProcessEvent(newAssetEvent);*/
                }
                else
                {
                    /*var updateAssetEvent = new ToolEvents.UpdateAssetEvent { Asset = import.asset, AssetType = ToolEvents.AssetType.Script };
                    client.ProcessEvent(updateAssetEvent);*/
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

                /*var deleteAssetEvent = new ToolEvents.DeleteAssetEvent() { Asset = viewmodel.SelectedScript, AssetType = ToolEvents.AssetType.Script };

                client.ProcessEvent(deleteAssetEvent);*/

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
                /*var newAssetEvent = new ToolEvents.NewAssetEvent() { Asset = import.asset, AssetType = ToolEvents.AssetType.Shader };

                client.ProcessEvent(newAssetEvent);*/
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
                    /*var deleteAssetEvent = new ToolEvents.DeleteAssetEvent { Asset = import.asset, AssetType = ToolEvents.AssetType.Shader };
                    client.ProcessEvent(deleteAssetEvent);*/
                    System.IO.File.Delete(oldImported);

                    import.asset.Name = newName;

                    /*var newAssetEvent = new ToolEvents.NewAssetEvent() { Asset = import.asset, AssetType = ToolEvents.AssetType.Shader };
                    client.ProcessEvent(newAssetEvent);*/
                }
                else
                {
                    /*var updateAssetEvent = new ToolEvents.UpdateAssetEvent { Asset = import.asset, AssetType = ToolEvents.AssetType.Shader };
                    client.ProcessEvent(updateAssetEvent);*/
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

                //check to see if any material depends on this shader
                var dependencyChecker = new DependencyChecker();
                dependencyChecker.Dependencies =
                       viewmodel.Materials.Where(
                           m => m.VertexShader == viewmodel.SelectedShader
                               || m.GeometryShader == viewmodel.SelectedShader
                               || m.PixelShader == viewmodel.SelectedShader)
                               .Select(m => new Dependency { Name = m.Name, AssetType = "Material" }).ToList();

                if (dependencyChecker.Dependencies.Count > 0)
                {
                    dependencyChecker.Show();
                    MessageBox.Show("Can't remove this shader, there are materials that depend on it. See the dependency window", "ERROR!", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }
                else
                {
                    /*var deleteAssetEvent = new ToolEvents.DeleteAssetEvent() { Asset = viewmodel.SelectedShader, AssetType = ToolEvents.AssetType.Shader };

                    client.ProcessEvent(deleteAssetEvent);*/

                    AssetMetadata.deleteShaderMetadata(viewmodel.SelectedShader);

                    System.IO.File.Delete(viewmodel.SelectedShader.ImportedFilename);
                    viewmodel.Shaders.Remove(viewmodel.SelectedShader);
                }
            }
        }

        private void CreateMaterial(object sender, RoutedEventArgs e)
        {
            MaterialEditor editor = new MaterialEditor();
            editor.AvailableVertexShaders = viewmodel.Shaders;

            editor.AvailableGeometryShaders = new ObservableCollection<ShaderAsset>(viewmodel.Shaders.Where(s =>
                s.Combination == ShaderCombination.VertexGeometry || s.Combination == ShaderCombination.VertexGeometryPixel));

            editor.AvailablePixelShaders = new ObservableCollection<ShaderAsset>(viewmodel.Shaders.Where(s =>
                s.Combination == ShaderCombination.VertexPixel || s.Combination == ShaderCombination.VertexGeometryPixel));

            editor.AvailableTextures = viewmodel.Textures;

            var result = editor.ShowDialog();

            if (result == true)
            {
                /*var newAssetEvent = new ToolEvents.NewAssetEvent() { Asset = editor.asset, AssetType = ToolEvents.AssetType.Material };

                client.ProcessEvent(newAssetEvent);*/
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

            editor.AvailableVertexShaders = viewmodel.Shaders;

            editor.AvailableGeometryShaders = new ObservableCollection<ShaderAsset>(viewmodel.Shaders.Where(s =>
                s.Combination == ShaderCombination.VertexGeometry || s.Combination == ShaderCombination.VertexGeometryPixel));

            editor.AvailablePixelShaders = new ObservableCollection<ShaderAsset>(viewmodel.Shaders.Where(s =>
                s.Combination == ShaderCombination.VertexPixel || s.Combination == ShaderCombination.VertexGeometryPixel));

            editor.AvailableTextures = viewmodel.Textures;

            var result = editor.ShowDialog();

            if (result == true)
            {
                if (oldName != editor.asset.Name)
                {
                    var newName = editor.asset.Name;

                    editor.asset.Name = oldName;

                    //we changed the name, delete the old asset then create a new one
                    /*var deleteAssetEvent = new ToolEvents.DeleteAssetEvent { Asset = editor.asset, AssetType = ToolEvents.AssetType.Material };
                    client.ProcessEvent(deleteAssetEvent);*/
                    System.IO.File.Delete(oldImported);

                    editor.asset.Name = newName;

                    /*var newAssetEvent = new ToolEvents.NewAssetEvent() { Asset = editor.asset, AssetType = ToolEvents.AssetType.Material };
                    client.ProcessEvent(newAssetEvent);*/
                }
                else
                {
                    /*var updateAssetEvent = new ToolEvents.UpdateAssetEvent { Asset = editor.asset, AssetType = ToolEvents.AssetType.Material };
                    client.ProcessEvent(updateAssetEvent);*/
                }
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
                /*var deleteAssetEvent = new ToolEvents.DeleteAssetEvent() { Asset = viewmodel.SelectedMaterial, AssetType = ToolEvents.AssetType.Material };

                client.ProcessEvent(deleteAssetEvent);*/
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
                /*var deleteAssetEvent = new ToolEvents.DeleteAssetEvent() { Asset = viewmodel.SelectedUI, AssetType = ToolEvents.AssetType.UI };

                client.ProcessEvent(deleteAssetEvent);*/

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
                /*var newAssetEvent = new ToolEvents.NewAssetEvent() { Asset = import.asset, AssetType = ToolEvents.AssetType.Font };

                client.ProcessEvent(newAssetEvent);*/

                viewmodel.Fonts.Add(import.asset);
            }
            else
            {
                
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
                    /*var deleteAssetEvent = new ToolEvents.DeleteAssetEvent { Asset = import.asset, AssetType = ToolEvents.AssetType.Font };
                    client.ProcessEvent(deleteAssetEvent);*/
                    System.IO.File.Delete(oldImported);

                    import.asset.Name = newName;

                    /*var newAssetEvent = new ToolEvents.NewAssetEvent() { Asset = import.asset, AssetType = ToolEvents.AssetType.Font };
                    client.ProcessEvent(newAssetEvent);*/
                }
                else
                {
                    /*var updateAssetEvent = new ToolEvents.UpdateAssetEvent { Asset = import.asset, AssetType = ToolEvents.AssetType.Font };
                    client.ProcessEvent(updateAssetEvent);*/
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
                /*var deleteAssetEvent = new ToolEvents.DeleteAssetEvent() { Asset = viewmodel.SelectedFont, AssetType = ToolEvents.AssetType.Font };

                client.ProcessEvent(deleteAssetEvent);*/

                System.IO.File.Delete(viewmodel.SelectedFont.ImportedFilename);
                viewmodel.Fonts.Remove(viewmodel.SelectedFont);
            }
        }

        

    }
}
