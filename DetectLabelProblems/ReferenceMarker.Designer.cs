namespace DetectLabelProblems
{
    partial class ReferenceMarker
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.picReference = new System.Windows.Forms.PictureBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnSkip = new System.Windows.Forms.Button();
            this.lblFileName = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picReference)).BeginInit();
            this.SuspendLayout();
            // 
            // picReference
            // 
            this.picReference.Location = new System.Drawing.Point(12, 35);
            this.picReference.Name = "picReference";
            this.picReference.Size = new System.Drawing.Size(791, 433);
            this.picReference.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picReference.TabIndex = 0;
            this.picReference.TabStop = false;
            this.picReference.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picReference_MouseDown);
            this.picReference.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picReference_MouseMove);
            this.picReference.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picReference_MouseUp);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(728, 474);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 36);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnSkip
            // 
            this.btnSkip.Location = new System.Drawing.Point(647, 474);
            this.btnSkip.Name = "btnSkip";
            this.btnSkip.Size = new System.Drawing.Size(75, 36);
            this.btnSkip.TabIndex = 2;
            this.btnSkip.Text = "Skip";
            this.btnSkip.UseVisualStyleBackColor = true;
            this.btnSkip.Click += new System.EventHandler(this.btnSkip_Click);
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.Location = new System.Drawing.Point(12, 13);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(0, 19);
            this.lblFileName.TabIndex = 3;
            // 
            // ReferenceMarker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(815, 515);
            this.Controls.Add(this.lblFileName);
            this.Controls.Add(this.btnSkip);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.picReference);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ReferenceMarker";
            this.Text = "Mark reference area";
            this.Load += new System.EventHandler(this.ReferenceMarker_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picReference)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private PictureBox picReference;
        private Button btnOk;
        private Button btnSkip;
        private Label lblFileName;
    }
}