

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
	std::string fxc = "\"C:\\Program Files (x86)\\Microsoft DirectX SDK (June 2010)\\Utilities\\bin\\x64\\fxc.exe\" ";
	std::string command = fxc;
	std::string tempFilename = getTempFilename();

	command += shader;
	command += " " + entryPoint;
	command += " " + target;
	command += (" /Fo") + tempFilename;

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
	case 0x1:

		switch (description.ComponentType)
		{
		case D3D_REGISTER_COMPONENT_FLOAT32:

			return DXGI_FORMAT_R32_FLOAT;

		case D3D_REGISTER_COMPONENT_SINT32:

			return DXGI_FORMAT_R32_SINT;

		case D3D_REGISTER_COMPONENT_UINT32:

			return DXGI_FORMAT_R32_UINT;
		
		}

	case 0x11:

		switch (description.ComponentType)
		{
		case D3D_REGISTER_COMPONENT_FLOAT32:

			return DXGI_FORMAT_R32G32_FLOAT;

		case D3D_REGISTER_COMPONENT_SINT32:

			return DXGI_FORMAT_R32G32_SINT;

		case D3D_REGISTER_COMPONENT_UINT32:

			return DXGI_FORMAT_R32G32_UINT;

		}

	case 0x111:
		switch (description.ComponentType)
		{
		case D3D_REGISTER_COMPONENT_FLOAT32:

			return DXGI_FORMAT_R32G32B32_FLOAT;

		case D3D_REGISTER_COMPONENT_SINT32:

			return DXGI_FORMAT_R32G32B32_SINT;

		case D3D_REGISTER_COMPONENT_UINT32:

			return DXGI_FORMAT_R32G32B32_UINT;

		}
	case 0x1111:
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
}

const int SHDR = 
	('S' )
	| ('H' << 8)
	| ('D' << 16)
	| ('R' << 24);

const int SHADER_IMPORTER_VERSION = 1;

/*
header:
[SHDR]
[version 1]
[input layout start - 4 bytes]
[constant buffer start - 4 bytes]
[shader bytecode start - 4 bytes]

input layout :

[input parameter count - 4 byte]
input parameter :
[semantic name - 4 bytes for string length + length bytes for characters]
[semantic index - 4 byte]
[format - 4 byte]
[inputSlot - 4 byte]
[slotClass - 4 byte]
[stepRate - 4 byte]

constant buffers :

[buffer count - 4 byte]
buffer :
	[name - 4 bytes length + length bytes]
	   [size - 4 bytes]
	   [slot - 4 bytes]

   variables :
	[count - 4 bytes]
	[name - 4 bytes length + length bytes]
	[startOffset - 4 bytes]
	[frequency - 4 bytes]



[compiled vertex shader - rest of file]

*/

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

	unsigned int constantBufferHeaderStart = 8;
	unsigned int bytecodeStart = 0;

	std::ofstream vertexShader(outputFilename, std::ios::binary | std::ios::out);
	vertexShader.write(reinterpret_cast<const char*>(&SHDR), 4);
	vertexShader.write(reinterpret_cast<const char*>(&SHADER_IMPORTER_VERSION), 4);
	vertexShader.write(reinterpret_cast<const char*>(&constantBufferHeaderStart), 4);
	vertexShader.write(reinterpret_cast<const char*>(&bytecodeStart), 4);

	unsigned int constantBufferHeaderSizeInBytes = 0;

	vertexShader.write(reinterpret_cast<const char*>(&description.ConstantBuffers), 4);
	constantBufferHeaderSizeInBytes += 4;

	for (unsigned int i = 0; i < description.ConstantBuffers; i++)
	{
		ID3D11ShaderReflectionConstantBuffer* buffer;
		buffer = reflection->GetConstantBufferByIndex(i);

		D3D11_SHADER_BUFFER_DESC bufferDescription;
		buffer->GetDesc(&bufferDescription);

		auto bufferNameLength = strlen(bufferDescription.Name);
		
		vertexShader.write(reinterpret_cast<const char*>(&bufferNameLength), 4);
		constantBufferHeaderSizeInBytes += 4;

		vertexShader.write(bufferDescription.Name, bufferNameLength);
		constantBufferHeaderSizeInBytes += bufferNameLength;

		vertexShader.write(reinterpret_cast<const char*>(&bufferDescription.Size), 4);
		constantBufferHeaderSizeInBytes += 4;

		D3D11_SHADER_INPUT_BIND_DESC bind;
		reflection->GetResourceBindingDescByName(bufferDescription.Name, &bind);

		vertexShader.write(reinterpret_cast<const char*>(&bind.BindPoint), 4);
		constantBufferHeaderSizeInBytes += 4;

		vertexShader.write(reinterpret_cast<const char*>(&bufferDescription.Variables), 4);
		constantBufferHeaderSizeInBytes += 4;

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

	vertexShader.seekp(constantBufferHeaderStart + 4, std::ios::beg);
	vertexShader.write(reinterpret_cast<const char*>(&bytecodeStart), 4);
	vertexShader.seekp(bytecodeStart, std::ios::beg);

	vertexShader.write(bytecode.data(), bytecode.size());
}

int main(int argc, char** argv)
{	
	if (argc != 3)
	{
		std::cerr << "must supply shader filename" << std::endl;
		return 1;
	}

	std::cout << "input: " << argv[1] << " output: " << argv[2] << std::endl;

	std::ifstream file(argv[1], std::ios::binary | std::ios::ate);
	auto fileLength = file.tellg();

	std::string shaderSource(fileLength, '0');

	file.seekg(0, std::ios::beg);
	file.read(&shaderSource[0], fileLength);
	file.close();
	
	if (shaderSource.find("main_vertex") != std::string::npos)
	{
		importShader(argv[1], std::string(argv[2]) + ".cvs", "/Emain_vertex", "/Tvs_4_0");
	}

	if (shaderSource.find("main_pixel") != std::string::npos)
	{
		importShader(argv[1], std::string(argv[2]) + ".cps", "/Emain_pixel", "/Tps_4_0");
	}	

	return 0;
}


