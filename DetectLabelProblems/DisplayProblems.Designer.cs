namespace DetectLabelProblems
{
    partial class DisplayProblems
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DisplayProblems));
            this.btnClose = new System.Windows.Forms.Button();
            this.picReference = new System.Windows.Forms.PictureBox();
            this.picProblematic = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picReference)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picProblematic)).BeginInit();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(891, 473);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 31);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // picReference
            // 
            this.picReference.Location = new System.Drawing.Point(12, 47);
            this.picReference.Name = "picReference";
            this.picReference.Size = new System.Drawing.Size(466, 419);
            this.picReference.TabIndex = 1;
            this.picReference.TabStop = false;
            // 
            // picProblematic
            // 
            this.picProblematic.Location = new System.Drawing.Point(500, 47);
            this.picProblematic.Name = "picProblematic";
            this.picProblematic.Size = new System.Drawing.Size(466, 419);
            this.picProblematic.TabIndex = 2;
            this.picProblematic.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 19);
            this.label1.TabIndex = 3;
            this.label1.Text = "Reference:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(500, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 19);
            this.label2.TabIndex = 4;
            this.label2.Text = "Problematic:";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(810, 473);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 31);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // DisplayProblems
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(976, 516);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.picProblematic);
            this.Controls.Add(this.picReference);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DisplayProblems";
            this.Text = "DisplayProblems";
            this.Load += new System.EventHandler(this.DisplayProblems_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picReference)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picProblematic)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button btnClose;
        private PictureBox picReference;
        private PictureBox picProblematic;
        private Label label1;
        private Label label2;
        private Button btnSave;
    }
}