namespace Vanille
{
    partial class Behind
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
            this.descLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // descLabel
            // 
            this.descLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.descLabel.Location = new System.Drawing.Point(10, 10);
            this.descLabel.Margin = new System.Windows.Forms.Padding(0);
            this.descLabel.Name = "descLabel";
            this.descLabel.Size = new System.Drawing.Size(264, 41);
            this.descLabel.TabIndex = 0;
            this.descLabel.Text = "Krileを更新しています。しばらくお待ちください...\r\nNow updating your krile. Take a while...";
            this.descLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Behind
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(284, 61);
            this.ControlBox = false;
            this.Controls.Add(this.descLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Behind";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Krile Automatic Updater";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label descLabel;
    }
}