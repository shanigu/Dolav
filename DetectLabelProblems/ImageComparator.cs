using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
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
        public static int FilterSize { get; set; } //3 for large objects, 1 for small objects
        public static int MaxOffset { get; set; }
        public static int RectangleSize { get; set; }
        public static int BlackThreshold { get; set; }
        public static int WhiteThreshold { get; set; }
        public static int SmoothingArea { get; set; }

        public static Rectangle ReferenceRectangle { get; set; }
        public static bool ReferenceRectangleMarked { get; set; }

        public static bool AdjustBrightness { get; set; }

        public static double ResizeFactor { get; set; }


        public ImageComparator()
        {
            Errors = new List<Rectangle>();
            //ReferenceRectangleMarked = false;
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
            else
                Console.WriteLine(Message);

           

            FileInfo fiNewImage = new FileInfo(sNewImage);
            string sPath = fiNewImage.Directory.FullName + @"\Rects\";
            string sName = sPath + fiNewImage.Name + ".";
            if(Directory.Exists(sPath))
                Directory.Delete(sPath, true);
            Directory.CreateDirectory(sPath);

            Console.WriteLine("Loading images");
            //Loading the original bitmaps, reference and inspected
            Bitmap bmp1 = new Bitmap(sReferenceImage);
            Bitmap bmp2 = new Bitmap(sNewImage);

            //Resize images if needed, scaling down the resolution
            if(ResizeFactor != 1.0)
            {
                bmp1 = new Bitmap(bmp1, new Size((int)(bmp1.Width * ResizeFactor), (int)(bmp1.Height * ResizeFactor)));
                bmp2 = new Bitmap(bmp2, new Size((int)(bmp2.Width * ResizeFactor), (int)(bmp2.Height * ResizeFactor)));

                if(ReferenceRectangleMarked)
                {
                    ReferenceRectangle = new Rectangle((int)(ReferenceRectangle.X * ResizeFactor), (int)(ReferenceRectangle.Y * ResizeFactor),
                        (int)(ReferenceRectangle.Width * ResizeFactor), (int)(ReferenceRectangle.Height * ResizeFactor));
                }
            }

            //using a wrapper for the Bitmap class that allows for fast access to the pixels
            DirectBitmap db1 = new DirectBitmap(bmp1);
            DirectBitmap db2 = new DirectBitmap(bmp2);

            DirectBitmap db1Adjusted = db1;
            DirectBitmap db2Adjusted = db2;

            //If there is light from a specific corner, adjust the brightness of the entire picture accordingly
            if (AdjustBrightness)
            {
                Console.WriteLine("Adjusting brightness");
                db1Adjusted = db1.AdjustBrightness();
                db2Adjusted = db2.AdjustBrightness();
                db1Adjusted.Bitmap.Save(sName + "adjusted1.jpg");
                db2Adjusted.Bitmap.Save(sName + "adjusted2.jpg");
            }


            //conver the bitmap to grayscale, this is done to ignore color variance
            //here we also cur the reference rectangle - the part of the image that we want to focus on
            Console.WriteLine("Converting to grayscale");
            GrayBitmap gray1 = null;
            if(ReferenceRectangleMarked)
                gray1 = new GrayBitmap(db1Adjusted, ReferenceRectangle);
            else
                 gray1 = new GrayBitmap(db1Adjusted);
            gray1.Save(sName + "original1.jpg");

            GrayBitmap gray2 = null;
            if (ReferenceRectangleMarked)
                gray2 = new GrayBitmap(db2Adjusted, ReferenceRectangle);
            else
                gray2 = new GrayBitmap(db2Adjusted);
            gray2.Save(sName + "original2.jpg");


            //now we smooth - replace each pixel with the median of its neighbors
            //we also segment the image into 3 colors - black, white, and gray (neutral background color)
            Console.WriteLine("Smoothing and binarizing");
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



            //we break the image into rectangles so that we can compare offsets on each rectangle independently
            Console.WriteLine("Splitting image to rectangles");
            List<GrayRect> lGrayRects1 = ComputeRects(gray1, RectangleSize);
            List<GrayRect> lGrayRects2 = ComputeRects(gray2, RectangleSize);

            Dictionary<string, double> diffs = new Dictionary<string, double>();

            Console.WriteLine("Started comparing rectangles");
            for (int i = 0; i < lGrayRects1.Count; i++)
            {

                GrayRect r1 = lGrayRects1[i];
                GrayRect r2 = lGrayRects2[i];

                Debug.WriteLine("Rect " + i + "/" + lGrayRects1.Count + ": " + r1.XStart + "," + r1.YStart + ": " + r1.NonNeutralPoints.Count);
                int iPercetage = (int)((100 * i) / lGrayRects1.Count);
                if (WorkerReportsProgress)
                {
                    Message = "";
                    ReportProgress(iPercetage);
                }
                else
                {
                    Console.Write("\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b" + "" +
                        "Completed: " + iPercetage + "%, Errors: " + Errors.Count);
                }

                //we compute the offset between the rectangles
                (int xBestOffset, int yBestOffset) = ComputeBestOffset(r1, r2, MaxOffset, out bool bValid);
                if (!bValid)
                    continue;

                //given the offset, we compute the diff - pixels which are different between the images
                GrayRect rDiff = ComputeDiff(gray1, gray2, r1.XStart, r1.XStart + r1.Width,
                    r1.YStart, r1.YStart + r1.Height, xBestOffset, yBestOffset);

                
                //we now use erosion and dilation to get rid of small differences - thin lines and small areas
                GrayRect rDiffErode = rDiff.Erode(FilterSize);
                GrayRect rDiffDilate = rDiffErode.Dilate(FilterSize);

                //afterwards we comput the number of pixels that are truly different between the images
                int cDiff = rDiffDilate.NonNeutralPoints.Count();
                diffs[r1.XStart + "_" + r1.YStart] = cDiff;

                //if the number of different pixels exceed a threshold we decide that this is an error that should be reported
                if (cDiff > ErrorDiffThreshold)
                {
                    rDiff.Save(sName + "diff" + r1.XStart + "_" + r1.YStart + ".jpg");
                    rDiffErode.Save(sName + "diff" + r1.XStart + "_" + r1.YStart + ".erode.jpg");
                    rDiffDilate.Save(sName + "diff" + r1.XStart + "_" + r1.YStart + ".dilate.jpg");
                    //We now compute a rectangle that contains all the different pixels for reporting the error later
                    Rectangle rBoundingBox = rDiffDilate.GetNonNeutralBoundingBox();
                    r1.Save(sName + "r1_" + r1.XStart + "_" + r1.YStart + ".jpg");
                    r2.Save(sName + "r2_" + r1.XStart + "_" + r1.YStart + ".jpg");
                    Errors.Add(rBoundingBox);
                }
            }
            
            Console.WriteLine("\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b" + "" +
                "Completed: " + 100 + "%, Errors: " + Errors.Count);
            Console.WriteLine("Merging rectangles with errors");

            //we now merge the rectnagles becasue several of them cover the same area
            Errors = Merge(Errors);
            Console.WriteLine("Done comparing images, detected " + Errors.Count + " problematic areas"); 
            
            foreach(Rectangle r in Errors)
            {
                db2.DrawRectangle(r);
            }
            db2.Bitmap.Save(sName + "result.jpg");

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
                        if(iSumDiff < 50)
                            dbDiff.SetPixel(x - XStart, y - YStart, NEUTRAL);
                        else
                            dbDiff.SetPixel(x - XStart, y - YStart, 0);
                    }
                    else
                        dbDiff.SetPixel(x - XStart, y - YStart, NEUTRAL);
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

        public GrayRect CreateCopy(GrayBitmap gray, int XStart, int YStart, int iSize)
        {
            GrayRect rect = new GrayRect(iSize);

            rect.XStart = XStart;
            rect.YStart = YStart;
            for (int x = 0; x < iSize; x++)
            {
                for (int y = 0; y < iSize; y++)
                {
                    if (XStart + x < gray.Width && YStart + y < gray.Height)
                        rect[x, y] = gray.GetPixel(XStart + x, YStart + y);
                }
            }
            return rect;
        }

        private List<GrayRect> ComputeRects(GrayBitmap gray, int iSize)
        {
            List<GrayRect> lRects = new List<GrayRect>();
            GrayRect rect = null;
            for (int i = 0; i < gray.Width; i += iSize / 2)
            {
                for (int j = 0; j < gray.Height; j += iSize / 2)
                {

                    rect = new GrayRect(gray, i, j, iSize);
                    lRects.Add(rect);
                }
            }
            return lRects;
        }


        public class GrayRect
        {
            public bool Static;
            public int[,] Pixels;
            GrayBitmap FullImage;
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
                Static = false;
                Width = Height = iSize;
                Pixels = new int[iSize, iSize];
                FullImage = null;
                NonNeutralPoints = new List<Point>();
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        SetPixel(x, y, NEUTRAL);
                    }
                }
            }
            public GrayRect(GrayBitmap image, int iXStart, int iYStart, int iSize)
            {
                Static = true;
                Width = Height = iSize;
                FullImage = image;
                Pixels = null;
                XStart = iXStart; 
                YStart = iYStart;
                ComputeNonNeutralPoints();
            }


            private void ComputeNonNeutralPoints()
            {
                if (Static)
                {
                    NonNeutralPoints = new List<Point>();
                    for (int x = 0; x < Width; x++)
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            int c = GetPixel(x, y);
                            if(c != NEUTRAL)
                                NonNeutralPoints.Add(new Point(x, y));
                        }
                    }
                }
            }

            public void SetPixel(int i, int j, int c)
            {
                if (Static)
                {
                    throw new InvalidOperationException();
                }
                else
                {
                    if (i >= 0 && j >= 0 && i < Width && j < Height)
                    {
                        Pixels[i, j] = c;
                        Point p = new Point(i, j);
                        if (c != GrayBitmap.NEUTRAL)
                            NonNeutralPoints.Add(p);
                        else
                            NonNeutralPoints.Remove(p);
                    }
                }
            }
            public int GetPixel(int i, int j)
            {
                if (Static)
                {
                    int x = XStart + i, y = YStart + j;
                    if (x < 0 || x >= FullImage.Width || y < 0 || y >= FullImage.Height)
                        return GrayBitmap.NEUTRAL;
                    return FullImage.GetPixel(x, y);
                }
                else
                {
                    if (i < 0 || i >= Width || j < 0 || j >= Height)
                        return GrayBitmap.NEUTRAL;
                    return Pixels[i, j];
                }
            }

            public GrayRect Convolution(double[,] kernel, bool bReplaceBlackWithNeutral)
            {
                int iSize = kernel.GetLength(0) / 2;
                GrayRect gr = new GrayRect(Width);
                for (int x = iSize; x < Width; x++)
                {
                    for (int y = iSize; y < Height; y++)
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
                            gr.SetPixel(x, y, (int)dSum);
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
                        int iGray = GetPixel(x,y);
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

            public GrayRect Erode(int iSize)
            {
                GrayRect gr = new GrayRect(Width);
                gr.XStart = XStart; gr.YStart = YStart;
                foreach (Point p in NonNeutralPoints)
                {
                    bool bNeutralNeighbor = false;
                    for (int i = -iSize; i <= iSize && !bNeutralNeighbor; i++)
                    {
                        for (int j = -iSize; j <= iSize && !bNeutralNeighbor; j++)
                        {
                            int c = GetPixel(p.X + i, p.Y + j);
                            if (c == NEUTRAL)
                                bNeutralNeighbor = true;
                        }
                    }
                    int cCurrent = GetPixel(p.X, p.Y);
                    if (!bNeutralNeighbor)
                        gr.SetPixel(p.X, p.Y, cCurrent);
                }
                return gr;
            }


            public GrayRect Dilate(int iSize)
            {
                GrayRect gr = new GrayRect(Width);
                gr.XStart = XStart; gr.YStart = YStart;

                foreach (Point p in NonNeutralPoints)
                {
                    int c = GetPixel(p.X, p.Y);
                    for (int i = -iSize; i <= iSize; i++)
                    {
                        for (int j = -iSize; j <= iSize; j++)
                        {
                            gr.SetPixel(p.X + i, p.Y + j, c);
                        }
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
                foreach (Point p in NonNeutralPoints)
                {
                    if (p.X > iEdge && p.Y > iEdge && p.X < Width - iEdge && p.Y < Height - iEdge)
                        cPoints++;
                }
                return cPoints;
            }

            public Rectangle GetNonNeutralBoundingBox()
            {
                int iMinX = int.MaxValue;
                int iMaxX = 0;
                int iMinY = int.MaxValue;
                int iMaxY = 0;
                foreach (Point p in NonNeutralPoints)
                {
                    if (p.X > iMaxX)
                        iMaxX = p.X;
                    if (p.X < iMinX)
                        iMinX = p.X;
                    if (p.Y > iMaxY)
                        iMaxY = p.Y;
                    if (p.Y < iMinY)
                        iMinY = p.Y;
                }
                if (ReferenceRectangleMarked)
                    return new Rectangle(iMinX + XStart + ReferenceRectangle.X, iMinY + YStart + ReferenceRectangle.Y, iMaxX - iMinX, iMaxY - iMinY);
                else
                    return new Rectangle(iMinX + XStart, iMinY + YStart, iMaxX - iMinX, iMaxY - iMinY);
            }
        }



        public class GrayRect2
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
            public GrayRect2(int iSize)
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
