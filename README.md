# AssetManager
Converts game assets such as meshes, shaders, scripts to a custom binary format used by Project Stacks and Chengine. To learn more visit http://patrick-lafferty.github.io/projects/projecstacks.

# Building
1. clone [assimp](https://github.com/assimp/assimp) into Dependencies/assimp. 
2. run CMake for assimp (set LIBRARY_SUFFIX to empty string when generating), build Assimp.sln
3. build AssetManager.sln, requires Visual Studio 2015.

# Usage
AssetManager expects:
* a path where raw assets are stored (wavefront meshes: *.obj. shaders: *.hlsl)
* a path where it can store metadata for assets
* a path where it saves the binary imported assets to

Typically you would have a layout like this:

├── Game  
│   ├── RawAssets  
│   │   └── Metadata  
│   ├── RawAssetsImportedAssets
