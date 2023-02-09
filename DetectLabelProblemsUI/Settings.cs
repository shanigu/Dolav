using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DetectLabelProblems
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }


        private void btnOk_Click(object sender, EventArgs e)
        {
            ImageComparator.WhiteThreshold = (int)udWhiteThreshold.Value;
            ImageComparator.BlackThreshold = (int)udBlackThreshold.Value;
            ImageComparator.RectangleSize = (int)udRectSize.Value;
            ImageComparator.SmoothingArea = (int)udSmoothingArea.Value;
            ImageComparator.ErrorDiffThreshold = (int)udErrorThreshold.Value;
            ImageComparator.MaxOffset = (int)udMaxOffset.Value;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Settings_Load(object sender, EventArgs e)
        {

            udWhiteThreshold.Maximum = 255;
            udWhiteThreshold.Minimum = 0;
            udWhiteThreshold.Value = ImageComparator.WhiteThreshold;

            udBlackThreshold.Maximum = 255;
            udBlackThreshold.Minimum = 0;
            udBlackThreshold.Value = ImageComparator.BlackThreshold;

            udRectSize.Maximum = 1000;
            udRectSize.Minimum = 50;
            udRectSize.Value = ImageComparator.RectangleSize;

            udSmoothingArea.Maximum = 50;
            udSmoothingArea.Minimum = 0;
            udSmoothingArea.Value = ImageComparator.SmoothingArea;

            udErrorThreshold.Maximum = 1000;
            udErrorThreshold.Minimum = 10;
            udErrorThreshold.Value = ImageComparator.ErrorDiffThreshold;

            udMaxOffset.Maximum = 100;
            udMaxOffset.Minimum = 0;
            udMaxOffset.Value = ImageComparator.MaxOffset;

        }
    }
}
