using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Vanille
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            string[] args = Environment.GetCommandLineArgs();
            bool RunAsElevated = false;
            foreach (string s in args)
            {
                if (s.Equals("runas"))
                {
                    Application.Run(new UpdateCore.Update());
                    RunAsElevated = true;
                    break;
                }
            }
            if (!RunAsElevated)
                Application.Run(new Behind());
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            using (var sw = new System.IO.StreamWriter("upderr.txt"))
            {
                sw.WriteLine(e.ExceptionObject.ToString());
            }
            MessageBox.Show(
                "Fatal error has occured." + Environment.NewLine +
                "I wrote debug information in upderr.txt." + Environment.NewLine +
                "Please feedback this file to Krile Development Team.",
                "kup fatal error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            using (var sw = new System.IO.StreamWriter("upderr.txt"))
            {
                sw.WriteLine(e.Exception.ToString());
            }
            MessageBox.Show(
                "Fatal error has occured." + Environment.NewLine +
                "I wrote debug information in upderr.txt." + Environment.NewLine +
                "Please feedback this file to Krile Development Team.",
                "kup fatal error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }
    }
}
