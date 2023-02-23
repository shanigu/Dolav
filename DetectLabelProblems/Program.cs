
using DetectLabelProblems;
using System.Drawing;


try
{
    ImageComparator.BlackThreshold = 30;
    ImageComparator.WhiteThreshold = 170;
    ImageComparator.MaxOffset = 25;
    ImageComparator.ReferenceRectangle = new Rectangle(700, 1000, 3000, 1500);
    ImageComparator.ReferenceRectangleMarked = true;
    ImageComparator.SmoothingArea = 5;
    ImageComparator.RectangleSize = 300;
    ImageComparator.AdjustBrightness = false;
    ImageComparator.ErrorDiffThreshold = 1000;
    ImageComparator.FilterSize = 5;
    ImageComparator.ResizeFactor = 0.5;

    string sOutputFile = "results.txt";


    ImageComparator comp = new ImageComparator();

#if (DEBUG)
    comp.ReferenceImage = "D:\\Local\\Dolav\\2023-02-21\\00-12-12\\Image_001.jpg";
    comp.NewImage = "D:\\Local\\Dolav\\2023-02-21\\01-13-15\\Image_001.jpg";
    sOutputFile = "D:\\Local\\Dolav\\2023-02-21\\01-13-15\\results.txt";

#else
    if (args.Length < 3)
    {
        Console.WriteLine("Usage: DetectLabelProblems reference_image inspected_image result_file [config_file]");
        Environment.Exit(-1);
    }
    if (!File.Exists(args[0]))
    {
        Console.WriteLine("Could not find reference image: " + args[0]);
        Environment.Exit(-1);
    }
    if (!File.Exists(args[1]))
    {
        Console.WriteLine("Could not find inspected image: " + args[1]);
        Environment.Exit(-1);
    }



    if (args.Length > 3)
    {
        if (File.Exists(args[3]))
        {
            using (StreamReader sr = new StreamReader(args[3]))
            {
                ImageComparator.ReferenceRectangleMarked = false;
                while (!sr.EndOfStream)
                {
                    string sLine = sr.ReadLine();
                    string[] aLine = sLine.Split(new char[] { '=', ':' });
                    if (aLine[0].StartsWith("WhiteThreshold"))
                    {
                        ImageComparator.WhiteThreshold = int.Parse(aLine[1]);
                    }
                    if (aLine[0].StartsWith("BlackThreshold"))
                    {
                        ImageComparator.BlackThreshold = int.Parse(aLine[1]);
                    }
                    if (aLine[0].StartsWith("MaxOffset"))
                    {
                        ImageComparator.MaxOffset = int.Parse(aLine[1]);
                    }
                    if (aLine[0].StartsWith("ErrorDiffThreshold"))
                    {
                        ImageComparator.ErrorDiffThreshold = int.Parse(aLine[1]);
                    }
                    if (aLine[0].StartsWith("SmoothingArea"))
                    {
                        ImageComparator.SmoothingArea = int.Parse(aLine[1]);
                    }
                    if (aLine[0].StartsWith("RectangleSize"))
                    {
                        ImageComparator.RectangleSize = int.Parse(aLine[1]);
                    }
                    if (aLine[0].StartsWith("FilterSize"))
                    {
                        ImageComparator.FilterSize = int.Parse(aLine[1]);
                    }
                    if (aLine[0].StartsWith("AdjustBrightness"))
                    {
                        ImageComparator.AdjustBrightness = bool.Parse(aLine[1]);
                    }
                    if (aLine[0].StartsWith("ResizeFactor"))
                    {
                        ImageComparator.ResizeFactor = double.Parse(aLine[1]);
                    }
                    if (aLine[0].StartsWith("ReferenceRectangle"))
                    {
                        string[] aRect = aLine[1].Split(',');
                        Rectangle r = new Rectangle(int.Parse(aRect[0]), int.Parse(aRect[1]), int.Parse(aRect[2]), int.Parse(aRect[3]));
                        ImageComparator.ReferenceRectangle = r;
                        ImageComparator.ReferenceRectangleMarked = true;
                    }
                }
                sr.Close();
            }
        }
    }

    comp.ReferenceImage = args[0];
    comp.NewImage = args[1];
    sOutputFile = args[2];
#endif


    comp.Run(null, null);

    using (StreamWriter swResult = new StreamWriter(sOutputFile))
    {
        foreach(Rectangle r in comp.Errors)
        {
            swResult.WriteLine(r.ToString());
        }
        swResult.Close();
    }
}
catch (Exception e)
{
    Console.WriteLine("Caught an unexpected exception:" + e.Message);
    Environment.Exit(-1);
}
