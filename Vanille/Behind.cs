using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;

namespace Vanille
{
    public partial class Behind : Form
    {
        public Behind()
        {
            InitializeComponent();
            this.Shown += new EventHandler(Behind_Shown);
        }

        private Process RunAsStart()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.WorkingDirectory = Environment.CurrentDirectory;
            startInfo.FileName = Application.ExecutablePath;
            var cmd = Environment.GetCommandLineArgs();
            // Command parameters is
            // [File], [Version], [UpdateTarget], [Process ID]
            if (cmd.Length < 4)
            {
#if DEBUG
                cmd = new[] { cmd[0], "0.00", "5", "0" };
#else
                MessageBox.Show("Information losted.", "Updater", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Application.Exit();
                return null;
#endif
            }
            startInfo.Arguments = cmd[1] + " " + cmd[2] + " " + cmd[3] + " runas";
            if (Environment.OSVersion.Version.Major >= 6)
                startInfo.Verb = "runas";
            try
            {
                return Process.Start(startInfo);
            }
            catch
            {
                return null;
            }
        }

        private void Behind_Shown(object sender, EventArgs e)
        {
            var cmd = Environment.GetCommandLineArgs();
#if !DEBUG
            if (cmd.Length < 2)
            {
                MessageBox.Show("Information losted.", "Updater", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Application.Exit();
                return;
            }
#endif
            var p = RunAsStart();
            while (p == null)
            {
                if (MessageBox.Show(
                    "Fail to start updater with elevated authority." + Environment.NewLine +
                    "Do you want to retry?",
                    "Krile updater - Execution error",
                     MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
                    break;
                p = RunAsStart();
            }
            if (p == null)
            {
                Application.Exit();
            }
            else
            {
                p.EnableRaisingEvents = true;
                p.SynchronizingObject = this;
                p.Exited += new EventHandler(p_Exited);
            }
        }

        void p_Exited(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Path.Combine(Application.StartupPath, Define.CallbackFile));
            }
            finally
            {
                Application.Exit();
            }
        }
    }
}
