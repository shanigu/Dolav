using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectLabelProblems
{
    public class Pipeline
    {
        public GrayBitmap Input, Output;
        public bool WriteIntermediateImages;

        List<Step> Steps;

        public Pipeline()
        {
            Steps = new List<Step>();
            WriteIntermediateImages = false;
        }

        public GrayBitmap Apply(GrayBitmap input)
        {
            Input = input;
            GrayBitmap output = Input;
            foreach(Step s in Steps)
            {
                s.Input = output;
                s.Apply();
                output = s.Output;

                if (WriteIntermediateImages)
                    output.Save(s.FileName);
            }
            Output = output;
            return Output;
        }

        public void AddStep(Func<GrayBitmap, GrayBitmap> f, string sFileName)
        {
            Step s = new Step();
            s.StepFunction = f;
            s.FileName = sFileName;
            Steps.Add(s);
        }

        public class Step
        {
            public GrayBitmap Input, Output;
            public Func<GrayBitmap, GrayBitmap> StepFunction;
            public string FileName;
            
            public void Apply()
            {
                Output = StepFunction(Input);
            }
        }
    }
}
