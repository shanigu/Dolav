using System.CodeDom;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace DetectLabelProblems
{
    public partial class DetectionUI : Form
    {
        private FolderChangeListener Listener;
        private string LogFile = "DetectLabelProblems.log";

        private Dictionary<string, int> ImageProcessingProgress;
        private Dictionary<string, Rectangle> ImageReferenceRectangle;

        public DetectionUI()
        {
            InitializeComponent();
            ImageProcessingProgress = new Dictionary<string, int>();
            ImageReferenceRectangle = new Dictionary<string, Rectangle>();
        }


        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                DialogResult res = dlg.ShowDialog();
                if (res == DialogResult.OK)
                    txtFolder.Text = dlg.SelectedPath;
            }
        }

        private void Log(string s, bool bError)
        {
            DateTime dt = DateTime.Now;

            string sMessage = s + " (" + dt + ")";

            tbMessages.SelectionColor = bError ? Color.Red : Color.Black;
            tbMessages.AppendText(s + "\n");

            using (StreamWriter swLog = new StreamWriter(LogFile, true))
            {
                swLog.WriteLine(dt + ": " + s);
                swLog.Close();
            }
        }

        private void DetectionUI_Load(object sender, EventArgs e)
        {
            ImageComparator.BlackThreshold = 20;
            ImageComparator.WhiteThreshold = 170;
            ImageComparator.RectangleSize = 200;
            ImageComparator.ErrorDiffThreshold = 100;
            ImageComparator.MaxOffset = 50;
            ImageComparator.SmoothingArea = 5;
        }

        protected void NewFolderFound(object sender, ProgressChangedEventArgs e)
        {
            FolderChangeListener listener = (FolderChangeListener)sender;
            DirectoryInfo dirNew = new DirectoryInfo(listener.Subfolders.Last());
            DirectoryInfo dirRef = new DirectoryInfo(txtReference.Text);

            foreach (FileInfo file in dirRef.GetFiles("*.jpg"))
            {
                string sNewImage = dirNew.FullName + "/" + file.Name;
                if (!File.Exists(sNewImage))
                {
                    Log("Image " + file.Name + " not found in new folder", true);
                }
                else
                {
                    AddImageProgress(sNewImage);
                    

                    ImageComparator comp = new ImageComparator();
                    comp.DoWork += comp.Run;
                    comp.WorkerReportsProgress = true;
                    comp.ProgressChanged += ComparatorProgressReport;
                    comp.RunWorkerCompleted += ComparatorDone;
                    comp.ReferenceImage = file.FullName;
                    comp.NewImage = sNewImage;

                    if (ImageReferenceRectangle.ContainsKey(file.Name))
                    {
                        comp.ReferenceRectangleMarked = true;
                        comp.ReferenceRectangle = ImageReferenceRectangle[file.Name];
                    }
                    else
                        comp.ReferenceRectangleMarked = false;

                    Debug.WriteLine("Running comparator");
                    comp.RunWorkerAsync();
                }
            }
        }

        private void ComparatorProgressReport(object? sender, ProgressChangedEventArgs e)
        {
            ImageComparator comp = (ImageComparator)sender;

            ImageProcessingProgress[comp.NewImage] = (int)e.ProgressPercentage;
            pbImage1.Value = ImageProcessingProgress[cmbImagesInProcessing.SelectedItem.ToString()];
            if(comp.Message != "")
                Log(comp.Message, false);
        }

        private void ComparatorDone(object? sender, RunWorkerCompletedEventArgs e)
        {
            ImageComparator comp = (ImageComparator)sender;
            if (comp.Errors.Count > 0)
            {
                Log("Errors detected at: " + comp.NewImage, true);
                DisplayProblems frm = new DisplayProblems();
                frm.Errors = comp.Errors;
                FileInfo fiProblematic = new FileInfo(comp.NewImage);

                frm.ProblematicImage = fiProblematic.FullName;
                frm.ReferenceImage = comp.ReferenceImage;


                frm.Show();
            }
            else
                Log("Done checking " + comp.NewImage + ". No errors detected.", false);

            RemoveImageProgress(comp.NewImage);
        }

        private void RemoveImageProgress(string sImage)
        {
            ImageProcessingProgress.Remove(sImage);
            cmbImagesInProcessing.Items.Remove(sImage);
            if(ImageProcessingProgress.Count == 0)
            {
                cmbImagesInProcessing.Enabled = false;
                pbImage1.Enabled = false;
                pbImage1.Value = 0;
            }
            else
            {
                cmbImagesInProcessing.SelectedIndex = 0;
            }
        }

        private void AddImageProgress(string sImage)
        {
            ImageProcessingProgress[sImage] = 0;
            cmbImagesInProcessing.Items.Add(sImage);
            cmbImagesInProcessing.Enabled = true;
            pbImage1.Enabled = true;

            if (cmbImagesInProcessing.SelectedIndex == -1)
            {
                cmbImagesInProcessing.SelectedIndex = 0;
            }
        }
        private void btnBrowseReference_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                DialogResult res = dlg.ShowDialog();
                if (res == DialogResult.OK)
                {
                    txtReference.Text = dlg.SelectedPath;
                    DirectoryInfo dir = new DirectoryInfo(dlg.SelectedPath);

                    foreach(FileInfo file in dir.GetFiles("*.jpg"))
                    {
                        ReferenceMarker marker = new ReferenceMarker();
                        marker.FileName = file.FullName;
                        var resRef = marker.ShowDialog();
                        if (resRef == DialogResult.OK)
                            ImageReferenceRectangle[file.Name] = marker.GetRectangle();
                        else
                            ImageReferenceRectangle.Remove(file.Name);
                    }

                    if (Listener == null)
                    {

                        Listener = new FolderChangeListener();
                        Listener.WorkerReportsProgress = true;
                        Listener.WorkerSupportsCancellation = true;

                        Listener.DoWork += Listener.Run;
                        Listener.ProgressChanged += NewFolderFound;
                        Listener.FolderName = txtFolder.Text;

                        Debug.WriteLine("Running listener");
                        Log("New folder detector started", false);
                        Listener.RunWorkerAsync();
                    }
                }
            }

        }

        private void cmbImagesInProcessing_SelectedIndexChanged(object sender, EventArgs e)
        {
            pbImage1.Value = ImageProcessingProgress[cmbImagesInProcessing.SelectedItem.ToString()];
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Settings set = new Settings();
            set.ShowDialog();
        }
    }
}