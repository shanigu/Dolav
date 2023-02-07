using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DetectLabelProblems
{
    public class DirectBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public Int32[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        protected GCHandle BitsHandle { get; private set; }

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new Int32[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
        }


        public DirectBitmap(double width, double height) : this((int)width, (int)height)
        {

        }
        public DirectBitmap(Bitmap b)
        {
            Width = b.Width;
            Height = b.Height;
            Bits = new Int32[Width * Height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
            for(int x = 0; x < Width; x++)
                for(int y = 0; y < Height; y++)
                {
                    Color c = b.GetPixel(x, y);
                    SetPixel(x, y, c);
                }
        }
        public DirectBitmap(Bitmap b, double dResize)
        {
            Width = (int)(b.Width * dResize);
            Height = (int)(b.Height * dResize);
            Bits = new Int32[Width * Height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
            int iStep = b.Width / Width;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    int cR = 0, cG = 0, cB = 0;
                    for(int i = 0; i < iStep; i++)
                    {
                        for (int j = 0; j < iStep; j++)
                        {
                            Color c = b.GetPixel(x * iStep + i, y * iStep + j);
                            cR += c.R;
                            cG += c.G;
                            cB += c.B;
                        }
                    }
                    int cPixels = iStep * iStep;
                    Color cAvg = Color.FromArgb(cR / cPixels,cG / cPixels, cB / cPixels);
                    SetPixel(x, y, cAvg);
                }
            }
        }

        public void SetPixel(int x, int y, Color colour)
        {
            int index = x + (y * Width);
            int col = colour.ToArgb();

            Bits[index] = col;
        }

        public Color GetPixel(int x, int y)
        {
            int index = x + (y * Width);
            if (index < 0 || index >= Bits.Length)
                return Color.Black;
            int col = Bits[index];
            Color result = Color.FromArgb(col);

            return result;
        }

        

        public void SaveRect(int xStart, int yStart, int width, int height, string sFileName)
        {
            Bitmap bmp = new Bitmap(width, height);
            for(int x = 0; x < width;x++)
            {
                for(int y = 0; y < height;y++)
                {
                    Color c = GetPixel(xStart + x, yStart + y);
                    bmp.SetPixel(x, y, c);
                }
            }
            bmp.Save(sFileName);
            bmp.Dispose();

        }
        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }
    }
}
