using System;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vanille.UpdateCore
{
    public partial class Update : Form
    {
        private UpdateCoreProcessor core;

        public Update()
        {
            InitializeComponent();
            core = new UpdateCoreProcessor(Write);
            this.Load += new EventHandler(Update_Load);
            this.UpdateLabel.Text = "Krile Update v" + core.Version.ToString("0.0");
            core.SetCancellable += new Action<bool>(core_SetCancellable);
        }

        void Write(string text)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(Write), text);
            }
            else
            {
                detail.AppendText(text);
                Application.DoEvents();
            }
        }

        void Update_Load(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    core.BeginUpdate();
                    WriteLine("Wait for starting NEW Krile...");
                    System.IO.File.Create(Application.ExecutablePath + ".completed");
                    System.Threading.Thread.Sleep(100);
                    Application.Exit();
                }
                catch (SecurityException sex)
                {
                    MessageBox.Show("シグネチャが一致しません。" + Environment.NewLine +
                        "不正なバイナリをダウンロードしたか、もしくは更新手段に変更があります。" + Environment.NewLine +
                        "お手数ですが、手動での更新をお願いします。" + Environment.NewLine +
                        "message:" + sex.Message, "更新エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
                catch (Exception ex)
                {
                    WriteLine("ERROR THROWN:-------------------------------------------------");
                    WriteLine(ex.ToString());
                    MessageBox.Show("エラーが発生しました。" + Environment.NewLine +
                        ex.Message, "更新エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
            });
        }

        void core_SetCancellable(bool obj)
        {
            this.CancelButton.Enabled = obj;
        }

        private void WriteLine(string p)
        {
            Write(p + Environment.NewLine);
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
