using Assets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		static readonly int version = 1;

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
						writeString(writer, texture.Source.ImportedFilename);
						writer.Write((byte)0); //todo: NOT IMPLEMENTED YET
					}

					writer.Write((int)asset.ParameterGroups.Count);

					foreach (var group in asset.ParameterGroups)
					{
						//writer.Write(group.Name);
						writeString(writer, group.Name);
						writer.Write((byte)group.Parameters.Count);

						foreach (var parameter in group.Parameters)
						{
							//writer.Write(parameter.Name);
							writeString(writer, parameter.Name);
							writer.Write((byte)parameter.PodType);
							writer.Write((byte)parameter.ParameterType);
							parameter.WriteoutValue(writer);
							
						}
					}
				}
			}

			asset.LastUpdated = DateTime.Now.ToString();
            asset.ImporterVersion = ImporterVersion;

			return true;
		}
	}
}
