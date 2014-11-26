using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageConverter
{
	enum PixelFormatFlags
	{
		ALPHA_PIXELS = 0x1,
		ALPHA = 0x2,
		FOUR_CC = 0x4,
		RGB = 0x40,
		YUV = 0x200,
        LUMINANCE = 0x20000,
	}

    enum HeaderFlags
    {
        CAPS = 0x1,
        HEIGHT = 0x2,
        WIDTH = 0x4,
        PITCH = 0x8,
        PIXELFORMAT = 0x1000,
        MIPMAPCOUNT = 0x20000,
        LINEARSIZE = 0x80000,
        DEPTH = 0x800000
    }

    enum Caps
    {
        COMPLEX = 0x8,
        MIPMAP = 0x400000,
        TEXTURE = 0x1000
    }

	//http://msdn.microsoft.com/en-us/library/windows/desktop/bb943984(v=vs.85).aspx
	class PixelFormat 
	{
        public uint size;
        public uint flags;
        public uint fourCC;
        public uint rgbBitCount;
        public uint rBitMask;
		public uint gBitMask;
		public uint bBitMask;
		public uint aBitMask;
	};

	//http://msdn.microsoft.com/en-us/library/windows/desktop/bb943982(v=vs.85).aspx
	class Header
	{
		public uint size;
        public uint flags;
        public uint height;
        public uint width;
        public uint pitchOrLinearSize;
        public uint depth;
		public uint mipMapCount;
		public uint reserved1; //write 11 uints
		public PixelFormat pixelFormat;
		public uint caps;
        public uint caps2;
        public uint caps3;
        public uint caps4;
        public uint reserved2;
	}

    //copied from http://msdn.microsoft.com/en-us/library/windows/desktop/bb173059(v=vs.85).aspx
    enum DXGI_FORMAT
    {
        DXGI_FORMAT_UNKNOWN = 0,
        DXGI_FORMAT_R32G32B32A32_TYPELESS = 1,
        DXGI_FORMAT_R32G32B32A32_FLOAT = 2,
        DXGI_FORMAT_R32G32B32A32_UINT = 3,
        DXGI_FORMAT_R32G32B32A32_SINT = 4,
        DXGI_FORMAT_R32G32B32_TYPELESS = 5,
        DXGI_FORMAT_R32G32B32_FLOAT = 6,
        DXGI_FORMAT_R32G32B32_UINT = 7,
        DXGI_FORMAT_R32G32B32_SINT = 8,
        DXGI_FORMAT_R16G16B16A16_TYPELESS = 9,
        DXGI_FORMAT_R16G16B16A16_FLOAT = 10,
        DXGI_FORMAT_R16G16B16A16_UNORM = 11,
        DXGI_FORMAT_R16G16B16A16_UINT = 12,
        DXGI_FORMAT_R16G16B16A16_SNORM = 13,
        DXGI_FORMAT_R16G16B16A16_SINT = 14,
        DXGI_FORMAT_R32G32_TYPELESS = 15,
        DXGI_FORMAT_R32G32_FLOAT = 16,
        DXGI_FORMAT_R32G32_UINT = 17,
        DXGI_FORMAT_R32G32_SINT = 18,
        DXGI_FORMAT_R32G8X24_TYPELESS = 19,
        DXGI_FORMAT_D32_FLOAT_S8X24_UINT = 20,
        DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS = 21,
        DXGI_FORMAT_X32_TYPELESS_G8X24_UINT = 22,
        DXGI_FORMAT_R10G10B10A2_TYPELESS = 23,
        DXGI_FORMAT_R10G10B10A2_UNORM = 24,
        DXGI_FORMAT_R10G10B10A2_UINT = 25,
        DXGI_FORMAT_R11G11B10_FLOAT = 26,
        DXGI_FORMAT_R8G8B8A8_TYPELESS = 27,
        DXGI_FORMAT_R8G8B8A8_UNORM = 28,
        DXGI_FORMAT_R8G8B8A8_UNORM_SRGB = 29,
        DXGI_FORMAT_R8G8B8A8_UINT = 30,
        DXGI_FORMAT_R8G8B8A8_SNORM = 31,
        DXGI_FORMAT_R8G8B8A8_SINT = 32,
        DXGI_FORMAT_R16G16_TYPELESS = 33,
        DXGI_FORMAT_R16G16_FLOAT = 34,
        DXGI_FORMAT_R16G16_UNORM = 35,
        DXGI_FORMAT_R16G16_UINT = 36,
        DXGI_FORMAT_R16G16_SNORM = 37,
        DXGI_FORMAT_R16G16_SINT = 38,
        DXGI_FORMAT_R32_TYPELESS = 39,
        DXGI_FORMAT_D32_FLOAT = 40,
        DXGI_FORMAT_R32_FLOAT = 41,
        DXGI_FORMAT_R32_UINT = 42,
        DXGI_FORMAT_R32_SINT = 43,
        DXGI_FORMAT_R24G8_TYPELESS = 44,
        DXGI_FORMAT_D24_UNORM_S8_UINT = 45,
        DXGI_FORMAT_R24_UNORM_X8_TYPELESS = 46,
        DXGI_FORMAT_X24_TYPELESS_G8_UINT = 47,
        DXGI_FORMAT_R8G8_TYPELESS = 48,
        DXGI_FORMAT_R8G8_UNORM = 49,
        DXGI_FORMAT_R8G8_UINT = 50,
        DXGI_FORMAT_R8G8_SNORM = 51,
        DXGI_FORMAT_R8G8_SINT = 52,
        DXGI_FORMAT_R16_TYPELESS = 53,
        DXGI_FORMAT_R16_FLOAT = 54,
        DXGI_FORMAT_D16_UNORM = 55,
        DXGI_FORMAT_R16_UNORM = 56,
        DXGI_FORMAT_R16_UINT = 57,
        DXGI_FORMAT_R16_SNORM = 58,
        DXGI_FORMAT_R16_SINT = 59,
        DXGI_FORMAT_R8_TYPELESS = 60,
        DXGI_FORMAT_R8_UNORM = 61,
        DXGI_FORMAT_R8_UINT = 62,
        DXGI_FORMAT_R8_SNORM = 63,
        DXGI_FORMAT_R8_SINT = 64,
        DXGI_FORMAT_A8_UNORM = 65,
        DXGI_FORMAT_R1_UNORM = 66,
        DXGI_FORMAT_R9G9B9E5_SHAREDEXP = 67,
        DXGI_FORMAT_R8G8_B8G8_UNORM = 68,
        DXGI_FORMAT_G8R8_G8B8_UNORM = 69,
        DXGI_FORMAT_BC1_TYPELESS = 70,
        DXGI_FORMAT_BC1_UNORM = 71,
        DXGI_FORMAT_BC1_UNORM_SRGB = 72,
        DXGI_FORMAT_BC2_TYPELESS = 73,
        DXGI_FORMAT_BC2_UNORM = 74,
        DXGI_FORMAT_BC2_UNORM_SRGB = 75,
        DXGI_FORMAT_BC3_TYPELESS = 76,
        DXGI_FORMAT_BC3_UNORM = 77,
        DXGI_FORMAT_BC3_UNORM_SRGB = 78,
        DXGI_FORMAT_BC4_TYPELESS = 79,
        DXGI_FORMAT_BC4_UNORM = 80,
        DXGI_FORMAT_BC4_SNORM = 81,
        DXGI_FORMAT_BC5_TYPELESS = 82,
        DXGI_FORMAT_BC5_UNORM = 83,
        DXGI_FORMAT_BC5_SNORM = 84,
        DXGI_FORMAT_B5G6R5_UNORM = 85,
        DXGI_FORMAT_B5G5R5A1_UNORM = 86,
        DXGI_FORMAT_B8G8R8A8_UNORM = 87,
        DXGI_FORMAT_B8G8R8X8_UNORM = 88,
        DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM = 89,
        DXGI_FORMAT_B8G8R8A8_TYPELESS = 90,
        DXGI_FORMAT_B8G8R8A8_UNORM_SRGB = 91,
        DXGI_FORMAT_B8G8R8X8_TYPELESS = 92,
        DXGI_FORMAT_B8G8R8X8_UNORM_SRGB = 93,
        DXGI_FORMAT_BC6H_TYPELESS = 94,
        DXGI_FORMAT_BC6H_UF16 = 95,
        DXGI_FORMAT_BC6H_SF16 = 96,
        DXGI_FORMAT_BC7_TYPELESS = 97,
        DXGI_FORMAT_BC7_UNORM = 98,
        DXGI_FORMAT_BC7_UNORM_SRGB = 99,
        DXGI_FORMAT_AYUV = 100,
        DXGI_FORMAT_Y410 = 101,
        DXGI_FORMAT_Y416 = 102,
        DXGI_FORMAT_NV12 = 103,
        DXGI_FORMAT_P010 = 104,
        DXGI_FORMAT_P016 = 105,
        DXGI_FORMAT_420_OPAQUE = 106,
        DXGI_FORMAT_YUY2 = 107,
        DXGI_FORMAT_Y210 = 108,
        DXGI_FORMAT_Y216 = 109,
        DXGI_FORMAT_NV11 = 110,
        DXGI_FORMAT_AI44 = 111,
        DXGI_FORMAT_IA44 = 112,
        DXGI_FORMAT_P8 = 113,
        DXGI_FORMAT_A8P8 = 114,
        DXGI_FORMAT_B4G4R4A4_UNORM = 115,
        //DXGI_FORMAT_FORCE_UINT = 0xffffffffUL
    }

    enum D3D10_RESOURCE_DIMENSION
    {
        D3D10_RESOURCE_DIMENSION_UNKNOWN = 0,
        D3D10_RESOURCE_DIMENSION_BUFFER = 1,
        D3D10_RESOURCE_DIMENSION_TEXTURE1D = 2,
        D3D10_RESOURCE_DIMENSION_TEXTURE2D = 3,
        D3D10_RESOURCE_DIMENSION_TEXTURE3D = 4
    }

    class HeaderDXT10
    {
        public int dxgiFormat {get; set;}
        public int resourceDimension { get; set; }
        public uint miscFlag { get; set; }
        public uint arraySize { get; set; }
        public uint miscFlags2 { get; set; }
    }
	
	class Image
	{
		public byte[] bytes {get; set;}
		public System.Windows.Media.PixelFormat format {get; set;}
        public int width {get; set;}
        public int height {get; set;}
        public int pitch {get; set;}
	}

	class Program
	{   

		static Image loadImage(string filename)
		{
			using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
				var source = decoder.Frames[0];
				var width = source.PixelWidth;
				var height = source.PixelHeight;
                var pitch = (source.Format.BitsPerPixel + 7) / 8;
				var stride = width * pitch;

				var bytes = new byte[height * stride];
				source.CopyPixels(bytes, stride, 0);

                Image image = new Image();

				image.bytes = bytes;
                image.format = source.Format;
                image.width = source.PixelWidth;
                image.height = source.PixelHeight;
                image.pitch = pitch;                

                return image;
			}
		}
        
        static int getDXGIFormat(System.Windows.Media.PixelFormat format)
        {
            //(int)DXGI_FORMAT.DXGI_FORMAT_R8_UNORM;
            //bgr24, bgra32, rbga64
            if (format == PixelFormats.Bgra32)
            {
                return (int)DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM_SRGB;
            }
            else if (format == PixelFormats.Bgr24)
            {
                return (int)DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM_SRGB;
            }
            else if (format == PixelFormats.Rgba64)
            {
                return (int)DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_FLOAT;
            }
            else if (format == PixelFormats.Gray8)
            {
                return (int)DXGI_FORMAT.DXGI_FORMAT_R8_UNORM;
            }
            else
            {
                throw new Exception("unknown format");
            }
        }

		static int Main(string[] args)
		{
            if (args.Length != 2)
            {
	            Console.WriteLine("syntax: ImageConverter {input fullpath} {output fullpath}");
                return 1;  
            }

            var filename = args[0]; //@"c:\ProjectStacks\RawAssets\Textures\Roughness\first.png";

            var image = loadImage(filename);
			
			var outputName = Path.GetFileNameWithoutExtension(filename);

			var textureType = new DirectoryInfo(filename).Parent.Name;
            var output = args[1]; //@"C:\ProjectStacks\ImportedAssets\Textures\" + textureType + @"\" + outputName + ".dds";

			const int DDS_MAGIC_NUMBER = 0x20534444;

			Header header = new Header();
            header.size = 124;
			header.flags = (uint)(HeaderFlags.CAPS | HeaderFlags.WIDTH | HeaderFlags.HEIGHT | HeaderFlags.PIXELFORMAT);
            header.width = (uint)image.width;
            header.height = (uint)image.height;
            header.pitchOrLinearSize = (uint)image.pitch;
            
            header.pixelFormat = new PixelFormat();
            header.pixelFormat.size = 32;
            header.pixelFormat.flags = (uint)PixelFormatFlags.FOUR_CC;
            header.pixelFormat.fourCC = ('D') | ('X' << 8) | ('1' << 16) | ('0' << 24);
            header.pixelFormat.rgbBitCount = (uint)image.format.BitsPerPixel;            

            header.caps = (uint)Caps.TEXTURE;
            header.caps2 = 0;
            header.caps3 = 0;
            header.caps4 = 0;

            HeaderDXT10 dxtHeader = new HeaderDXT10();
            dxtHeader.dxgiFormat = getDXGIFormat(image.format); 
            dxtHeader.resourceDimension = (int)D3D10_RESOURCE_DIMENSION.D3D10_RESOURCE_DIMENSION_TEXTURE2D;
            dxtHeader.miscFlag = 0;
            dxtHeader.arraySize = 1;
            dxtHeader.miscFlags2 = 0;

            var buffer = new byte[image.bytes.Length + 4 + 124 + 20];

			using (var stream = new MemoryStream(buffer))
			{
				using (var writer = new BinaryWriter(stream))
				{
					writer.Write(DDS_MAGIC_NUMBER);

                    writer.Write(header.size);
                    writer.Write(header.flags);
                    writer.Write(header.height);
                    writer.Write(header.width);
                    writer.Write(header.pitchOrLinearSize);
                    writer.Write(header.depth);
                    writer.Write(header.mipMapCount);

                    //write out dwReserved1[11]
                    for (int i = 0; i < 11; i++)
                    {                        
                        writer.Write((uint)0);
                    }

                    writer.Write(header.pixelFormat.size);
                    writer.Write(header.pixelFormat.flags);
                    writer.Write(header.pixelFormat.fourCC);
                    writer.Write(header.pixelFormat.rgbBitCount);
                    writer.Write(header.pixelFormat.rBitMask);
                    writer.Write(header.pixelFormat.gBitMask);
                    writer.Write(header.pixelFormat.bBitMask);
                    writer.Write(header.pixelFormat.aBitMask);

                    writer.Write(header.caps);
                    writer.Write(header.caps2);
                    writer.Write(header.caps3);
                    writer.Write(header.caps4);                    
                    writer.Write(header.reserved2);

                    writer.Write(dxtHeader.dxgiFormat);
                    writer.Write(dxtHeader.resourceDimension);
                    writer.Write(dxtHeader.miscFlag);
                    writer.Write(dxtHeader.arraySize);
                    writer.Write(dxtHeader.miscFlags2);

                    writer.Write(image.bytes);                                       
				}               
			}

            File.WriteAllBytes(output, buffer);

            return 0;
		}        
	}
}
