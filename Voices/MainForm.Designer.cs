namespace Voices
{
    partial class MainForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.subDescLabel = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.errorInfoGroup = new System.Windows.Forms.GroupBox();
            this.errorInfoSendState = new System.Windows.Forms.Label();
            this.sendInformation = new System.Windows.Forms.Button();
            this.errorInfoLabel = new System.Windows.Forms.LinkLabel();
            this.errorInfoText = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.headLabel = new System.Windows.Forms.Label();
            this.EndKrile = new System.Windows.Forms.Button();
            this.RestartKrile = new System.Windows.Forms.Button();
            this.aboutApp = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            this.errorInfoGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // subDescLabel
            // 
            this.subDescLabel.AutoSize = true;
            this.subDescLabel.Location = new System.Drawing.Point(68, 42);
            this.subDescLabel.Margin = new System.Windows.Forms.Padding(3);
            this.subDescLabel.Name = "subDescLabel";
            this.subDescLabel.Size = new System.Drawing.Size(352, 24);
            this.subDescLabel.TabIndex = 1;
            this.subDescLabel.Text = "{0}は、動作の過程で回復不可能なエラーに遭遇し、強制終了されました。\r\n{0}の改良のために、エラー情報の送信にご協力ください。";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Voices.Properties.Resources.error;
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(48, 48);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.errorInfoGroup);
            this.panel1.Controls.Add(this.headLabel);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.subDescLabel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(616, 307);
            this.panel1.TabIndex = 0;
            // 
            // errorInfoGroup
            // 
            this.errorInfoGroup.Controls.Add(this.errorInfoSendState);
            this.errorInfoGroup.Controls.Add(this.sendInformation);
            this.errorInfoGroup.Controls.Add(this.errorInfoLabel);
            this.errorInfoGroup.Controls.Add(this.errorInfoText);
            this.errorInfoGroup.Controls.Add(this.label3);
            this.errorInfoGroup.Location = new System.Drawing.Point(62, 79);
            this.errorInfoGroup.Margin = new System.Windows.Forms.Padding(5, 10, 5, 15);
            this.errorInfoGroup.Name = "errorInfoGroup";
            this.errorInfoGroup.Size = new System.Drawing.Size(539, 216);
            this.errorInfoGroup.TabIndex = 2;
            this.errorInfoGroup.TabStop = false;
            this.errorInfoGroup.Text = "エラー情報の送信";
            // 
            // errorInfoSendState
            // 
            this.errorInfoSendState.AutoSize = true;
            this.errorInfoSendState.ForeColor = System.Drawing.SystemColors.GrayText;
            this.errorInfoSendState.Location = new System.Drawing.Point(159, 189);
            this.errorInfoSendState.Name = "errorInfoSendState";
            this.errorInfoSendState.Size = new System.Drawing.Size(146, 12);
            this.errorInfoSendState.TabIndex = 4;
            this.errorInfoSendState.Text = "送信はまだ行われていません。";
            // 
            // sendInformation
            // 
            this.sendInformation.Location = new System.Drawing.Point(6, 181);
            this.sendInformation.Name = "sendInformation";
            this.sendInformation.Size = new System.Drawing.Size(147, 29);
            this.sendInformation.TabIndex = 3;
            this.sendInformation.Text = "情報の送信(&S)";
            this.sendInformation.UseVisualStyleBackColor = true;
            this.sendInformation.Click += new System.EventHandler(this.sendInformation_Click);
            // 
            // errorInfoLabel
            // 
            this.errorInfoLabel.AutoSize = true;
            this.errorInfoLabel.Location = new System.Drawing.Point(6, 155);
            this.errorInfoLabel.Margin = new System.Windows.Forms.Padding(3);
            this.errorInfoLabel.Name = "errorInfoLabel";
            this.errorInfoLabel.Size = new System.Drawing.Size(299, 12);
            this.errorInfoLabel.TabIndex = 2;
            this.errorInfoLabel.TabStop = true;
            this.errorInfoLabel.Text = "その他の送信される情報は、こちらをクリックすると確認できます...";
            this.errorInfoLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.errorInfoLabel_LinkClicked);
            // 
            // errorInfoText
            // 
            this.errorInfoText.Location = new System.Drawing.Point(8, 42);
            this.errorInfoText.Multiline = true;
            this.errorInfoText.Name = "errorInfoText";
            this.errorInfoText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.errorInfoText.Size = new System.Drawing.Size(516, 107);
            this.errorInfoText.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(425, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "以下に、エラー発生時の状況や、その他気付いた点などをご自由にお書きください。（任意）";
            // 
            // headLabel
            // 
            this.headLabel.AutoSize = true;
            this.headLabel.Font = new System.Drawing.Font("メイリオ", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.headLabel.ForeColor = System.Drawing.Color.DodgerBlue;
            this.headLabel.Location = new System.Drawing.Point(66, 12);
            this.headLabel.Margin = new System.Windows.Forms.Padding(3);
            this.headLabel.Name = "headLabel";
            this.headLabel.Size = new System.Drawing.Size(332, 24);
            this.headLabel.TabIndex = 0;
            this.headLabel.Text = "{0} twitter clientは強制終了されました。";
            // 
            // EndKrile
            // 
            this.EndKrile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.EndKrile.Location = new System.Drawing.Point(473, 319);
            this.EndKrile.Name = "EndKrile";
            this.EndKrile.Size = new System.Drawing.Size(131, 29);
            this.EndKrile.TabIndex = 2;
            this.EndKrile.Text = "再起動せず終了(X)";
            this.EndKrile.UseVisualStyleBackColor = true;
            this.EndKrile.Click += new System.EventHandler(this.EndKrile_Click);
            // 
            // RestartKrile
            // 
            this.RestartKrile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.RestartKrile.Location = new System.Drawing.Point(336, 319);
            this.RestartKrile.Name = "RestartKrile";
            this.RestartKrile.Size = new System.Drawing.Size(131, 29);
            this.RestartKrile.TabIndex = 1;
            this.RestartKrile.Text = "{0}の再起動(R)";
            this.RestartKrile.UseVisualStyleBackColor = true;
            this.RestartKrile.Click += new System.EventHandler(this.RestartKrile_Click);
            // 
            // aboutApp
            // 
            this.aboutApp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.aboutApp.Location = new System.Drawing.Point(12, 319);
            this.aboutApp.Name = "aboutApp";
            this.aboutApp.Size = new System.Drawing.Size(131, 29);
            this.aboutApp.TabIndex = 3;
            this.aboutApp.Text = "この機能について(I)...";
            this.aboutApp.UseVisualStyleBackColor = true;
            this.aboutApp.Click += new System.EventHandler(this.aboutApp_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(616, 360);
            this.Controls.Add(this.aboutApp);
            this.Controls.Add(this.RestartKrile);
            this.Controls.Add(this.EndKrile);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "{0} twitter clientは強制終了されました。";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.errorInfoGroup.ResumeLayout(false);
            this.errorInfoGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label subDescLabel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label headLabel;
        private System.Windows.Forms.Button EndKrile;
        private System.Windows.Forms.Button RestartKrile;
        private System.Windows.Forms.Button aboutApp;
        private System.Windows.Forms.GroupBox errorInfoGroup;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox errorInfoText;
        private System.Windows.Forms.LinkLabel errorInfoLabel;
        private System.Windows.Forms.Button sendInformation;
        private System.Windows.Forms.Label errorInfoSendState;
    }
}

