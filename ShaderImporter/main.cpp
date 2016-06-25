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
#include <iostream>
#include <fstream>
#include <vector>
#include <d3dcompiler.h>
#include <d3d11.h>

auto getTempFilename() -> std::string
{
	TCHAR tempPathBuffer[MAX_PATH];
	GetTempPath(MAX_PATH, tempPathBuffer);
	TCHAR filename[MAX_PATH];
	GetTempFileName(tempPathBuffer, "shdr", 0, filename);

	return std::string(filename);
}

auto runFxc(std::string shader, std::string entryPoint, std::string target) -> std::string
{
	//TODO: hardcoded path	
	std::string fxc = "\"C:\\Program Files (x86)\\Windows Kits\\8.1\\bin\\x64\\fxc.exe\" ";
	std::string command = fxc;
	std::string tempFilename = getTempFilename();

	command += shader;
	command += " " + entryPoint;
	command += " " + target;
	command += (" /Fo") + tempFilename;	

#ifdef _DEBUG

	command += " /Zi /Od";

#else

	command += " /O3";

#endif

	auto result = system(command.c_str());

	return tempFilename;
}

auto getBytecode(std::string filename) -> std::vector<char>
{
	std::ifstream file(filename, std::ios::binary | std::ios::ate);
	auto fileLength = static_cast<int>(file.tellg());
	std::vector<char> bytecode(fileLength);
	file.seekg(0, std::ios::beg);
	file.read(bytecode.data(), fileLength);

	return bytecode;
}

auto getFormat(D3D11_SIGNATURE_PARAMETER_DESC description) -> DXGI_FORMAT
{
	switch (description.Mask)
	{
	case 1:

		switch (description.ComponentType)
		{
		case D3D_REGISTER_COMPONENT_FLOAT32:

			return DXGI_FORMAT_R32_FLOAT;

		case D3D_REGISTER_COMPONENT_SINT32:

			return DXGI_FORMAT_R32_SINT;

		case D3D_REGISTER_COMPONENT_UINT32:

			return DXGI_FORMAT_R32_UINT;
		
		}

	case 3:

		switch (description.ComponentType)
		{
		case D3D_REGISTER_COMPONENT_FLOAT32:

			return DXGI_FORMAT_R32G32_FLOAT;

		case D3D_REGISTER_COMPONENT_SINT32:

			return DXGI_FORMAT_R32G32_SINT;

		case D3D_REGISTER_COMPONENT_UINT32:

			return DXGI_FORMAT_R32G32_UINT;

		}

	case 7:
		switch (description.ComponentType)
		{
		case D3D_REGISTER_COMPONENT_FLOAT32:

			return DXGI_FORMAT_R32G32B32_FLOAT;

		case D3D_REGISTER_COMPONENT_SINT32:

			return DXGI_FORMAT_R32G32B32_SINT;

		case D3D_REGISTER_COMPONENT_UINT32:

			return DXGI_FORMAT_R32G32B32_UINT;

		}
	case 15:
		switch (description.ComponentType)
		{
		case D3D_REGISTER_COMPONENT_FLOAT32:

			return DXGI_FORMAT_R32G32B32A32_FLOAT;

		case D3D_REGISTER_COMPONENT_SINT32:

			return DXGI_FORMAT_R32G32B32A32_SINT;

		case D3D_REGISTER_COMPONENT_UINT32:

			return DXGI_FORMAT_R32G32B32A32_UINT;

		}
	}

	return DXGI_FORMAT_UNKNOWN;
}

const int SHDR = 
	('S' )
	| ('H' << 8)
	| ('D' << 16)
	| ('R' << 24);

const int SHADER_IMPORTER_VERSION = 2;

/*
header:
[SHDR]
[input layout start - 4 bytes]
[constant buffer start - 4 bytes]
[shader bytecode start - 4 bytes]
[shader bytecode length - 4 bytes]

constant buffers :

[buffer count - 4 byte]
buffer :
	[name - 4 bytes length + length bytes]
	   [size - 4 bytes]
	   [type - 4 bytes] (constant buffer/structured buffer/...)
	   [slot - 4 bytes]

   variables :
	[count - 4 bytes]
	[name - 4 bytes length + length bytes]
	[startOffset - 4 bytes]
	[frequency - 4 bytes]



[compiled vertex shader - shader bytecode length bytes]]

[optional data]

input layout :

[input parameter count - 4 byte]
input parameter :
[semantic name - 4 bytes for string length + length bytes for characters]
[semantic index - 4 byte]
[format - 4 byte]
[inputSlot - 4 byte]
[slotClass - 4 byte]
[stepRate - 4 byte]



*/

struct Buffer
{
	ID3D11ShaderReflectionConstantBuffer* buffer;
	D3D11_SHADER_BUFFER_DESC description;
	D3D11_SHADER_INPUT_BIND_DESC bind;
};

struct Buffers
{
	std::vector<Buffer> constantBuffers;
};

auto getBuffers(D3D11_SHADER_DESC description, ID3D11ShaderReflection* reflection) -> Buffers
{
	Buffers buffers;

	for (unsigned int i = 0; i < description.ConstantBuffers; i++)
	{
		ID3D11ShaderReflectionConstantBuffer* buffer;
		buffer = reflection->GetConstantBufferByIndex(i);

		D3D11_SHADER_BUFFER_DESC bufferDescription;
		buffer->GetDesc(&bufferDescription);

		D3D11_SHADER_INPUT_BIND_DESC bind;
		reflection->GetResourceBindingDescByName(bufferDescription.Name, &bind);

		switch (bind.Type)
		{
			case D3D_SIT_CBUFFER
				:
			{
				buffers.constantBuffers.push_back(Buffer{ buffer, bufferDescription, bind });
				break;
			}			
		}
	}

	return buffers;
}

auto importShader(std::string inputFilename, std::string outputFilename,
	std::string entryPoint, std::string target) -> void
{
	auto vertexTempOutput = runFxc(inputFilename, entryPoint, target);
	auto bytecode = getBytecode(vertexTempOutput);
	
	DeleteFile(vertexTempOutput.c_str());
	
	ID3D11ShaderReflection* reflection;
	D3DReflect(bytecode.data(), bytecode.size(), IID_ID3D11ShaderReflection, (void**)(&reflection));

	D3D11_SHADER_DESC description;
	reflection->GetDesc(&description);

	unsigned int constantBufferHeaderStart = 16;
	unsigned int bytecodeStart = 0;
	unsigned int bytecodeLength = 0;

	std::ofstream vertexShader(outputFilename, std::ios::binary | std::ios::out);
	vertexShader.write(reinterpret_cast<const char*>(&SHDR), 4);
	vertexShader.write(reinterpret_cast<const char*>(&SHADER_IMPORTER_VERSION), 4);
	vertexShader.write(reinterpret_cast<const char*>(&constantBufferHeaderStart), 4);
	vertexShader.write(reinterpret_cast<const char*>(&bytecodeStart), 4);
	vertexShader.write(reinterpret_cast<const char*>(&bytecodeLength), 4);

	unsigned int constantBufferHeaderSizeInBytes = 0;

	auto buffers = getBuffers(description, reflection);

	int constantBufferCount = buffers.constantBuffers.size();
	vertexShader.write(reinterpret_cast<const char*>(&constantBufferCount), 4);
	constantBufferHeaderSizeInBytes += 4;

	for (auto& constantBuffer : buffers.constantBuffers)
	{
		auto bufferDescription = constantBuffer.description;

		auto bufferNameLength = strlen(bufferDescription.Name);

		vertexShader.write(reinterpret_cast<const char*>(&bufferNameLength), 4);
		constantBufferHeaderSizeInBytes += 4;

		vertexShader.write(bufferDescription.Name, bufferNameLength);
		constantBufferHeaderSizeInBytes += bufferNameLength;

		vertexShader.write(reinterpret_cast<const char*>(&bufferDescription.Size), 4);
		constantBufferHeaderSizeInBytes += 4;

		auto bind = constantBuffer.bind;

		vertexShader.write(reinterpret_cast<const char*>(&bind.Type), 4);
		constantBufferHeaderSizeInBytes += 4;

		vertexShader.write(reinterpret_cast<const char*>(&bind.BindPoint), 4);
		constantBufferHeaderSizeInBytes += 4;

		vertexShader.write(reinterpret_cast<const char*>(&bufferDescription.Variables), 4);
		constantBufferHeaderSizeInBytes += 4;

		auto buffer = constantBuffer.buffer;

		for (unsigned int j = 0; j < bufferDescription.Variables; j++)
		{
			auto variable = buffer->GetVariableByIndex(j);

			D3D11_SHADER_VARIABLE_DESC variableDesc;
			variable->GetDesc(&variableDesc);

			auto nameLength = strlen(variableDesc.Name);

			vertexShader.write(reinterpret_cast<const char*>(&nameLength), 4);
			constantBufferHeaderSizeInBytes += 4;

			vertexShader.write(variableDesc.Name, nameLength);
			constantBufferHeaderSizeInBytes += nameLength;

			vertexShader.write(reinterpret_cast<const char*>(&variableDesc.StartOffset), 4);
			constantBufferHeaderSizeInBytes += 4;

		}
	}

	bytecodeStart = static_cast<unsigned int>(vertexShader.tellp());
	bytecodeLength = bytecode.size();

	vertexShader.seekp(12, std::ios::beg);
	vertexShader.write(reinterpret_cast<const char*>(&bytecodeStart), 4);
	vertexShader.write(reinterpret_cast<const char*>(&bytecodeLength), 4);
	vertexShader.seekp(bytecodeStart, std::ios::beg);

	vertexShader.write(bytecode.data(), bytecode.size());

	if (entryPoint == "/Emain_vertex")
	{
		//write the input layout at the end of the file

		vertexShader.write(reinterpret_cast<const char*>(&description.InputParameters), 4);
		
		for (unsigned int i = 0; i < description.InputParameters; i++)
		{
			D3D11_SIGNATURE_PARAMETER_DESC parameter;
			reflection->GetInputParameterDesc(i, &parameter);

			int semanticNameLength = strlen(parameter.SemanticName);
			vertexShader.write(reinterpret_cast<const char*>(&semanticNameLength), 4);
			vertexShader.write(parameter.SemanticName, semanticNameLength);
			vertexShader.write(reinterpret_cast<const char*>(&parameter.SemanticIndex), 4);
			
			auto format = getFormat(parameter);
			vertexShader.write(reinterpret_cast<const char*>(&format), 4);
			/*
			input slots aren't written. input layouts will need both a mesh and a shader in order 
			to create a layout. first you find a match of semantic names with mesh and shader, then
			you take the input slots from the mesh, the bytecode from the shader, combine it all to 
			get your layout.
			*/
		}
	}
}

int main(int argc, char** argv)
{	
	if (argc != 3)
	{
		std::cerr << "syntax: ShaderImporter {input fullpath} {output fullpath without extension}" << std::endl;
		return 1;
	}

	std::cout << "input: " << argv[1] << " output: " << argv[2] << std::endl;

	std::ifstream file(argv[1], std::ios::binary | std::ios::ate);
	auto fileLength = file.tellg();

	if (fileLength <= 0)
	{
		std::cerr << "unable to read file " << argv[1] << std::endl;
		return 1;
	}

	std::string shaderSource(static_cast<unsigned int>(fileLength), '0');

	file.seekg(0, std::ios::beg);
	file.read(&shaderSource[0], fileLength);
	file.close();
	
	if (shaderSource.find("main_vertex") != std::string::npos)
	{
		importShader(argv[1], std::string(argv[2]) + ".cvs", "/Emain_vertex", "/Tvs_5_0");
	}

	if (shaderSource.find("main_geometry") != std::string::npos)
	{
		importShader(argv[1], std::string(argv[2]) + ".cgs", "/Emain_geometry", "/Tgs_5_0");
	}

	if (shaderSource.find("main_pixel") != std::string::npos)
	{
		importShader(argv[1], std::string(argv[2]) + ".cps", "/Emain_pixel", "/Tps_5_0");
	}	

	return 0;
}


