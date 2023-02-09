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
    public partial class DisplayProblems : Form
    {
        public string ReferenceImage { get; set; }
        public string ProblematicImage { get; set; }

        public List<Rectangle> Errors { get; set; }
        public DisplayProblems()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void DisplayProblems_Load(object sender, EventArgs e)
        {
            Bitmap bmpRef = new Bitmap(ReferenceImage);
            picReference.Image = bmpRef;
            picReference.SizeMode = PictureBoxSizeMode.StretchImage;
            Bitmap bmpProblematic = new Bitmap(ProblematicImage);
            using(Graphics g = Graphics.FromImage(bmpProblematic))
            {
                Pen p = new Pen(Color.Red, 5);
                foreach (Rectangle r in Errors)
                    g.DrawRectangle(p, r);
            }
            picProblematic.Image = bmpProblematic;
            picProblematic.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            string sSuggestedName = "";
            int cPrefix = 0;
            while (ReferenceImage[cPrefix] == ProblematicImage[cPrefix])
                cPrefix++;
            sSuggestedName = ProblematicImage.Substring(cPrefix).Replace("\\", "_").Replace("/", "_");
            dlg.FileName = sSuggestedName;
            var res = dlg.ShowDialog();
            if(res == DialogResult.OK)
            {
                picProblematic.Image.Save(dlg.FileName);
            }
        }
    }
}
