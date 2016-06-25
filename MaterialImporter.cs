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
using System;
using System.IO;
using System.Text;

namespace Importers
{
    /*
	imported format:

	[mate - 77 65 84 69 - 4 bytes]
	[format version - 4 bytes]

	textures:
	[count - 1 byte]
	[texture binding - x bytes]
	[string filename - x bytes]
	[real or generated - 1 byte]          is this texture an offline asset, or one generated at runtime

	default parameter values:
	[parameter group count - 1 byte]

	parameter group:
		[string name - x bytes]
		[parameter count - 1 byte]

		parameter:
			[string name - x bytes]
			[type - 1 byte]  {0 = float, 1 = int, 2 = bool}
			[dimension - 1 byte] { 0 = scalar, 1 = vector, 2 = array}
			
			scalar?
				[value - 4 bytes]

			vector?
				[values - 16 bytes]

			array?

				[element count - 2 bytes]
				[elements - scalar: 4 bytes, vector: 16 bytes, array: 16 bytes * count]
	
	*/

    public class MaterialImporter
	{
		static readonly int version = 2;

        public static int ImporterVersion { get { return version; } }

		static char[] getAsciiString(string s)
		{
			Encoding ascii = Encoding.ASCII;
			Encoding unicode = Encoding.Unicode;

			byte[] unicodeBytes = unicode.GetBytes(s);

			byte[] asciiBytes = Encoding.Convert(unicode, ascii, unicodeBytes);

			char[] asciiChars = new char[ascii.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
			ascii.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);

			return asciiChars;
		}

		static void writeString(BinaryWriter writer, string s)
		{
			var ascii = getAsciiString(s);

			writer.Write(s.Length);
			writer.Write(ascii);
		}

		public static bool Import(MaterialAsset asset)
		{
			using (var stream = File.Open(asset.ImportedFilename, FileMode.Create))
			{
				using (var writer = new BinaryWriter(stream))
				{
					writer.Write((byte)77);
					writer.Write((byte)65);
					writer.Write((byte)84);
					writer.Write((byte)69);

					writer.Write(version);

					writer.Write((int)asset.Textures.Count);

					foreach (var texture in asset.Textures)
					{
						writeString(writer, texture.Binding);

                        if (texture.IsProcedural)
                        {
                            writeString(writer, texture.SourceId);
                        }
                        else
                        {
                            writeString(writer, texture.Source.ImportedFilename);
                        }
						
						writer.Write(Convert.ToByte(texture.IsProcedural));
					}

					writer.Write((int)asset.ParameterGroups.Count);

					foreach (var group in asset.ParameterGroups)
					{
						writeString(writer, group.Name);
						writer.Write((byte)group.Parameters.Count);

						foreach (var parameter in group.Parameters)
						{
							writeString(writer, parameter.Name);
							writer.Write((byte)parameter.PodType);
							writer.Write((byte)parameter.ParameterType);
							parameter.WriteoutValue(writer);
							
						}
					}
				}
			}

            asset.LastUpdated = DateTime.Now;
            asset.ImporterVersion = ImporterVersion;

			return true;
		}
	}
}
