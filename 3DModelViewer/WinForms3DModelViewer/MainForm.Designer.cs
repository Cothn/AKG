namespace WinForms3DModelViewer
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            this.pictureBoxPaintArea = new System.Windows.Forms.PictureBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.LskippedPixelsDraw = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPaintArea)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxPaintArea
            // 
            this.pictureBoxPaintArea.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxPaintArea.Location = new System.Drawing.Point(12, 16);
            this.pictureBoxPaintArea.Name = "pictureBoxPaintArea";
            this.pictureBoxPaintArea.Size = new System.Drawing.Size(798, 581);
            this.pictureBoxPaintArea.TabIndex = 0;
            this.pictureBoxPaintArea.TabStop = false;
            this.pictureBoxPaintArea.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBoxPaintArea_MouseDown);
            this.pictureBoxPaintArea.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBoxPaintArea_MouseMove);
            this.pictureBoxPaintArea.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureBoxPaintArea_MouseUp);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 5;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // LskippedPixelsDraw
            // 
            this.LskippedPixelsDraw.AutoSize = true;
            this.LskippedPixelsDraw.Location = new System.Drawing.Point(12, 0);
            this.LskippedPixelsDraw.Name = "LskippedPixelsDraw";
            this.LskippedPixelsDraw.Size = new System.Drawing.Size(35, 13);
            this.LskippedPixelsDraw.TabIndex = 1;
            this.LskippedPixelsDraw.Text = "label1";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(822, 609);
            this.Controls.Add(this.LskippedPixelsDraw);
            this.Controls.Add(this.pictureBoxPaintArea);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResizeEnd += new System.EventHandler(this.MainForm_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MainForm_KeyPress);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPaintArea)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxPaintArea;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label LskippedPixelsDraw;
    }
}