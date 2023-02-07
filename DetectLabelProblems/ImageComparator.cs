using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;
using static DetectLabelProblems.GrayBitmap;

namespace DetectLabelProblems
{
    public class ImageComparator : BackgroundWorker
    {

        public string NewImage { get; set; }
        public string ReferenceImage { get; set; }
        public string Message { get; private set; }

        public List<Rectangle> Errors;
       
        //settings
        public static int ErrorDiffThreshold { get; set; }
        public static int MaxOffset { get; set; }
        public static int RectangleSize { get; set; }
        public static int BlackThreshold { get; set; }
        public static int WhiteThreshold { get; set; }
        public static int SmoothingArea { get; set; }

        public Rectangle ReferenceRectangle { get; set; }
        public bool ReferenceRectangleMarked { get; set; }


        public ImageComparator()
        {
            Errors = new List<Rectangle>();
            ReferenceRectangleMarked = false;
        }

        public void Run(object sender, DoWorkEventArgs e)
        {

            try
            {
                CompareInParts(ReferenceImage, NewImage);
            }
            catch (Exception ex)
            {
                Message = "Exception caught: " + ex.Message;
                if (WorkerReportsProgress)
                    ReportProgress(1);
            }


        }




        private void CompareInParts(string sReferenceImage, string sNewImage)
        {
            Message = "Comparing images " + sReferenceImage + " to " + sNewImage;
            if (WorkerReportsProgress)
                ReportProgress(0);

           

            FileInfo fiNewImage = new FileInfo(sNewImage);
            string sPath = fiNewImage.Directory.FullName + @"\Rects\";
            string sName = sPath + fiNewImage.Name + ".";
            Directory.CreateDirectory(sPath);

            Bitmap bmp1 = new Bitmap(sReferenceImage);
            Bitmap bmp2 = new Bitmap(sNewImage);
            
            DirectBitmap db1 = new DirectBitmap(bmp1);
            DirectBitmap db2 = new DirectBitmap(bmp2);


            //BUGBUG; defining smaller region here due to reflections - need to revise
            GrayBitmap gray1 = null;
            if(ReferenceRectangleMarked)
                gray1 = new GrayBitmap(db1, ReferenceRectangle);
            else
                 gray1 = new GrayBitmap(db1);
            gray1.Save(sName + "original1.jpg");
            GrayBitmap gray2 = null;
            if (ReferenceRectangleMarked)
                gray2 = new GrayBitmap(db2, ReferenceRectangle);
            else
                gray2 = new GrayBitmap(db2);
            gray2.Save(sName + "original2.jpg");
            
            Pipeline pipeline1 = new Pipeline();
            pipeline1.WriteIntermediateImages = true;
            pipeline1.AddStep(gb => gb.SmoothMedian(SmoothingArea / 2), sName + "smoothed1.jpg");
            pipeline1.AddStep(gb => gb.StupidSegmentation(BlackThreshold, WhiteThreshold), sName + "segmented1.jpg");
            pipeline1.Apply(gray1);
            gray1 = pipeline1.Output;
            
            
            Pipeline pipeline2 = new Pipeline();
            pipeline2.WriteIntermediateImages = true;
            pipeline2.AddStep(gb => gb.SmoothMedian(SmoothingArea / 2), sName + "smoothed2.jpg");
            pipeline2.AddStep(gb => gb.StupidSegmentation(BlackThreshold, WhiteThreshold), sName + "segmented2.jpg");
            pipeline2.Apply(gray2);
            gray2 = pipeline2.Output;

            List<GrayRect> lGrayRects1 = ComputeRects(gray1, RectangleSize);
            List<GrayRect> lGrayRects2 = ComputeRects(gray2, RectangleSize);

            Dictionary<string, double> diffs = new Dictionary<string, double>();


            for (int i = 0; i < lGrayRects1.Count; i++)
            {
                GrayRect r1 = lGrayRects1[i];
                GrayRect r2 = lGrayRects2[i];

                Debug.WriteLine("Rect " + i + "/" + lGrayRects1.Count + ": " + r1.XStart + "," + r1.YStart + ": " + r1.NonNeutralPoints.Count);
                if (WorkerReportsProgress)
                {
                    int iPercetage = (int)((100 * i) / lGrayRects1.Count);
                    Message = "";
                    ReportProgress(iPercetage);
                }


                (int xBestOffset, int yBestOffset) = ComputeBestOffset(r1, r2, MaxOffset, out bool bValid);
                if (!bValid)
                    continue;
                GrayRect rDiff = ComputeDiff(gray1, gray2, r1.XStart, r1.XStart + r1.Width,
                    r1.YStart, r1.YStart + r1.Height, xBestOffset, yBestOffset);

                rDiff = rDiff.ErodeDilate(3, true);
                rDiff = rDiff.ErodeDilate(3, false);
                int cDiff = rDiff.CountNonZero();
                diffs[r1.XStart + "_" + r1.YStart] = cDiff;
                if (cDiff > ErrorDiffThreshold)
                {
                    rDiff.Save(sName + "diff" + r1.XStart + "_" + r1.YStart + ".jpg");
                    r1.Save(sName + "r1_" + r1.XStart + "_" + r1.YStart + ".jpg");
                    r2.Save(sName + "r2_" + r1.XStart + "_" + r1.YStart + ".jpg");
                    Errors.Add(new Rectangle(r1.XStart + ReferenceRectangle.X, r1.YStart + ReferenceRectangle.Y, r1.Width, r1.Height));
                   
                }
            }

            Errors = Merge(Errors);
            

            bmp1.Dispose();
            bmp2.Dispose();
            db1.Dispose();
            db2.Dispose();
        }

        private List<Rectangle> Merge(List<Rectangle> l)
        {
            Queue<Rectangle> q = new Queue<Rectangle>(l);
            int cRects = q.Count;
            while(cRects > 0)
            {
                Rectangle r1 = q.Dequeue();
                int cRemaining = q.Count;
                bool bMerged = false;
                while(!bMerged && cRemaining>0)
                {
                    Rectangle r2 = q.Dequeue();
                    cRemaining--;
                    if (r1.IntersectsWith(r2))
                    {
                        int xMin = Math.Min(r1.X, r2.X);
                        int yMin = Math.Min(r1.Y, r2.Y);
                        int xMax = Math.Max(r1.X + r1.Width, r2.X + r2.Width);
                        int yMax = Math.Max(r1.Y + r1.Height, r2.Y + r2.Height);
                        Rectangle rUnion = new Rectangle(xMin, yMin, xMax - xMin, yMax - yMin);
                        q.Enqueue(rUnion);
                        bMerged = true;
                    }
                    else
                        q.Enqueue(r2);
                }
                if (bMerged)
                {
                    cRects = q.Count;
                }
                else
                {
                    q.Enqueue(r1);
                    cRects--;
                }
            }
            return new List<Rectangle>(q);
        }


        private List<Rectangle> Merge2(List<Rectangle> l)
        {
            bool bMerged = true;
            while (bMerged && l.Count > 1)
            {
                bMerged = false;
                List<Rectangle> lNew = new List<Rectangle>();
                for (int i = 0; i < lNew.Count; i++)
                {
                    Rectangle r1 = l[i];
                    bool bIntersects = false;
                    for (int j = i + 1; j < Errors.Count; j++)
                    {
                        Rectangle r2 = Errors[j];
                        if (r1.IntersectsWith(r2))
                        {
                            int xMin = Math.Min(r1.X, r2.X);
                            int yMin = Math.Min(r1.Y, r2.Y);
                            int xMax = Math.Max(r1.X + r1.Width, r2.X + r2.Width);
                            int yMax = Math.Max(r1.Y + r1.Height, r2.Y + r2.Height);
                            Rectangle rUnion = new Rectangle(xMin, yMin, xMax - xMin, yMax - yMin);
                            lNew.Add(rUnion);
                            bIntersects = true;
                            bMerged = true;
                            break;
                        }
                    }
                    if (!bIntersects)
                        lNew.Add(r1);
                }
                l = lNew;
            }
            return l;
        }

        private GrayRect ComputeDiff(GrayBitmap db1, GrayBitmap db2,
               int XStart, int XEnd, int YStart, int YEnd,
               int xOffest, int yOffset)
        {


            GrayRect dbDiff = new GrayRect(XEnd - XStart);
            dbDiff.XStart = XStart;
            dbDiff.YStart = YStart;
            for (int x = XStart; x < XEnd; x++)
            {
                for (int y = YStart; y < YEnd; y++)
                {

                    int c1 = db1.GetPixel(x, y);
                    int c2 = db2.GetPixel(x + xOffest, y + yOffset);

                    if (c1 > -1 && c2 > -1)
                    {
                        int iSumDiff = Math.Abs(c1 - c2);
                        dbDiff.SetPixel(x - XStart, y - YStart, iSumDiff);
                    }
                }
            }
            return dbDiff;
        }

        




        private (int, int) ComputeBestOffset(GrayRect gray1, GrayRect gray2, int iMaxOffset, out bool bValid)
        {
            bValid = false;
            if (gray1.NonNeutralPoints.Count == 0 && gray2.NonNeutralPoints.Count == 0)
                return (0, 0);
            int cInsidePoints1 = gray1.CountInsidePoints(iMaxOffset);
            int cInsidePoints2 = gray2.CountInsidePoints(iMaxOffset);
            if (cInsidePoints1 < ErrorDiffThreshold && cInsidePoints2 < ErrorDiffThreshold)
                return (0, 0);

            bValid = true;

            double dBestXOffset = ComputeDistance(gray1, gray2, 0, 0);
            int xBestOffset = 0, yBestOffset = 0;
            for (int xOffset = -iMaxOffset; xOffset <= iMaxOffset; xOffset++)
            {
                for (int yOffset = -iMaxOffset; yOffset <= iMaxOffset; yOffset++)
                {
                    double dSumDistances = ComputeDistance(gray1, gray2, xOffset, yOffset);

                    if (dSumDistances < dBestXOffset)
                    {
                        dBestXOffset = dSumDistances;
                        xBestOffset = xOffset;
                        yBestOffset = yOffset;
                    }
                }
            }
            
            return (xBestOffset, yBestOffset);
        }


        private double ComputeDistance(GrayRect gray1, GrayRect gray2, int xOffest, int yOffset)
        {
            double dSumDistances = 0.0;
            int Width = gray1.Width;
            int Height = gray1.Height;
            int cValidPixels = 0;
            foreach (Point p in gray1.NonNeutralPoints)
            {
                int g1 = gray1[p.X, p.Y];
                int g2 = gray2[p.X + xOffest, p.Y + yOffset];
                {
                    cValidPixels++;
                    int iDistance = Math.Abs(g1 - g2);
                    if (iDistance > 50)
                        dSumDistances++;
                }
            }
            
            foreach (Point p in gray2.NonNeutralPoints)
            {
                int g1 = gray1[p.X - xOffest, p.Y - yOffset];
                int g2 = gray2[p.X, p.Y];
                {
                    cValidPixels++;
                    int iDistance = Math.Abs(g1 - g2);
                    if (iDistance > 50)
                        dSumDistances++;
                }
            }
            
            return dSumDistances / cValidPixels;
        }


        private List<GrayRect> ComputeRects(GrayBitmap gray, int iSize)
        {
            List<GrayRect> lRects = new List<GrayRect>();
            for (int i = 0; i < gray.Width - iSize; i += iSize / 2)
            {
                for (int j = 0; j < gray.Height - iSize; j += iSize / 2)
                {

                    GrayRect rect = new GrayRect(iSize);
                    
                    rect.XStart = i;
                    rect.YStart = j;
                    for (int x = 0; x < iSize; x++)
                    {
                        for (int y = 0; y < iSize; y++)
                        {
                            if (i + x < gray.Width && j + y < gray.Height)
                                rect[x, y] = gray.GetPixel(i + x, j + y);
                        }
                    }
                    lRects.Add(rect);
                }
            }
            return lRects;
        }


        public class GrayRect
        {
            public int[,] Pixels;
            public int Width, Height;
            public int XStart, YStart;
            public List<Point> NonNeutralPoints;

            public int this[int i, int j]
            {
                get
                {

                    return GetPixel(i, j);
                }
                set
                {
                    SetPixel(i, j, value);
                }
            }
            public GrayRect(int iSize)
            {
                Width = Height = iSize;
                Pixels = new int[iSize, iSize];
                NonNeutralPoints = new List<Point>();
            }


            public void SetPixel(int i, int j, int c)
            {
                if (i >= 0 && j >= 0 && i < Width && j < Height)
                {
                    Pixels[i, j] = c;
                    if (c != GrayBitmap.NEUTRAL)
                        NonNeutralPoints.Add(new Point(i, j));
                }
            }
            public int GetPixel(int i, int j)
            {
                if (i < 0 || i >= Width || j < 0 || j >= Height)
                    return GrayBitmap.NEUTRAL;
                return Pixels[i, j];
            }

            public GrayRect Convolution(double[,] kernel, bool bReplaceBlackWithNeutral)
            {
                int iSize = kernel.GetLength(0) / 2;
                GrayRect gr = new GrayRect(Width);
                for (int x = iSize; x < Width ; x++)
                {
                    for (int y = iSize; y < Height ; y++)
                    {
                        if (x >= iSize && y >= iSize && x < Width - iSize && y < Height - iSize)
                        {
                            double dSum = 0.0;
                            for (int i = -iSize; i <= iSize; i++)
                            {
                                for (int j = -iSize; j <= iSize; j++)
                                {
                                    dSum += GetPixel(x + i, y + j) * kernel[i + iSize, j + iSize];
                                }
                            }
                            if (bReplaceBlackWithNeutral && dSum == 0)
                                dSum = NEUTRAL;
                            gr.Pixels[x, y] = (int)dSum;
                        }
                        else
                            gr.SetPixel(x, y, NEUTRAL);
                    }
                }
                return gr;
            }
            public GrayRect Sharpen()
            {
                double[,] kernel = new double[,] { { -1, -1, -1 }, { -1, 9, -1 }, { -1, -1, -1 } };
                return Convolution(kernel, false);
            }

            public void Save(string sFileName)
            {
                Bitmap bmp = new Bitmap(Width, Height);
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        int iGray = Pixels[x, y];
                        if (iGray > 255)
                            iGray = 255;
                        if (iGray < 0)
                            iGray = 0;
                        Color c = Color.FromArgb(iGray, iGray, iGray);
                        bmp.SetPixel(x, y, c);
                    }
                }
                bmp.Save(sFileName);
                bmp.Dispose();

            }

            public GrayRect ErodeDilate(int iSize, bool bMin)
            {
                GrayRect gr = new GrayRect(Width);
                for (int x = iSize; x < Width - iSize; x++)
                {
                    for (int y = iSize; y < Height - iSize; y++)
                    {
                        int iMin = 255;
                        int iMax = 0;
                        for (int i = -iSize; i <= iSize; i++)
                        {
                            for (int j = -iSize; j <= iSize; j++)
                            {
                                int c = GetPixel(x + i, y + j);
                                if (c < iMin)
                                    iMin = c;
                                if (c > iMax)
                                    iMax = c;

                            }
                        }
                        if (bMin)
                            gr.Pixels[x, y] = iMin;
                        else
                            gr.Pixels[x, y] = iMax;
                    }
                }
                return gr;
            }

            public int CountNonZero()
            {
                int cPixels = 0;
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        if (GetPixel(x, y) > 0)
                            cPixels++;
                    }
                }
                return cPixels;
            }

            public int CountInsidePoints(int iEdge)
            {
                int cPoints = 0;
                foreach(Point p in NonNeutralPoints)
                {
                    if (p.X > iEdge && p.Y > iEdge && p.X < Width - iEdge && p.Y < Height - iEdge)
                        cPoints++;
                }
                return cPoints;
            }
        }


    }
}
