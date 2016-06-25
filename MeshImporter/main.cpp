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
#include <vector>
#include <fstream>
#include <assimp/Importer.hpp>
#include <assimp/scene.h>
#include <assimp/postprocess.h>

/*
mesh format:

[MESH - 4 bytes]
[version - 4 bytes]
[vertex count - 4 bytes]
[index count - 4 bytes]

axis aligned bounding box:
[min - xyz float - 12 bytes]
[max - xyz float - 12 bytes]

input layout:
[element count - 4 bytes]
	element:
	[name - x bytes]
	[format - 4 bytes]
	[slot - 4 bytes]

vertex streams:
[count - 4 bytes]

stream:
	[type - 4 bytes] {vertex, index, normal, texture coords }
	[element size - 4 bytes]
	[stream size - 4 bytes]
	[stream data - stream size bytes]


new in version 4:
-normal stream also contains tangents

*/
const int POSITION_SIZE = 16;
const int INDEX_SIZE = 4;
const int NORMAL_SIZE = 12;
const int TANGENT_SIZE = 12;
const int TEXCOORD_SIZE = 8;
const int NORMALTEXCOORD_SIZE = NORMAL_SIZE + TANGENT_SIZE + TANGENT_SIZE + TEXCOORD_SIZE;

const int MESH =
	('M')
	| ('E' << 8)
	| ('S' << 16)
	| ('H' << 24);

const int MESH_IMPORTER_VERSION = 4;

enum class StreamType {
	Vertex = 0,
	Index,
	NormalTexCoord
};

aiVector3D min(const aiVector3D& left, const aiVector3D& right)
{
	return aiVector3D(std::min(left.x, right.x), std::min(left.y, right.y), std::min(left.z, right.z));
}

aiVector3D max(const aiVector3D& left, const aiVector3D& right)
{
	return aiVector3D(std::max(left.x, right.x), std::max(left.y, right.y), std::max(left.z, right.z));
}

int main(int argc, char** argv)
{
	if (argc != 3)
	{
		std::cerr << "syntax: MeshImporter {input fullpath} {output fullpath}" << std::endl;
		return 1;
	}

	Assimp::Importer importer;

	auto flags = 
		aiProcess_CalcTangentSpace
		| aiProcess_JoinIdenticalVertices
		| aiProcess_ConvertToLeftHanded
		;

	auto scene = importer.ReadFile(argv[1], flags);

	if (!scene)
	{
		std::cerr << "failed to read mesh" << std::endl;
		return 1;
	}

	auto mesh = scene->mMeshes[0];

	int streamCount = 2;
	bool hasExtraStream = mesh->HasNormals() && mesh->HasTextureCoords(0) && mesh->HasTangentsAndBitangents();

	if (hasExtraStream)
	{
		streamCount++;
	}
	
	std::ofstream output(argv[2], std::ios::binary | std::ios::out);

	if (!output)
	{
		std::cerr << "can't open output file" << std::endl;
	}

	output.write(reinterpret_cast<const char*>(&MESH), 4);
	output.write(reinterpret_cast<const char*>(&MESH_IMPORTER_VERSION), 4);

	output.write(reinterpret_cast<const char*>(&mesh->mNumVertices), 4);
	int indexCount = mesh->mNumFaces * 3;
	output.write(reinterpret_cast < const char*>(&indexCount), 4);

	aiVector3D min, max;
	for (unsigned int i = 0; i < mesh->mNumVertices; i++)
	{
		min = ::min(min, mesh->mVertices[i]);
		max = ::max(max, mesh->mVertices[i]);
	}

	output.write(reinterpret_cast<const char*>(&min), 12);
	output.write(reinterpret_cast<const char*>(&max), 12);

	/*input layout :
	[element count - 4 bytes]
		element :
			[name - x bytes]
			[format - 4 bytes]
			[slot - 4 bytes]*/
	int elementCount = 1;
	
	if (hasExtraStream)
	{
		elementCount += 4;
	}

	const char* semanticNames[] = { "POSITION", "NORMAL", "TANGENT", "BITANGENT", "TEXCOORDS" };
	const int DXGI_FORMAT_R32G32B32A32_FLOAT = 2;
	const int DXGI_FORMAT_R32G32B32_FLOAT = 6;
	const int DXGI_FORMAT_R32G32_FLOAT = 16;
	const int formats[] = { DXGI_FORMAT_R32G32B32A32_FLOAT, DXGI_FORMAT_R32G32B32_FLOAT, DXGI_FORMAT_R32G32B32_FLOAT, DXGI_FORMAT_R32G32B32_FLOAT, DXGI_FORMAT_R32G32_FLOAT };
	const int slots[] = { 0, 1, 1, 1, 1}; //normal and texcoord share the same slot

	output.write(reinterpret_cast<const char*>(&elementCount), 4);

	for (int i = 0; i < elementCount; i++)
	{
		auto stringLength = strlen(semanticNames[i]);
		output.write(reinterpret_cast<const char*>(&stringLength), 4);
		output.write(semanticNames[i], stringLength);
		output.write(reinterpret_cast<const char*>(&formats[i]), 4);
		output.write(reinterpret_cast<const char*>(&slots[i]), 4);
	}

	output.write(reinterpret_cast<const char*>(&streamCount), 4);

	StreamType streamType{ StreamType::Vertex };
	int elementSize = POSITION_SIZE;
	int streamSize = elementSize * mesh->mNumVertices;

	//position stream
	output.write(reinterpret_cast<const char*>(&streamType), 4);
	output.write(reinterpret_cast<const char*>(&elementSize), 4);
	output.write(reinterpret_cast<const char*>(&streamSize), 4);

	float wCoordinate = 1;
	for (unsigned int i = 0; i < mesh->mNumVertices; i++)
	{
		output.write(reinterpret_cast<const char*>(&mesh->mVertices[i]), 12);
		output.write(reinterpret_cast<const char*>(&wCoordinate), 4);
	}

	//index stream
	elementSize = INDEX_SIZE;
	streamSize = elementSize * mesh->mNumFaces * 3;
	streamType = StreamType::Index;

	output.write(reinterpret_cast<const char*>(&streamType), 4);
	output.write(reinterpret_cast<const char*>(&elementSize), 4);
	output.write(reinterpret_cast<const char*>(&streamSize), 4);

	std::vector<unsigned int> indexData;
	for (unsigned int i = 0; i < mesh->mNumFaces; i++)
	{
		auto& face = mesh->mFaces[i];
		indexData.push_back(face.mIndices[0]);
		indexData.push_back(face.mIndices[1]);
		indexData.push_back(face.mIndices[2]);
	}

	output.write(reinterpret_cast<const char*>(indexData.data()), streamSize);

	//extra streams
	if (hasExtraStream)
	{
		elementSize = NORMALTEXCOORD_SIZE;
		streamSize = elementSize * mesh->mNumVertices;
		streamType = StreamType::NormalTexCoord;

		output.write(reinterpret_cast<const char*>(&streamType), 4);
		output.write(reinterpret_cast<const char*>(&elementSize), 4);
		output.write(reinterpret_cast<const char*>(&streamSize), 4);

		std::vector<unsigned char> extraStream(streamSize);

		unsigned int offset = 0;
		for (unsigned int i = 0; i < mesh->mNumVertices; i++)
		{

			memcpy(extraStream.data() + offset, &mesh->mNormals[i], 12);
			memcpy(extraStream.data() + offset + 12, &mesh->mTangents[i], 12);
			memcpy(extraStream.data() + offset + 24, &mesh->mBitangents[i], 12);
			memcpy(extraStream.data() + offset + 36, &mesh->mTextureCoords[0][i], 8);

			offset += 44;
		}

		output.write(reinterpret_cast<const char*>(extraStream.data()), streamSize);
	}
}
