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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FontImporter
{
    public class GlyphTextureCoordinates
    {
        public Tuple<float, float> TopLeft { get; set; }
        public Tuple<float, float> TopRight { get; set; }
        public Tuple<float, float> BottomLeft { get; set; }
        public Tuple<float, float> BottomRight { get; set; }
    }

    /*
    imported format:

    [font - 70 79 78 84 - 4 bytes]
    [format version - 4 bytes]

    [glyph count - 4 bytes]
     
    glyphs:
    [glyph - 2 bytes]
    [topleft - 4 + 4 bytes]
    [topright - 4 + 4 bytes]
    [bottomleft - 4 + 4 bytes]
    [bottomright - 4 + 4 bytes]
    
     
    [implicit width count - same as glyph count]
    widths:
     [glyph - 2 bytes]
     [a - 4 bytes]
     [b - 4 bytes]
     [c - 4 bytes]

    remainder: signed distance field png

    */

    public class FontImporter
    {
        const int version = 2;

        const int width = 4096;
        const int height = 4096;

        int maxGlyphWidth = 256;
        int maxGlyphHeight = 256;

        const int GRID_SIZE = 256;

        const int GROUP_SIZE = 64;
        const int BLOCK_SIZE = 32;
        const int CHUNK_SIZE = 16;

        const int GROUPS_PER_GRID = GRID_SIZE / GROUP_SIZE;
        const int BLOCKS_PER_GROUP = GROUP_SIZE / BLOCK_SIZE;
        const int BLOCKS_PER_GRID = GRID_SIZE / BLOCK_SIZE;
        const int CHUNKS_PER_BLOCK = BLOCK_SIZE / CHUNK_SIZE;
        const int CHUNKS_PER_GROUP = GROUP_SIZE / CHUNK_SIZE;
        const int CHUNKS_PER_GRID = GRID_SIZE / CHUNK_SIZE;

        const float MAX_DISTANCE = 50;

        const int firstCharacter = 32;
        const int lastCharacter = 127;

        Bitmap bitmap;
        Graphics graphics;

        Font font;
        Brush brush;

        byte[,] pixels = new byte[width, height];

        [DllImport("gdi32.dll")]
        private static extern bool GetCharABCWidthsFloat(IntPtr hdc, uint iFirstChar, uint iLastChar, [Out] CharacterWidths[] lpABCF);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiObj);

        [DllImport("gdi32.dll")]
        private static extern int GetKerningPairs(IntPtr hdc, int numPairs, [In, Out] KerningPair[] pairs);

        [DllImport("gdi32.dll")]
        private static extern int SetMapMode(IntPtr hdc, int mapMode);

        public FontImporter()
        {
            bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            graphics = Graphics.FromImage(bitmap);
            brush = Brushes.White;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;           
        }

        Dictionary<char, GlyphTextureCoordinates> textureCoordinates = new Dictionary<char, GlyphTextureCoordinates>();

        struct CharacterWidths
        {
            public float a, b, c;
        }

        Dictionary<char, CharacterWidths> characterWidths = new Dictionary<char, CharacterWidths>();

        [StructLayout(LayoutKind.Explicit)]
        struct KerningPair
        {
            [FieldOffset(0)]
            ushort first;
            [FieldOffset(2)]
            ushort second;
            [FieldOffset(4)]
            int amount;
        }

        void ssssss()
        {
            var widths = new CharacterWidths[lastCharacter - firstCharacter + 1];
            var hdc = graphics.GetHdc();

            var so = SelectObject(hdc, font.ToHfont());
            SetMapMode(hdc, 1);

            GetCharABCWidthsFloat(hdc, firstCharacter, lastCharacter, widths);

            for (int glyphIndex = firstCharacter; glyphIndex < lastCharacter; glyphIndex++)
            {
                characterWidths[(char)glyphIndex] = widths[glyphIndex - firstCharacter];
            }

            var num = GetKerningPairs(hdc, Int32.MaxValue, null);

            var pairs = new KerningPair[num];

            GetKerningPairs(hdc, num, pairs);

            SelectObject(hdc, so);

            graphics.ReleaseHdc();
        }

        private Bitmap generate()
        {
            float cursorPositionX = 0;
            float cursorPositionY = 0;

            var glyphSizes = new Dictionary<string, SizeF>();
            graphics.FillRectangle(Brushes.Black, 0, 0, width, height);

            //calculate glyph dimensions, find the max glyph height
            for (int glyphIndex = firstCharacter; glyphIndex < lastCharacter; glyphIndex++)
            {
                var glyph = Char.ToString((char)glyphIndex);

                if (Char.IsControl((char)glyphIndex))
                    continue;

                var size = graphics.MeasureString(glyph, font);

                glyphSizes.Add(glyph, size);
            }

            ssssss();

            //draw all of the glyphs into a grid
            foreach (var kvp in glyphSizes)
            {
                var glyph = kvp.Key;
                var size = kvp.Value;

                var characterWidth = characterWidths[glyph[0]];

                if (cursorPositionX + maxGlyphWidth > width)
                {
                    cursorPositionX = 0;
                    cursorPositionY += maxGlyphHeight;
                }

                cursorPositionX += characterWidth.a;

                var texCoords = new GlyphTextureCoordinates()
                {
                    TopLeft = Tuple.Create(cursorPositionX / width, cursorPositionY / height),
                    TopRight = Tuple.Create((cursorPositionX + characterWidth.b) / width, cursorPositionY / height),
                    BottomLeft = Tuple.Create(cursorPositionX / width, (cursorPositionY + maxGlyphHeight) / height),
                    BottomRight = Tuple.Create((cursorPositionX + characterWidth.b) / width, (cursorPositionY + maxGlyphHeight) / height)
                };

                textureCoordinates.Add(glyph[0], texCoords);

                graphics.DrawString(glyph, font, brush, cursorPositionX, cursorPositionY);

                cursorPositionX += characterWidth.b;
                cursorPositionX += (maxGlyphWidth - characterWidth.a - characterWidth.b);
            }

            bitmap.Save("glyphs.png", ImageFormat.Png);
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    pixels[x, y] = bitmap.GetPixel(x, y).R;
                }
            }          

            chunkify();

            Parallel.For(0, height, y => calculateSignedDistanceField(y));
            return finish();
        }

        enum ChunkStatus
        {
            AllIn,
            AllOut,
            Mixed
        }

        ChunkStatus[,] chunkUniformity = new ChunkStatus[width / CHUNK_SIZE, height / CHUNK_SIZE];
        ChunkStatus[,] blockUniformity = new ChunkStatus[width / BLOCK_SIZE, height / BLOCK_SIZE];
        ChunkStatus[,] groupUniformity = new ChunkStatus[width / GROUP_SIZE, height / GROUP_SIZE];

        void chunkify()
        {
            Bitmap chunk = new Bitmap(width, height);
            Bitmap block = new Bitmap(width, height);
            Bitmap group = new Bitmap(width, height);

            for (int gridX = 0; gridX < width / GRID_SIZE; gridX++)
            {
                for (int gridY = 0; gridY < height / GRID_SIZE; gridY++)
                {
                    for (int groupX = 0; groupX < GROUPS_PER_GRID; groupX++)
                    {
                        for (int groupY = 0; groupY < GROUPS_PER_GRID; groupY++)
                        {
                            int groupPositionX =
                                (gridX * GRID_SIZE)
                                 + (groupX * GROUP_SIZE);

                            int groupPositionY =
                                (gridY * GRID_SIZE)
                                + (groupY * GROUP_SIZE);

                            ChunkStatus groupStatus = pixels[groupPositionX, groupPositionY] == 0 ? ChunkStatus.AllIn : ChunkStatus.AllOut;

                            bool groupEarlyOut = false;

                            for (int blockX = 0; blockX < BLOCKS_PER_GROUP; blockX++)
                            {
                                for (int blockY = 0; blockY < BLOCKS_PER_GROUP; blockY++)
                                {
                                    int blockPositionX = groupPositionX
                                        + (blockX * BLOCK_SIZE);

                                    int blockPositionY = groupPositionY
                                            + (blockY * BLOCK_SIZE);

                                    ChunkStatus blockStatus = pixels[blockPositionX, blockPositionY] == 0 ? ChunkStatus.AllIn : ChunkStatus.AllOut;

                                    bool blockEarlyOut = false;

                                    for (int chunkX = 0; chunkX < CHUNKS_PER_BLOCK; chunkX++)
                                    {
                                        for (int chunkY = 0; chunkY < CHUNKS_PER_BLOCK; chunkY++)
                                        {
                                            int chunkPositionX = blockPositionX
                                                    + (chunkX * CHUNK_SIZE);

                                            int chunkPositionY = blockPositionY
                                                    + (chunkY * CHUNK_SIZE);

                                            ChunkStatus chunkStatus = pixels[chunkPositionX, chunkPositionY] == 0 ? ChunkStatus.AllIn : ChunkStatus.AllOut;

                                            bool chunkEarlyOut = false;

                                            for (int x = 0; x < CHUNK_SIZE; x++)
                                            {
                                                for (int y = 0; y < CHUNK_SIZE; y++)
                                                {
                                                    int posX = chunkPositionX + x;
                                                    int posY = chunkPositionY + y;

                                                    ChunkStatus pixelStatus = pixels[posX, posY] == 0 ? ChunkStatus.AllIn : ChunkStatus.AllOut;

                                                    if (pixelStatus != chunkStatus)
                                                    {
                                                        chunkStatus = ChunkStatus.Mixed;
                                                        chunkEarlyOut = true;
                                                    }
                                                }

                                            }

                                            int cx = (gridX * CHUNKS_PER_GRID) + (groupX * CHUNKS_PER_GROUP) + (blockX * CHUNKS_PER_BLOCK) + chunkX;
                                            int cy = (gridY * CHUNKS_PER_GRID) + (groupY * CHUNKS_PER_GROUP) + (blockY * CHUNKS_PER_BLOCK) + chunkY;

                                            chunkUniformity[cx, cy] = chunkStatus;

                                            if (chunkStatus != blockStatus || chunkEarlyOut)
                                            {
                                                blockStatus = ChunkStatus.Mixed;
                                                blockEarlyOut = true;
                                            }

                                        }

                                    }

                                    int bx = (gridX * BLOCKS_PER_GRID) + (groupX * BLOCKS_PER_GROUP) + (blockX);
                                    int by = (gridY * BLOCKS_PER_GRID) + (groupY * BLOCKS_PER_GROUP) + (blockY);

                                    blockUniformity[bx, by] = blockStatus;

                                    if (blockStatus != groupStatus || blockEarlyOut)
                                    {
                                        groupStatus = ChunkStatus.Mixed;
                                        groupEarlyOut = true;
                                    }
                                }

                            }

                            int gx = (gridX * GROUPS_PER_GRID) + groupX;
                            int gy = (gridY * GROUPS_PER_GRID) + groupY;
                            groupUniformity[gx, gy] = groupEarlyOut ? ChunkStatus.Mixed : groupStatus;
                        }
                    }
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var chunkStatus = chunkUniformity[x / CHUNK_SIZE, y / CHUNK_SIZE];
                    int status = chunkStatus == ChunkStatus.AllIn ? 0 : chunkStatus == ChunkStatus.AllOut ? 255 : 125;

                    chunk.SetPixel(x, y, Color.FromArgb(status, status, status));

                    var blockStatus = blockUniformity[x / BLOCK_SIZE, y / BLOCK_SIZE];
                    status = blockStatus == ChunkStatus.AllIn ? 0 : blockStatus == ChunkStatus.AllOut ? 255 : 125;

                    block.SetPixel(x, y, Color.FromArgb(status, status, status));

                    var groupStatus = groupUniformity[x / GROUP_SIZE, y / GROUP_SIZE];
                    status = groupStatus == ChunkStatus.AllIn ? 0 : groupStatus == ChunkStatus.AllOut ? 255 : 125;

                    group.SetPixel(x, y, Color.FromArgb(status, status, status));
                }
            }
        }

        float calcQuadTree(int x, int y)
        {
            int gridX = x / maxGlyphWidth;
            int gridY = y / maxGlyphHeight;

            float closestDistance = float.MaxValue;

            var myPixel = pixels[x, y];

            int gridBoundaryX = Math.Min(gridX * maxGlyphWidth + maxGlyphWidth, width);
            int gridBoundaryY = Math.Min(gridY * maxGlyphHeight + maxGlyphHeight, height);

            ChunkStatus pixelStatus = myPixel == 0 ? ChunkStatus.AllIn : ChunkStatus.AllOut;

            for (int groupX = 0; groupX < GROUPS_PER_GRID; groupX++)
            {
                for (int groupY = 0; groupY < GROUPS_PER_GRID; groupY++)
                {
                    var groupStatus = groupUniformity[(gridX * GROUPS_PER_GRID) + groupX, (gridY * GROUPS_PER_GRID) + groupY];

                    if (groupStatus != pixelStatus)
                    {
                        for (int blockX = 0; blockX < BLOCKS_PER_GROUP; blockX++)
                        {
                            for (int blockY = 0; blockY < BLOCKS_PER_GROUP; blockY++)
                            {
                                int bx = (gridX * BLOCKS_PER_GRID) + (groupX * BLOCKS_PER_GROUP) + (blockX);
                                int by = (gridY * BLOCKS_PER_GRID) + (groupY * BLOCKS_PER_GROUP) + (blockY);

                                var blockStatus = blockUniformity[bx, by];

                                if (blockStatus != pixelStatus)
                                {
                                    for (int chunkX = 0; chunkX < CHUNKS_PER_BLOCK; chunkX++)
                                    {
                                        for (int chunkY = 0; chunkY < CHUNKS_PER_BLOCK; chunkY++)
                                        {
                                            int cx = (gridX * CHUNKS_PER_GRID) + (groupX * CHUNKS_PER_GROUP) + (blockX * CHUNKS_PER_BLOCK) + chunkX;
                                            int cy = (gridY * CHUNKS_PER_GRID) + (groupY * CHUNKS_PER_GROUP) + (blockY * CHUNKS_PER_BLOCK) + chunkY;

                                            var chunkStatus = chunkUniformity[cx, cy];

                                            if (chunkStatus != pixelStatus)
                                            {
                                                for (int px = 0; px < CHUNK_SIZE; px++)
                                                {
                                                    for (int py = 0; py < CHUNK_SIZE; py++)
                                                    {
                                                        int pixelLocationX = (gridX * GRID_SIZE) + (groupX * GROUP_SIZE) + (blockX * BLOCK_SIZE) + (chunkX * CHUNK_SIZE) + px;
                                                        int pixelLocationY = (gridY * GRID_SIZE) + (groupY * GROUP_SIZE) + (blockY * BLOCK_SIZE) + (chunkY * CHUNK_SIZE) + py;

                                                        var neighbourPixel = pixels[pixelLocationX, pixelLocationY];

                                                        if (myPixel != neighbourPixel)
                                                        {
                                                            int dx = pixelLocationX - x;
                                                            int dy = pixelLocationY - y;

                                                            int distance = dx * dx + dy * dy;

                                                            closestDistance = Math.Min(closestDistance, distance);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return closestDistance;
        }

        float[,] signedDistances = new float[width, height];

        void calculateSignedDistanceField(int y)
        {
            for (int x = 0; x < width; x++)
            {
                signedDistances[x, y] = calcQuadTree(x, y);
            }

            if (y % 10 == 0)
            {
                
            }
        }

        Bitmap finish()
        {
            var distanceBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            var normalizationFactor = (float)Math.Sqrt((maxGlyphWidth * maxGlyphWidth) + (maxGlyphHeight * maxGlyphHeight));

            float maxNegative = 0;
            float maxPositive = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var distance = signedDistances[x, y];

                    if (distance == float.MaxValue)
                    {
                        distance = 1;
                    }
                    else
                    {
                        distance = (float)Math.Sqrt(distance);
                    }

                    if (pixels[x, y] == 0)
                    {
                        distance = -distance;
                        maxNegative = Math.Min(maxNegative, distance);
                    }
                    else
                    {
                        maxPositive = Math.Max(maxPositive, distance);
                    }

                    signedDistances[x, y] = distance;
                }
            }

            //normalization
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var distance = signedDistances[x, y];

                    if (distance > MAX_DISTANCE)
                    {
                        distance = MAX_DISTANCE;
                    }
                    else if (distance < -MAX_DISTANCE)
                    {
                        distance = -MAX_DISTANCE;
                    }

                    float normalized = 0;
                    normalized = distance / MAX_DISTANCE;
                    normalized = normalized * 0.5f + 0.5f;
                  
                    var channel = (int)Math.Round(normalized * 255);
                    var colour = Color.FromArgb(255, channel, channel, channel);

                    distanceBitmap.SetPixel(x, y, colour);
                }
            }

            distanceBitmap.Save("4096.png", ImageFormat.Png);

            var downsampled2048 = downsample(distanceBitmap, 2048, 2048);

            downsampled2048.Save("2048.png", ImageFormat.Png);

            var downsampled1024 = downsample(downsampled2048, 1024, 1024);

            downsampled1024.Save("1024.png", ImageFormat.Png);

            return downsampled1024;
        }

        private Bitmap downsample(Bitmap input, int w, int h)
        {
            Bitmap output;

            output = new Bitmap(w, h);

            for (int x = 0; x < w * 2; x += 2)
            {
                for (int y = 0; y < h * 2; y += 2)
                {
                    int a = input.GetPixel(x, y).R;
                    int b = input.GetPixel(x + 1, y).R;
                    int c = input.GetPixel(x, y + 1).R;
                    int d = input.GetPixel(x + 1, y + 1).R;

                    float average = a + b + c + d;
                    int result = (int)(average / 4.0f);

                    output.SetPixel(x / 2, y / 2, Color.FromArgb(255, result, result, result));
                }
            }

            return output;
        }

        public bool Import(string fontFamily, string outputFilename)
        {
            font = new Font(fontFamily, 150);

            var signedDistanceField = generate();

            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write((byte)70);
                    writer.Write((byte)79);
                    writer.Write((byte)78);
                    writer.Write((byte)84);

                    writer.Write(version);

                    writer.Write(textureCoordinates.Count);
                    int i = 0;
                    foreach (var kvp in textureCoordinates)
                    {
                        var p = stream.Position;
                        writer.Write(kvp.Key);
                        
                        writer.Write(kvp.Value.TopLeft.Item1);
                        writer.Write(kvp.Value.TopLeft.Item2);
                        writer.Write(kvp.Value.TopRight.Item1);
                        writer.Write(kvp.Value.TopRight.Item2);
                        writer.Write(kvp.Value.BottomLeft.Item1);
                        writer.Write(kvp.Value.BottomLeft.Item2);
                        writer.Write(kvp.Value.BottomRight.Item1);
                        writer.Write(kvp.Value.BottomRight.Item2);

                        i++;
                    }

                    foreach(var kvp in characterWidths)
                    {
                        writer.Write(kvp.Key);
                        writer.Write(kvp.Value.a);
                        writer.Write(kvp.Value.b);
                        writer.Write(kvp.Value.c);
                    }
                    
                    var data = signedDistanceField.LockBits(new Rectangle(0, 0, signedDistanceField.Width, signedDistanceField.Height), ImageLockMode.ReadOnly, signedDistanceField.PixelFormat);
                    var bytes = new byte[data.Stride * data.Height];
                    Marshal.Copy(data.Scan0, bytes, 0, data.Stride * data.Height);
                    signedDistanceField.UnlockBits(data);

                    writer.Write(bytes);

                    using (var filestream = File.Open(outputFilename, FileMode.Create))
                    {
                        stream.Position = 0;
                        stream.CopyTo(filestream);
                    }
                }
            }

            return true;
        }
    }

    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("syntax: FontImporter font-family {output fullpath}");
                return 1;
            }

            var importer = new FontImporter();
            importer.Import(args[0], args[1]);

            return 0;
        }
    }
}
