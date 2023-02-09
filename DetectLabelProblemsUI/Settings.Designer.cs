namespace DetectLabelProblems
{
    partial class Settings
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
            this.label1 = new System.Windows.Forms.Label();
            this.udRectSize = new System.Windows.Forms.NumericUpDown();
            this.udErrorThreshold = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.udMaxOffset = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.udBlackThreshold = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.udWhiteThreshold = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.udSmoothingArea = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.udRectSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udErrorThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udMaxOffset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udBlackThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udWhiteThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udSmoothingArea)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 19);
            this.label1.TabIndex = 1;
            this.label1.Text = "Rectangle size:";
            // 
            // udRectSize
            // 
            this.udRectSize.Location = new System.Drawing.Point(175, 17);
            this.udRectSize.Name = "udRectSize";
            this.udRectSize.Size = new System.Drawing.Size(94, 26);
            this.udRectSize.TabIndex = 2;
            // 
            // udErrorThreshold
            // 
            this.udErrorThreshold.Location = new System.Drawing.Point(175, 65);
            this.udErrorThreshold.Name = "udErrorThreshold";
            this.udErrorThreshold.Size = new System.Drawing.Size(94, 26);
            this.udErrorThreshold.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(141, 19);
            this.label2.TabIndex = 3;
            this.label2.Text = "Error pixels threshold:";
            // 
            // udMaxOffset
            // 
            this.udMaxOffset.Location = new System.Drawing.Point(175, 113);
            this.udMaxOffset.Name = "udMaxOffset";
            this.udMaxOffset.Size = new System.Drawing.Size(94, 26);
            this.udMaxOffset.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 115);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 19);
            this.label3.TabIndex = 5;
            this.label3.Text = "Max offset:";
            // 
            // udBlackThreshold
            // 
            this.udBlackThreshold.Location = new System.Drawing.Point(175, 161);
            this.udBlackThreshold.Name = "udBlackThreshold";
            this.udBlackThreshold.Size = new System.Drawing.Size(94, 26);
            this.udBlackThreshold.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 163);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 19);
            this.label4.TabIndex = 7;
            // 
            // udWhiteThreshold
            // 
            this.udWhiteThreshold.Location = new System.Drawing.Point(175, 209);
            this.udWhiteThreshold.Name = "udWhiteThreshold";
            this.udWhiteThreshold.Size = new System.Drawing.Size(94, 26);
            this.udWhiteThreshold.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 211);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(110, 19);
            this.label5.TabIndex = 9;
            this.label5.Text = "White threshold:";
            // 
            // udSmoothingArea
            // 
            this.udSmoothingArea.Location = new System.Drawing.Point(175, 257);
            this.udSmoothingArea.Name = "udSmoothingArea";
            this.udSmoothingArea.Size = new System.Drawing.Size(94, 26);
            this.udSmoothingArea.TabIndex = 12;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 259);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(109, 19);
            this.label6.TabIndex = 11;
            this.label6.Text = "Smoothing area:";
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(84, 324);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(68, 29);
            this.btnOk.TabIndex = 13;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(175, 324);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(68, 29);
            this.btnCancel.TabIndex = 14;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 163);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(105, 19);
            this.label7.TabIndex = 15;
            this.label7.Text = "Black threshold:";
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(281, 362);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.udSmoothingArea);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.udWhiteThreshold);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.udBlackThreshold);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.udMaxOffset);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.udErrorThreshold);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.udRectSize);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Settings";
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.Settings_Load);
            ((System.ComponentModel.ISupportInitialize)(this.udRectSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udErrorThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udMaxOffset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udBlackThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udWhiteThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udSmoothingArea)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Label label1;
        private NumericUpDown udRectSize;
        private NumericUpDown udErrorThreshold;
        private Label label2;
        private NumericUpDown udMaxOffset;
        private Label label3;
        private NumericUpDown udBlackThreshold;
        private Label label4;
        private NumericUpDown udWhiteThreshold;
        private Label label5;
        private NumericUpDown udSmoothingArea;
        private Label label6;
        private Button btnOk;
        private Button btnCancel;
        private Label label7;
    }
}