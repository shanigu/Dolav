namespace DetectLabelProblems
{
    partial class DetectionUI
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DetectionUI));
            this.label1 = new System.Windows.Forms.Label();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnBrowseReference = new System.Windows.Forms.Button();
            this.txtReference = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbMessages = new System.Windows.Forms.RichTextBox();
            this.pbImage1 = new System.Windows.Forms.ProgressBar();
            this.lblProgress = new System.Windows.Forms.Label();
            this.cmbImagesInProcessing = new System.Windows.Forms.ComboBox();
            this.mMain = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 19);
            this.label1.TabIndex = 0;
            this.label1.Text = "Base folder:";
            // 
            // txtFolder
            // 
            this.txtFolder.Location = new System.Drawing.Point(136, 40);
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.Size = new System.Drawing.Size(586, 26);
            this.txtFolder.TabIndex = 1;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(728, 40);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(44, 26);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // btnBrowseReference
            // 
            this.btnBrowseReference.Location = new System.Drawing.Point(728, 81);
            this.btnBrowseReference.Name = "btnBrowseReference";
            this.btnBrowseReference.Size = new System.Drawing.Size(44, 26);
            this.btnBrowseReference.TabIndex = 5;
            this.btnBrowseReference.Text = "...";
            this.btnBrowseReference.UseVisualStyleBackColor = true;
            this.btnBrowseReference.Click += new System.EventHandler(this.btnBrowseReference_Click);
            // 
            // txtReference
            // 
            this.txtReference.Location = new System.Drawing.Point(136, 81);
            this.txtReference.Name = "txtReference";
            this.txtReference.Size = new System.Drawing.Size(586, 26);
            this.txtReference.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 84);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(110, 19);
            this.label2.TabIndex = 3;
            this.label2.Text = "Reference folder:";
            // 
            // tbMessages
            // 
            this.tbMessages.Location = new System.Drawing.Point(22, 120);
            this.tbMessages.Name = "tbMessages";
            this.tbMessages.Size = new System.Drawing.Size(750, 318);
            this.tbMessages.TabIndex = 6;
            this.tbMessages.Text = "";
            // 
            // pbImage1
            // 
            this.pbImage1.Location = new System.Drawing.Point(22, 486);
            this.pbImage1.Name = "pbImage1";
            this.pbImage1.Size = new System.Drawing.Size(750, 23);
            this.pbImage1.TabIndex = 7;
            // 
            // lblProgress
            // 
            this.lblProgress.AutoSize = true;
            this.lblProgress.Location = new System.Drawing.Point(22, 456);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(144, 19);
            this.lblProgress.TabIndex = 9;
            this.lblProgress.Text = "Images in processing: ";
            // 
            // cmbImagesInProcessing
            // 
            this.cmbImagesInProcessing.FormattingEnabled = true;
            this.cmbImagesInProcessing.Location = new System.Drawing.Point(163, 453);
            this.cmbImagesInProcessing.Name = "cmbImagesInProcessing";
            this.cmbImagesInProcessing.Size = new System.Drawing.Size(609, 27);
            this.cmbImagesInProcessing.TabIndex = 10;
            this.cmbImagesInProcessing.SelectedIndexChanged += new System.EventHandler(this.cmbImagesInProcessing_SelectedIndexChanged);
            // 
            // mMain
            // 
            this.mMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.aboutToolStripMenuItem});
            this.mMain.Location = new System.Drawing.Point(0, 0);
            this.mMain.Name = "mMain";
            this.mMain.Size = new System.Drawing.Size(801, 27);
            this.mMain.TabIndex = 11;
            this.mMain.Text = "menuStrip1";
            this.mMain.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.mMain_ItemClicked);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(70, 23);
            this.toolStripMenuItem1.Text = "Settings";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(59, 23);
            this.aboutToolStripMenuItem.Text = "About";
            // 
            // DetectionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(801, 511);
            this.Controls.Add(this.cmbImagesInProcessing);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.pbImage1);
            this.Controls.Add(this.tbMessages);
            this.Controls.Add(this.btnBrowseReference);
            this.Controls.Add(this.txtReference);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtFolder);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.mMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DetectionUI";
            this.Text = "Label problem detector";
            this.Load += new System.EventHandler(this.DetectionUI_Load);
            this.mMain.ResumeLayout(false);
            this.mMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label label1;
        private TextBox txtFolder;
        private Button btnBrowse;
        private Button btnBrowseReference;
        private TextBox txtReference;
        private Label label2;
        private RichTextBox tbMessages;
        private ProgressBar pbImage1;
        private Label lblProgress;
        private ComboBox cmbImagesInProcessing;
        private MenuStrip mMain;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem aboutToolStripMenuItem;
    }
}