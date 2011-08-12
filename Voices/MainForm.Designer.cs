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
            this.headLabel = new System.Windows.Forms.Label();
            this.EndKrile = new System.Windows.Forms.Button();
            this.RestartKrile = new System.Windows.Forms.Button();
            this.aboutApp = new System.Windows.Forms.Button();
            this.errorInfoLabel = new System.Windows.Forms.LinkLabel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.sendState = new System.Windows.Forms.Label();
            this.sendProgress = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // subDescLabel
            // 
            this.subDescLabel.AutoSize = true;
            this.subDescLabel.Location = new System.Drawing.Point(68, 42);
            this.subDescLabel.Margin = new System.Windows.Forms.Padding(3);
            this.subDescLabel.Name = "subDescLabel";
            this.subDescLabel.Size = new System.Drawing.Size(453, 24);
            this.subDescLabel.TabIndex = 1;
            this.subDescLabel.Text = "{0}は、動作中に内部エラーを検知し、強制終了しました。お手数をおかけし申し訳ございません。\r\n{0}の改良のために、エラー情報の送信にご協力ください。";
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
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.headLabel);
            this.panel1.Controls.Add(this.errorInfoLabel);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.subDescLabel);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(545, 186);
            this.panel1.TabIndex = 0;
            // 
            // headLabel
            // 
            this.headLabel.AutoSize = true;
            this.headLabel.Font = new System.Drawing.Font("メイリオ", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.headLabel.ForeColor = System.Drawing.Color.DodgerBlue;
            this.headLabel.Location = new System.Drawing.Point(66, 12);
            this.headLabel.Margin = new System.Windows.Forms.Padding(3);
            this.headLabel.Name = "headLabel";
            this.headLabel.Size = new System.Drawing.Size(222, 24);
            this.headLabel.TabIndex = 0;
            this.headLabel.Text = "{0} は強制終了されました。";
            // 
            // EndKrile
            // 
            this.EndKrile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.EndKrile.Location = new System.Drawing.Point(401, 192);
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
            this.RestartKrile.Location = new System.Drawing.Point(264, 192);
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
            this.aboutApp.Location = new System.Drawing.Point(12, 192);
            this.aboutApp.Name = "aboutApp";
            this.aboutApp.Size = new System.Drawing.Size(131, 29);
            this.aboutApp.TabIndex = 3;
            this.aboutApp.Text = "この機能について(I)...";
            this.aboutApp.UseVisualStyleBackColor = true;
            this.aboutApp.Click += new System.EventHandler(this.aboutApp_Click);
            // 
            // errorInfoLabel
            // 
            this.errorInfoLabel.AutoSize = true;
            this.errorInfoLabel.Location = new System.Drawing.Point(68, 72);
            this.errorInfoLabel.Margin = new System.Windows.Forms.Padding(3);
            this.errorInfoLabel.Name = "errorInfoLabel";
            this.errorInfoLabel.Size = new System.Drawing.Size(258, 12);
            this.errorInfoLabel.TabIndex = 2;
            this.errorInfoLabel.TabStop = true;
            this.errorInfoLabel.Text = "送信される情報は、こちらをクリックすると確認できます...";
            this.errorInfoLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.errorInfoLabel_LinkClicked);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.sendProgress);
            this.groupBox1.Controls.Add(this.sendState);
            this.groupBox1.Location = new System.Drawing.Point(70, 100);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(462, 68);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "エラー情報の送信状態";
            // 
            // sendState
            // 
            this.sendState.AutoSize = true;
            this.sendState.Location = new System.Drawing.Point(4, 25);
            this.sendState.Name = "sendState";
            this.sendState.Size = new System.Drawing.Size(71, 12);
            this.sendState.TabIndex = 0;
            this.sendState.Text = "送信準備中...";
            // 
            // sendProgress
            // 
            this.sendProgress.Location = new System.Drawing.Point(6, 40);
            this.sendProgress.Name = "sendProgress";
            this.sendProgress.Size = new System.Drawing.Size(450, 13);
            this.sendProgress.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.sendProgress.TabIndex = 1;
            this.sendProgress.Value = 100;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(544, 233);
            this.Controls.Add(this.aboutApp);
            this.Controls.Add(this.RestartKrile);
            this.Controls.Add(this.EndKrile);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "{0} Error Reporter";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
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
        private System.Windows.Forms.LinkLabel errorInfoLabel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ProgressBar sendProgress;
        private System.Windows.Forms.Label sendState;
    }
}

