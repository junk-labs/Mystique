namespace Vanille.UpdateCore
{
    partial class Update
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
            this._label = new System.Windows.Forms.Label();
            this._progress = new System.Windows.Forms.ProgressBar();
            this.detail = new System.Windows.Forms.TextBox();
            this.CancelButton = new System.Windows.Forms.Button();
            this.UpdateLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _label
            // 
            this._label.Location = new System.Drawing.Point(12, 9);
            this._label.Name = "_label";
            this._label.Size = new System.Drawing.Size(520, 12);
            this._label.TabIndex = 0;
            this._label.Text = "Krileを更新しています。しばらくお待ちください...";
            // 
            // _progress
            // 
            this._progress.Location = new System.Drawing.Point(12, 26);
            this._progress.Name = "_progress";
            this._progress.Size = new System.Drawing.Size(520, 10);
            this._progress.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this._progress.TabIndex = 1;
            // 
            // detail
            // 
            this.detail.Location = new System.Drawing.Point(12, 40);
            this.detail.Multiline = true;
            this.detail.Name = "detail";
            this.detail.ReadOnly = true;
            this.detail.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.detail.Size = new System.Drawing.Size(520, 146);
            this.detail.TabIndex = 2;
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(392, 192);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(140, 29);
            this.CancelButton.TabIndex = 3;
            this.CancelButton.Text = "キャンセル";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // UpdateLabel
            // 
            this.UpdateLabel.AutoSize = true;
            this.UpdateLabel.Location = new System.Drawing.Point(12, 200);
            this.UpdateLabel.Name = "UpdateLabel";
            this.UpdateLabel.Size = new System.Drawing.Size(90, 12);
            this.UpdateLabel.TabIndex = 4;
            this.UpdateLabel.Text = "Krile Updater 3.0";
            // 
            // Update
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(544, 233);
            this.ControlBox = false;
            this.Controls.Add(this.UpdateLabel);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.detail);
            this.Controls.Add(this._progress);
            this.Controls.Add(this._label);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Update";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Krile Update";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _label;
        private System.Windows.Forms.ProgressBar _progress;
        private System.Windows.Forms.TextBox detail;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Label UpdateLabel;
    }
}