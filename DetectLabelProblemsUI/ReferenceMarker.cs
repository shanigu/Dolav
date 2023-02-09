using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace DetectLabelProblems
{
    public partial class ReferenceMarker : Form
    {
        public string FileName { get; set; }
        private bool IsSelecting;
        private int XStart, YStart, XEnd, YEnd;
        private Bitmap OriginalImage;
        public ReferenceMarker()
        {
            InitializeComponent();
            IsSelecting = false;
        }

        public Rectangle GetRectangle()
        {
            return new Rectangle(XStart, YStart, XEnd - XStart, YEnd-YStart);
        }

        private void ReferenceMarker_Load(object sender, EventArgs e)
        {
            OriginalImage = new Bitmap(FileName);
            picReference.Image = OriginalImage;
           
            lblFileName.Text = FileName;
        }

        private Point Translate(int x, int y)
        {
            Point p = new Point( (int)(((float)this.picReference.Image.Width / this.picReference.Width) * x),
                                (int)(((float)this.picReference.Image.Height / this.picReference.Height) * y)
                        );
            return p;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnSkip_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void picReference_MouseDown(object sender, MouseEventArgs e)
        {
            IsSelecting = true;

            Point p = Translate(e.X, e.Y);
            // Save the start point.
            XStart = p.X;
            YStart = p.Y;
        }

        private void picReference_MouseMove(object sender, MouseEventArgs e)
        {
            // Do nothing it we're not selecting an area.
            if (!IsSelecting) return;


            Point p = Translate(e.X, e.Y);

            if (Math.Abs(XEnd - p.X) + Math.Abs(YEnd - p.Y) > 10)
            {

                // Save the new point.
                XEnd = p.X;
                YEnd = p.Y;

                // Make a Bitmap to display the selection rectangle.
                Bitmap bm = new Bitmap(OriginalImage);

                // Draw the rectangle.
                using (Graphics gr = Graphics.FromImage(bm))
                {
                    Pen pen = new Pen(Color.Red, 10);
                    gr.DrawRectangle(pen,
                        Math.Min(XStart, XEnd), Math.Min(YStart, YEnd),
                        Math.Abs(XStart - XEnd), Math.Abs(YStart - YEnd));
                }

                // Display the temporary bitmap.
                picReference.Image = bm;
            }
        }

        private void picReference_MouseUp(object sender, MouseEventArgs e)
        {
            // Do nothing it we're not selecting an area.
            if (!IsSelecting) return;
            IsSelecting = false;

            Point p = Translate(e.X, e.Y);
            // Save the new point.
            XEnd = p.X;
            YEnd = p.Y;


        }
    }
}
