namespace HCVNC
{
    partial class ScreenBox
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.OSPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.OSPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // OSPictureBox
            // 
            this.OSPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OSPictureBox.BackColor = System.Drawing.Color.Black;
            this.OSPictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.OSPictureBox.Location = new System.Drawing.Point(0, 0);
            this.OSPictureBox.Name = "OSPictureBox";
            this.OSPictureBox.Size = new System.Drawing.Size(792, 466);
            this.OSPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.OSPictureBox.TabIndex = 0;
            this.OSPictureBox.TabStop = false;
            // 
            // OneScreenPicBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.OSPictureBox);
            this.Name = "OneScreenPicBox";
            this.Size = new System.Drawing.Size(792, 466);
            this.Resize += new System.EventHandler(this.ResizeSelf);
            ((System.ComponentModel.ISupportInitialize)(this.OSPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.PictureBox OSPictureBox;
    }
}
