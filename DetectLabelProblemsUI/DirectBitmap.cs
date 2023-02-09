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

        public DirectBitmap AdjustBrightness()
        {
            DirectBitmap dbAdjusted = new DirectBitmap(Width, Height);
            double[] dSumBrightness = new double[4];
            int cPixels = (Width / 4) * (Height / 4);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Color c = GetPixel(x, y);
                    float fBrightness = c.GetBrightness();
                    if (x < Width * .25 && y < Height * .25)
                        dSumBrightness[0] += fBrightness;
                    if (x < Width * .25 && y > Height * .75)
                        dSumBrightness[1] += fBrightness;
                    if (x > Width * .75 && y < Height * .25)
                        dSumBrightness[2] += fBrightness;
                    if (x > Width * .75 && y > Height * .75)
                        dSumBrightness[3] += fBrightness;

                }
            }

            int iMaxIndex = 0;
            double dMax = 0.0;
            double dMin = 1.0;
            for (int i = 0; i < 4; i++)
            {

                dSumBrightness[i] /= cPixels;
                if(dSumBrightness[i] > dMax)
                {
                    dMax= dSumBrightness[i];
                    iMaxIndex = i;
                }
                if (dSumBrightness[i] < dMin)
                    dMin = dSumBrightness[i];
            }
            Point pOrigin = new Point(0,0);
            if(iMaxIndex == 0)
                pOrigin = new Point(0, 0);
            if (iMaxIndex == 1)
                pOrigin = new Point(0, Height);
            if (iMaxIndex == 2)
                pOrigin = new Point(Width, 0);
            if (iMaxIndex == 3)
                pOrigin = new Point(Width, Height);

            double dMaxDistance = Math.Sqrt(Width * Width + Height * Height);
            double fMaxDecrease = 0.3;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Point pCurrent = new Point(x, y);
                    double dDistanceFromOrigin = distance(pOrigin, pCurrent);
                    float fDecrease = (float)(fMaxDecrease * (1.0 - (dDistanceFromOrigin / dMaxDistance)));
                    Color c = GetPixel(x, y);
                    
                    HSV hsv = new HSV();
                    hsv.h = c.GetHue();
                    hsv.s = c.GetSaturation();
                    hsv.v = c.GetBrightness() - fDecrease;
                    Color cAdjusted = ColorFromHSL(hsv);
                    dbAdjusted.SetPixel(x, y, cAdjusted);
                    
                }
            }
            return dbAdjusted;
        }

        private double distance(Point pOrigin, Point pCurrent)
        {
            double dX = pOrigin.X - pCurrent.X;
            double dY = pOrigin.Y - pCurrent.Y;
            return Math.Sqrt(dX * dX + dY * dY);
        }

        Color SetHue(Color oldColor, float fNewHue)
        {
            var temp = new HSV();
            temp.h = fNewHue;
            //temp.h = oldColor.GetHue();
            temp.s = oldColor.GetSaturation();
            temp.v = oldColor.GetBrightness();
            //temp.v = getBrightness(oldColor);
            return ColorFromHSL(temp);
        }

        // A common triple float struct for both HSL & HSV
        // Actually this should be immutable and have a nice constructor!!
        public struct HSV { public float h; public float s; public float v; }

        // the Color Converter
        static public Color ColorFromHSL(HSV hsl)
        {
            if (hsl.s == 0)
            { 
                int L = (int)hsl.v; 
                return Color.FromArgb(255, L, L, L); 
            }
            if (hsl.v < 0)
                hsl.v = 0;

            double min, max, h;
            h = hsl.h / 360d;

            max = hsl.v < 0.5d ? hsl.v * (1 + hsl.s) : (hsl.v + hsl.s) - (hsl.v * hsl.s);
            min = (hsl.v * 2d) - max;

            Color c = Color.FromArgb(255, (int)(255 * RGBChannelFromHue(min, max, h + 1 / 3d)),
                                          (int)(255 * RGBChannelFromHue(min, max, h)),
                                          (int)(255 * RGBChannelFromHue(min, max, h - 1 / 3d)));
            return c;
        }

        static double RGBChannelFromHue(double m1, double m2, double h)
        {
            h = (h + 1d) % 1d;
            if (h < 0) h += 1;
            if (h * 6 < 1) return m1 + (m2 - m1) * 6 * h;
            else if (h * 2 < 1) return m2;
            else if (h * 3 < 2) return m1 + (m2 - m1) * 6 * (2d / 3d - h);
            else return m1;

        }

        float getBrightness(Color c)
        {
            return (c.R * 0.299f + c.G * 0.587f + c.B * 0.114f) / 256f;
        }

    }
}
