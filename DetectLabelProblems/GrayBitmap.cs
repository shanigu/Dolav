using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace DetectLabelProblems
{
    public class GrayBitmap
    {
        private int[,] Colors;
        public int Width, Height;
        public HashSet<Point> OriginalBlackPoints;

        public static int NEUTRAL = 100;

        public GrayBitmap(int iWidth, int iHeight)
        {
            Init(iWidth, iHeight);
        }
        public GrayBitmap()
        {

        }

        public GrayBitmap(DirectBitmap db, int xStart, int yStart, int xEnd, int yEnd)
        {
            Init(xEnd - xStart, yEnd - yStart);
            //BinarizeHSVOutliers(db, 0.9, true, false, false, xStart, yStart);
            ScaledGray(db, xStart, yStart, 1);
        }
        public GrayBitmap(DirectBitmap db) : this(db, 0, 0, db.Width, db.Height)
        {

        }
        public GrayBitmap(DirectBitmap db, Rectangle r) :
            this(db, r.X, r.Y, r.X + r.Width, r.Y + r.Height)
        {
            
        }
        private void ScaledGray(DirectBitmap db, int xStart, int yStart, int iGranularity)
        {
            for(int x = 0; x < Width; x++)
            {
                for(int y = 0; y < Height; y++)
                {
                    Color c = db.GetPixel(x + xStart, y + yStart);
                    //int iGray = (int)(255 * c.GetHue() / 360);
                    //int iGray = (int)(255 * c.GetSaturation());
                    //int iGray = (int)(255 * c.GetBrightness());
                    int iGray = (c.R + c.G + c.B) / 3;
                    int iScale = iGranularity * (iGray / iGranularity);
                    Colors[x, y] = iScale;
                    OriginalBlackPoints.Add(new Point(x, y));
                }
            }
        }

        private void Init(int iWidth, int iHeight)
        {
            Width = iWidth;
            Height = iHeight;
            Colors = new int[iWidth, iHeight];
            OriginalBlackPoints = new HashSet<Point>();
        }
        public void SetPixel(int x, int y, int c)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                Colors[x, y] = c;

        }
        public int GetPixel(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                return Colors[x, y];
            return -1;
        }

        public void Save(string sFileName)
        {
            try
            {
                using (Bitmap bmp = new Bitmap(Width, Height))
                {
                    for (int x = 0; x < Width; x++)
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            int c = Colors[x, y];
                            bmp.SetPixel(x, y, Color.FromArgb(c, c, c));
                        }
                    }
                    //bmp.Save(sFileName, ImageFormat.Bmp);
                    bmp.Save(sFileName, ImageFormat.Jpeg);
                    bmp.Dispose();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Save failed: " + e.Message);
            }
        }


        public GrayBitmap FilterSmallObjects(int iMinSize)
        {
            List<HashSet<Point>> lObjectsFilter = ObjectDetection(true);
            GrayBitmap gbFitlered = new GrayBitmap(Width, Height);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    gbFitlered.SetPixel(x, y, 255);
                }
            }
            gbFitlered.OriginalBlackPoints = new HashSet<Point>();
            foreach (HashSet<Point> lObject in lObjectsFilter)
            {
                if (lObject.Count > iMinSize)
                {
                    foreach (Point p in lObject)
                    {
                        gbFitlered.SetPixel(p, 0);
                        gbFitlered.OriginalBlackPoints.Add(p);
                    }
                }
            }
            return gbFitlered;

        }

        public GrayBitmap CloseHoles()
        {
            GrayBitmap gb = new GrayBitmap(Width, Height);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    gb.SetPixel(x, y, GetPixel(x, y));
                }
            }
            gb.OriginalBlackPoints = new HashSet<Point>(OriginalBlackPoints);
            List<HashSet<Point>> lHoles = ObjectDetection(false);
            foreach (HashSet<Point> lHole in lHoles)
            {
                //if (lHole.Count > 100)
                //    Console.Write("*");
                if (lHole.Count < 100000)
                {
                    foreach (Point p in lHole)
                    {
                        gb.SetPixel(p, 0);
                        gb.OriginalBlackPoints.Add(p);
                    }
                }
            }
            return gb;
        }

        public List<HashSet<Point>> ObjectDetection(bool bBlack)
        {
            Console.WriteLine("ObjectDetection");
            DateTime dtStart = DateTime.Now;

            int[,] aOriginal = new int[Width, Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    aOriginal[x, y] = Colors[x, y];
            HashSet<Point> lBlackPoints = new HashSet<Point>(OriginalBlackPoints);



            List<HashSet<Point>> lObjects = new List<HashSet<Point>>();
            int iSize = 1;
            int cObjects = 0;
            for (int x = 10; x < Width - 10; x++)
            {
                Console.Write("\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b" + x);
                for (int y = 10; y < Height - 10; y++)
                {
                    int c = GetPixel(x, y);
                    if ((bBlack && c == 0) || (!bBlack && c == 255))
                    {
                        cObjects++;
                        HashSet<Point> hsObject = new HashSet<Point>();
                        Point p = new Point(x, y);
                        hsObject.Add(p);
                        Queue<Point> points = new Queue<Point>();
                        points.Enqueue(new Point(x, y));
                        while (points.Count > 0)
                        {
                            p = points.Dequeue();
                            for (int dx = -iSize; dx <= iSize; dx++)
                            {
                                for (int dy = -iSize; dy <= iSize; dy++)
                                {
                                    Point pTag = new Point(p.X + dx, p.Y + dy);
                                    int cTag = GetPixel(pTag);
                                    if ((bBlack && cTag == 0) || (!bBlack && cTag == 255))
                                    {
                                        if (!hsObject.Contains(pTag))
                                        {
                                            SetPixel(pTag, cObjects);
                                            hsObject.Add(pTag);
                                            points.Enqueue(pTag);
                                        }
                                    }
                                }
                            }
                        }
                        lObjects.Add(hsObject);
                    }
                }
            }

            Colors = aOriginal;
            OriginalBlackPoints = lBlackPoints;

            Console.WriteLine();
            DateTime dtEnd = DateTime.Now;
            Console.WriteLine("Time: " + (dtEnd - dtStart).TotalSeconds);

            return lObjects;
        }

        /*
                public List<HashSet<Point>> ObjectDetection()
                {
                    Console.WriteLine("ObjectDetection");
                    DateTime dtStart = DateTime.Now;

                    List<HashSet<Point>> lObjects = new List<HashSet<Point>>();
                    int iSize = 1;
                    int cObjects = 0;
                    for (int x = 10; x < Width - 10; x++)
                    {
                        Console.Write("\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b" + x);
                        for (int y = 10; y < Height - 10; y++)
                        {
                            int c = GetPixel(x, y);
                            if (c == 0)
                            {
                                cObjects++;
                                HashSet<Point> hsObject = new HashSet<Point>();
                                Point p = new Point(x, y);
                                hsObject.Add(p);
                                Queue<Point> points = new Queue<Point>();
                                points.Enqueue(new Point(x, y));
                                while (points.Count > 0)
                                {
                                    p = points.Dequeue();
                                    for (int dx = -iSize; dx <= iSize; dx++)
                                    {
                                        for (int dy = -iSize; dy <= iSize; dy++)
                                        {
                                            Point pTag = new Point(p.X + dx, p.Y + dy);
                                            int cTag = GetPixel(pTag);
                                            if (cTag == 0)
                                            {
                                                if (!hsObject.Contains(pTag))
                                                {
                                                    SetPixel(pTag, cObjects);
                                                    hsObject.Add(pTag);
                                                    points.Enqueue(pTag);
                                                }
                                            }
                                        }
                                    }
                                }
                                lObjects.Add(hsObject);
                            }

                        }
                    }
                    Console.WriteLine();
                    DateTime dtEnd = DateTime.Now;
                    Console.WriteLine("Time: " + (dtEnd - dtStart).TotalSeconds);

                    return lObjects;
                }
        */
        public void SetPixel(Point pTag, int color)
        {
            SetPixel(pTag.X, pTag.Y, color);
        }

        public int GetPixel(Point pTag)
        {
            return GetPixel(pTag.X, pTag.Y);
        }


        public (int, int) GetMinMax(int xStart, int xEnd, int yStart, int yEnd, bool bBlackWhite)
        {
            int iMax = 0, iMin = int.MaxValue;
            for (int x = xStart; x <= xEnd; x++)
            {
                for (int y = yStart; y <= yEnd; y++)
                {
                    int iGray = GetPixel(x, y);
                    if (iGray != -1)
                    {
                        if (iGray > iMax)
                        {
                            iMax = iGray;
                        }
                        if (iGray < iMin)
                        {
                            iMin = iGray;
                        }
                    }
                    if (bBlackWhite)
                    {
                        if (iMin == 0 && iMax == 255)
                            return (iMin, iMax);
                    }
                }
            }
            return (iMin, iMax);
        }

        public int GetMax(int xStart, int xEnd, int yStart, int yEnd)
        {
            int iMax = 0;
            for (int x = xStart; x <= xEnd; x++)
            {
                for (int y = yStart; y <= yEnd; y++)
                {
                    int iGray = GetPixel(x, y);
                    if (iGray != -1)
                    {
                        if (iGray > iMax)
                        {
                            iMax = iGray;
                        }

                    }
                    if (iMax == 255)
                        return iMax;

                }
            }
            return iMax;
        }

        public static GrayBitmap DilateEfficient(GrayBitmap gb, int iSize)
        {
            return gb.DilateEfficient(iSize);
        }

        public GrayBitmap DilateEfficient(int iSize)
        {
            Console.WriteLine("DilateEfficient " + iSize);
            DateTime dtStart = DateTime.Now;
            GrayBitmap dbRevised = new GrayBitmap(Width, Height);
            dbRevised.OriginalBlackPoints = new HashSet<Point>();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    dbRevised.SetPixel(x, y, 255);
                }
            }


            int cPoints = 0;
            foreach (Point p in OriginalBlackPoints)
            {
                if (cPoints % 100000 == 0)
                    Console.Write("\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b" + cPoints + " / " + OriginalBlackPoints.Count);

                cPoints++;


                for (int x = -iSize; x < iSize; x++)
                {
                    for (int y = -iSize; y < iSize; y++)
                    {
                        Point pTag = new Point(p.X + x, p.Y + y);
                        dbRevised.SetPixel(pTag.X, pTag.Y, 0);
                        dbRevised.OriginalBlackPoints.Add(pTag);
                    }
                }
            }


            Console.WriteLine();
            DateTime dtEnd = DateTime.Now;
            Console.WriteLine("Time: " + (dtEnd - dtStart).TotalSeconds);
            return dbRevised;
        }




        //BUGBUG: not working - does not close holes
        public GrayBitmap ErodeDilateEfficient(int iSize, bool bMin, bool bBlackWhite = true)
        {
            Console.WriteLine("ErodeDilate " + iSize + ", " + bMin);
            DateTime dtStart = DateTime.Now;
            GrayBitmap dbRevised = new GrayBitmap(Width, Height);
            dbRevised.OriginalBlackPoints = new HashSet<Point>();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    dbRevised.SetPixel(x, y, 255);
                }
            }


            int cPoints = 0;
            foreach (Point p in OriginalBlackPoints)
            {
                if (cPoints % 100000 == 0)
                    Console.Write("\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b" + cPoints + " / " + OriginalBlackPoints.Count);

                cPoints++;

                int iMax = 0, iMin = int.MaxValue;
                (iMin, iMax) = GetMinMax(p.X - iSize, p.X + iSize, p.Y - iSize, p.Y + iSize, bBlackWhite);

                if (bMin)
                {
                    dbRevised.SetPixel(p, iMin);
                }
                else
                {
                    dbRevised.SetPixel(p, iMax);
                }
            }


            Console.WriteLine();
            DateTime dtEnd = DateTime.Now;
            Console.WriteLine("Time: " + (dtEnd - dtStart).TotalSeconds);
            return dbRevised;
        }



        public GrayBitmap ObjectBasedErosion()
        {
            Console.WriteLine("ObjectBasedErosion ");
            DateTime dtStart = DateTime.Now;
            GrayBitmap dbRevised = new GrayBitmap(Width, Height);
            dbRevised.OriginalBlackPoints = new HashSet<Point>();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    dbRevised.SetPixel(x, y, 255);
                }
            }

            int cPoints = 0;
            List<HashSet<Point>> lObjects = ObjectDetection(true);
            foreach (HashSet<Point> lObject in lObjects)
            {
                int iSize = 0;
                if (lObject.Count > 2000)
                    iSize = 1;
                if (lObject.Count > 5000)
                    iSize = 4;
                if (lObject.Count > 10000)
                    iSize = 10;

                foreach (Point p in lObject)
                {
                    if (cPoints % 100000 == 0)
                        Console.Write("\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b" + cPoints + " / " + OriginalBlackPoints.Count);

                    cPoints++;

                    int iMax = GetMax(p.X - iSize, p.X + iSize, p.Y - iSize, p.Y + iSize);

                    if (iMax == 0)
                    {
                        dbRevised.OriginalBlackPoints.Add(p);
                        dbRevised.SetPixel(p, iMax);
                    }
                }
            }


            Console.WriteLine();
            DateTime dtEnd = DateTime.Now;
            Console.WriteLine("Time: " + (dtEnd - dtStart).TotalSeconds);
            return dbRevised;
        }



        public GrayBitmap ErodeEfficient(int iSize)
        {
            Console.WriteLine("ErodeEfficient " + iSize);
            DateTime dtStart = DateTime.Now;
            GrayBitmap dbRevised = new GrayBitmap(Width, Height);
            dbRevised.OriginalBlackPoints = new HashSet<Point>();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    dbRevised.SetPixel(x, y, 255);
                }
            }


            int cPoints = 0;
            foreach (Point p in OriginalBlackPoints)
            {
                if (cPoints % 100000 == 0)
                    Console.Write("\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b" + cPoints + " / " + OriginalBlackPoints.Count);

                cPoints++;

                int iMax = GetMax(p.X - iSize, p.X + iSize, p.Y - iSize, p.Y + iSize);

                if (iMax == 0)
                {
                    dbRevised.OriginalBlackPoints.Add(p);
                    dbRevised.SetPixel(p, iMax);
                }
            }


            Console.WriteLine();
            DateTime dtEnd = DateTime.Now;
            Console.WriteLine("Time: " + (dtEnd - dtStart).TotalSeconds);
            return dbRevised;
        }



        public GrayBitmap ErodeDilate(int iSize, bool bMin, bool bBlackWhite = true)
        {
            Console.WriteLine("ErodeDilate " + iSize + ", " + bMin);
            DateTime dtStart = DateTime.Now;
            GrayBitmap dbRevised = new GrayBitmap(Width, Height);
            dbRevised.OriginalBlackPoints = OriginalBlackPoints;

            for (int x = 0; x < Width; x++)
            {
                Console.Write("\b\b\b\b\b\b\b\b\b\b\b\b\b\b" + x);
                for (int y = 0; y < Height; y++)
                {
                    int iMax = 0, iMin = int.MaxValue;
                    (iMin, iMax) = GetMinMax(x - iSize, x + iSize, y - iSize, y + iSize, bBlackWhite);
                    if (bMin)
                    {
                        dbRevised.SetPixel(x, y, iMin);
                    }
                    else
                    {
                        dbRevised.SetPixel(x, y, iMax);
                    }
                }
            }
            Console.WriteLine();
            DateTime dtEnd = DateTime.Now;
            Console.WriteLine("Time: " + (dtEnd - dtStart).TotalSeconds);
            return dbRevised;
        }



        public Color[,] Convolution(DirectBitmap db, double[,] kernel)
        {
            Color[,] blurred = new Color[db.Width, db.Height];
            int iSize = kernel.GetLength(0);
            for (int x = 0; x < db.Width; x++)
            {
                for (int y = 0; y < db.Height; y++)
                {
                    double iSumR = 0, iSumG = 0, iSumB = 0;
                    int cPixels = 0;
                    for (int i = 0; i < iSize; i++)
                    {
                        for (int j = 0; j < iSize; j++)
                        {

                            Color c = db.GetPixel(x + i - iSize / 2, y + j - iSize / 2);
                            if (c.R + c.G + c.B < 255 * 3)
                            {
                                cPixels++;
                                iSumR += c.R * kernel[i, j];
                                iSumG += c.G * kernel[i, j];
                                iSumB += c.B * kernel[i, j];
                            }
                        }
                    }


                    Color cNew = Color.FromArgb(ToColor(iSumR), ToColor(iSumG), ToColor(iSumB));
                    blurred[x, y] = cNew;
                }
            }
            return blurred;
        }

        private int ToColor(double d)
        {
            if (d > 255)
                return 255;
            if (d < 0)
                return 0;
            return (int)d;
        }

        public Color[,] Blur(DirectBitmap db, int iSize)
        {
            Color[,] blurred = new Color[db.Width, db.Height];
            for (int x = 0; x < db.Width; x++)
            {
                for (int y = 0; y < db.Height; y++)
                {
                    int iSumR = 0, iSumG = 0, iSumB = 0;
                    int cPixels = 0;
                    for (int i = -iSize; i <= iSize; i++)
                    {
                        for (int j = -iSize; j <= iSize; j++)
                        {

                            Color c = db.GetPixel(x + i, y + j);
                            if (c.R + c.G + c.B < 255 * 3)
                            {
                                cPixels++;
                                iSumR += c.R;
                                iSumG += c.G;
                                iSumB += c.B;
                            }
                        }
                    }
                    Color cNew = Color.FromArgb(iSumR / cPixels, iSumG / cPixels, iSumB / cPixels);
                    blurred[x, y] = cNew;
                }
            }
            return blurred;
        }
        public void Binarize(DirectBitmap db, Func<Color, bool> Accept)
        {
            Console.WriteLine("Binarizing");
            double[,] sharpen = new double[,] { { 1 } };
            //double[,] sharpen = new double[,] { { -1, -1, -1 }, { -1, 9, -1 }, { -1, -1, -1 } };
            //double d = 1.0 / 9 ;
            //double[,] sharpen = new double[,] { { d, d, d }, { d, d, d }, { d, d, d } };
            Color[,] blurred = Convolution(db, sharpen);
            //Color[,] blurred = Blur(db, 5);
            OriginalBlackPoints = new HashSet<Point>();
            DateTime dtStart = DateTime.Now;
            Init(db.Width, db.Height);
            for (int x = 0; x < db.Width; x++)
            {
                for (int y = 0; y < db.Height; y++)
                {
                    Color c = blurred[x, y];
                    bool b = Accept(c);
                    if (b)
                        SetPixel(x, y, 255);
                    else
                    {
                        SetPixel(x, y, 0);
                        OriginalBlackPoints.Add(new Point(x, y));
                    }
                }
            }
            DateTime dtEnd = DateTime.Now;
            Console.WriteLine("Time: " + (dtEnd - dtStart).TotalSeconds);

        }


        private (float, float) GetRange(Dictionary<float, int> d, int cPixels, double dBackgroundPortion)
        {
            double cSum = 0;
            List<float> lKeys = new List<float>(d.Keys);
            int iMax = 0;
            float fMax = 0;
            for (int i = 0; i < lKeys.Count; i++)
            {
                float f = d[lKeys[i]];
                if (f > fMax)
                {
                    fMax = f;
                    iMax = i;
                }
            }
            int iLow = iMax, iHigh = iMax;
            cSum = fMax;
            while(cSum < cPixels * dBackgroundPortion)
            {
                float fLow = 0;
                if (iLow > 0)
                    fLow = d[lKeys[iLow - 1]];
                float fHigh = 0;
                if(iHigh < lKeys.Count - 1)
                    fHigh = d[lKeys[iHigh + 1]];
                if(fLow > fHigh)
                {
                    cSum += fLow;
                    iLow--;
                }
                else
                {
                    cSum += fHigh;
                    iHigh++;
                }
            }
            return (lKeys[iLow], lKeys[iHigh]);
        }


        public static List<Cluster> GetClusters(Dictionary<float, int> d)
        {
            List<Cluster> clusters = new List<Cluster>();
            List<float> lKeys = new List<float>(d.Keys);
            for (int i = 0; i < lKeys.Count; i++)
            {
                float fKey = lKeys[i];
                int iPrevious = 0;
                if (i > 0)
                    iPrevious = d[lKeys[i - 1]];
                int iNext = 0;
                if(i < lKeys.Count - 1)
                    iNext= d[lKeys[i+1]];
                int iCurrent = d[fKey];
                if(iCurrent > iPrevious && iCurrent > iNext)
                {
                    Cluster c = new Cluster();
                    c.Start = i - 1;
                    c.End = i + 1;
                    c.MaxIndex = i;
                    c.MaxPixels = iCurrent;
                    clusters.Add(c);
                }
            }
            clusters.Last().End = lKeys.Count - 1;
            bool bChanged = true;
            while(bChanged)
            {
                bChanged = false;
                List<Cluster> lNew = new List<Cluster>();
                for(int i = 0; i < clusters.Count; i++)
                {
                    Cluster c = clusters[i];
                    if(i == clusters.Count - 1)
                    {
                        lNew.Add(c);
                        continue;
                    }
                    Cluster cNext = clusters[i + 1];
                    int iMinBetween = c.MaxPixels, iMinIndex = -1;
                    for(int j = c.MaxIndex; j < cNext.MaxIndex; j++)
                    {
                        int iCurrent = d[lKeys[j]];
                        if (iCurrent < iMinBetween)
                        {
                            iMinBetween = iCurrent;
                            iMinIndex = j;
                        }
                    }
                    if(iMinBetween * 2 > c.MaxPixels || iMinBetween * 2 > cNext.MaxPixels )
                    {
                        Cluster cNew = new Cluster();
                        cNew.Start = c.Start;
                        cNew.End = cNext.End;
                        if(c.MaxPixels > cNext.MaxPixels)
                        {
                            cNew.MaxPixels = c.MaxPixels;
                            cNew.MaxIndex = c.MaxIndex;
                        }
                        else
                        {
                            cNew.MaxPixels = cNext.MaxPixels;
                            cNew.MaxIndex = cNext.MaxIndex;
                        }
                        lNew.Add(cNew);
                        i++;
                        bChanged = true;
                    }
                    else
                    {
                        lNew.Add(c);
                        c.End = iMinIndex;
                        cNext.Start = iMinIndex + 1;
                    }
                }
                clusters = lNew;
            }
            /*
            if (clusters.Count > 2)
            {
                clusters = new List<Cluster>(clusters.OrderByDescending(x => x.PixelCount));

                Cluster cSecond = clusters[1];
                clusters[1] = clusters[clusters.Count - 1];
                clusters[clusters.Count - 1] = cSecond;
            }
            */
            foreach(Cluster c in clusters)
            {
                if (c.Start > 0)
                    c.Start = lKeys[(int)c.Start];
                else
                    c.Start = lKeys.First();
                if (c.End < lKeys.Count)

                    c.End = lKeys[(int)c.End];
                else
                    c.End = lKeys.Last();
                
            }
            return clusters;
        }


        public static List<Cluster> GetClusters2(Dictionary<float, int> d)
        {
            List<Cluster> clusters = new List<Cluster>();
            List<float> lKeys = new List<float>(d.Keys);
            int iClusterMin = int.MaxValue, iClusterMax = 0;
            float fClusterStart = 0, fClusterEnd = 0;
            int iPrevious = 0;
            bool bPreviousIncreasing = true, bIncreasing = true;
            int cPixels = 0;
            for(int i = 0; i < lKeys.Count; i++)
            {
                float fKey = lKeys[i];
                int iCurrent = d[fKey];
                if (iCurrent >= iPrevious)
                    bIncreasing = true;
                else
                    bIncreasing = false;
                if(bIncreasing != bPreviousIncreasing && bIncreasing)
                {
                    if (iPrevious < iClusterMax / 2)
                    {
                        fClusterEnd = lKeys[i - 1];

                        Cluster c = new Cluster();
                        c.Start = fClusterStart;
                        c.End = fClusterEnd;
                        c.PixelCount = cPixels;
                        c.MaxPixels = iClusterMax;
                        c.MinPixels = iClusterMin;

                        clusters.Add(c);

                        iClusterMin = int.MaxValue;
                        iClusterMax = 0;
                        fClusterStart = fKey;
                        cPixels = 0;
                    }
                }
                if(iCurrent > iClusterMax)
                    iClusterMax = iCurrent;
                if (iCurrent < iClusterMin)
                    iClusterMin = iCurrent;
                cPixels += iCurrent;
                bPreviousIncreasing = bIncreasing;
                iPrevious = iCurrent;
            }

            if (clusters.Count > 2)
            {
                clusters = new List<Cluster>(clusters.OrderByDescending(x => x.PixelCount));

                Cluster cSecond = clusters[1];
                clusters[1] = clusters[clusters.Count - 1];
                clusters[clusters.Count - 1] = cSecond;
            }
            return clusters;
        }

        public static Dictionary<float, int> ComputeHistogram(int[,] aColors, int iStep, int iMin, int iMax)
        {
            Dictionary<float,int> histogram = new Dictionary<float, int>();
            for (int i = iMin; i <= iMax; i += iStep)
                histogram[i] = 0;
            for(int x = 0; x< aColors.GetLength(0); x++)
            {
                for(int y = 0; y < aColors.GetLength(1); y++)
                {
                    int gray = aColors[x, y];
                    int value = (gray / iStep) * iStep;
                    histogram[value]++;
                }
            }
            return histogram;
        }

        private void ComputeHistogram(DirectBitmap db, out Dictionary<float, int> dHue, out Dictionary<float, int> dSaturation, out Dictionary<float, int> dBrightness, int xStart = 0, int yStart = 0)
        {
            dHue = new Dictionary<float, int>();
            dSaturation = new Dictionary<float, int>();
            dBrightness = new Dictionary<float, int>();
            for (int i = 0; i < 360; i += 10)
                dHue[i] = 0;
            for (float f = 0; f <= 10; f++)
                dSaturation[f / 10] = 0;
            for (float f = 0; f <= 10; f++)
                dBrightness[f / 10] = 0;

            int cPixels = Width * Height;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Color c = db.GetPixel(x + xStart, y + yStart);
                    float h = c.GetHue();
                    float s = c.GetSaturation();
                    float b = c.GetBrightness();

                    h = (float)Math.Floor(h / 10) * 10;
                    dHue[h]++;
                    s = (float)(Math.Floor(s * 10) / 10);
                    dSaturation[s]++;
                    b = (float)(Math.Floor(b * 10) / 10);
                    dBrightness[b]++;
                }
            }
        }

        public void ClusterHSV(DirectBitmap db, double dBackgroundPortion, bool bHue = true, bool bSaturation = true, bool bBrightness = true)
        {
            Console.WriteLine("Binarizing outliers");

            OriginalBlackPoints = new HashSet<Point>();
            DateTime dtStart = DateTime.Now;
            Init(db.Width, db.Height);
            Dictionary<float, int> dHue = null;
            Dictionary<float, int> dSaturation = null;
            Dictionary<float, int> dBrightness = null;
            ComputeHistogram(db, out dHue, out dSaturation, out dBrightness);

            List<Cluster> clusters = GetClusters(dHue);

            
            for (int x = 0; x < db.Width; x++)
            {
                for (int y = 0; y < db.Height; y++)
                {
                    Color c = db.GetPixel(x, y);
                    float h = c.GetHue();
                    float s = c.GetSaturation();
                    float b = c.GetBrightness();

                    int iCluster = 0;
                    foreach (Cluster cluster in clusters)
                    {
                        if (h >= cluster.Start && h <= cluster.End)
                            break;
                        iCluster++;
                    }
                    int iGray = iCluster * 255 / clusters.Count;
                    SetPixel(x, y, iGray);
                }
            }
            DateTime dtEnd = DateTime.Now;
            Console.WriteLine("Time: " + (dtEnd - dtStart).TotalSeconds);

        }

        public class Cluster
        {
            public float Start, End;
            public int PixelCount;
            public int MaxPixels, MinPixels;
            public int MaxIndex;
        }

        public void BinarizeHSVOutliers(DirectBitmap db, double dBackgroundPortion, bool bHue = true, bool bSaturation = true, bool bBrightness = true, int xStart = 0, int yStart = 0)
        {
            Console.WriteLine("Binarizing outliers");

            OriginalBlackPoints = new HashSet<Point>();
            DateTime dtStart = DateTime.Now;
            if (Colors == null)
                Init(db.Width, db.Height);
            int cPixels = Width * Height;
            Dictionary<float, int> dHue = null;
            Dictionary<float, int> dSaturation = null;
            Dictionary<float, int> dBrightness = null;
            ComputeHistogram(db, out dHue, out dSaturation, out dBrightness, xStart, yStart);



            float fLowH, fHighH;
            (fLowH, fHighH) = GetRange(dHue, cPixels, dBackgroundPortion);
            float fLowS, fHighS;
            (fLowS, fHighS) = GetRange(dSaturation, cPixels, dBackgroundPortion);
            float fLowB, fHighB;
            (fLowB, fHighB) = GetRange(dBrightness, cPixels, dBackgroundPortion);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Color c = db.GetPixel(x + xStart, y + yStart);
                    float h = c.GetHue();
                    float s = c.GetSaturation();
                    float b = c.GetBrightness();

                    bool bBackground = true;
                    h = (float)Math.Floor(h / 10) * 10;
                    if (bHue && (h < fLowH || h > fHighH))
                        bBackground = false;
                    s = (float)(Math.Floor(s * 10) / 10);
                    if (bSaturation && (s < fLowS || s > fHighS))
                        bBackground = false;
                    b = (float)(Math.Floor(b * 10) / 10);
                    if (bBrightness && (b < fLowB || b > fHighB))
                        bBackground = false;

                    if (bBackground)
                        SetPixel(x, y, 255);
                    else
                    {
                        SetPixel(x, y, 0);
                        OriginalBlackPoints.Add(new Point(x, y));
                    }
                }
            }
            DateTime dtEnd = DateTime.Now;
            Console.WriteLine("Time: " + (dtEnd - dtStart).TotalSeconds);

        }


        public void Binarize2(DirectBitmap db, Func<Color, bool> Accept)
        {
            Console.WriteLine("Binarizing");
            OriginalBlackPoints = new HashSet<Point>();
            DateTime dtStart = DateTime.Now;
            Init(db.Width, db.Height);
            for (int x = 0; x < db.Width; x++)
            {
                for (int y = 0; y < db.Height; y++)
                {
                    Color c = db.GetPixel(x, y);
                    bool b = Accept(c);
                    if (b)
                        SetPixel(x, y, 255);
                    else
                    {
                        SetPixel(x, y, 0);
                        OriginalBlackPoints.Add(new Point(x, y));
                    }
                }
            }
            DateTime dtEnd = DateTime.Now;
            Console.WriteLine("Time: " + (dtEnd - dtStart).TotalSeconds);

        }

        public GrayBitmap StupidSegmentation(int iMaxBlack, int iMinWhite)
        {
            GrayBitmap gb = new GrayBitmap(Width, Height);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    int c = GetPixel(x, y);
                    if(c <= iMaxBlack)
                        gb.SetPixel(x, y, 0);
                    else if(c>= iMinWhite)  
                        gb.SetPixel(x,y, 255);  
                    else
                        gb.SetPixel(x, y, NEUTRAL);
                }
            }
            return gb;
        }
        public GrayBitmap SmoothMedian(int iSize)
        {
            GrayBitmap gb = new GrayBitmap(Width, Height);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    List<int> lColors = new List<int>();
                    for (int i = -iSize; i <= iSize; i++)
                    {
                        for (int j = -iSize; j <= iSize; j++)
                        {
                            int c = GetPixel(x + i, y + j);
                            lColors.Add(c);
                        }
                    }
                    lColors.Sort();
                    int cMedian = lColors[lColors.Count / 2];
                    if (cMedian < 0)
                        cMedian = GetPixel(x,y);
                    gb.SetPixel(x, y, cMedian);
                }
            }
            return gb;
        }
    }
}
