using System;
using System.IO;
using System.Text;
using System.Windows;
using Inscribe;
using Inscribe.Common;
using Inscribe.Core;
using Inscribe.Threading;
using Livet;

namespace Mystique
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Application.Current.Exit += new ExitEventHandler(Exitting);
            DispatcherHelper.UIDispatcher = Dispatcher;
#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
#endif
            Initializer.Init();
            UpdateReceiver.StartSchedule();
        }

        void Exitting(object sender, ExitEventArgs e)
        {
            ThreadHelper.HaltThreads();
        }

        //集約エラーハンドラ
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ERROR THROWN:");
            System.Diagnostics.Debug.WriteLine(e.ExceptionObject);
            StringBuilder body = new StringBuilder();
            body.AppendLine("********************************************************************************");
            body.AppendLine(" ERROR TRACE: " + DateTime.Now.ToString());
            body.AppendLine("********************************************************************************");
            body.AppendLine(e.ExceptionObject.ToString());
            body.AppendLine();
            body.AppendLine("MEMORY USAGE:");
            var cp = System.Diagnostics.Process.GetCurrentProcess();
            body.AppendLine("paged:" + cp.PagedMemorySize64 + " / peak-virtual:" + cp.PeakVirtualMemorySize64);

            var tpath = Path.GetTempFileName();
            using (var sw = new StreamWriter(tpath))
            {
                sw.WriteLine(body.ToString());
                sw.WriteLine(Define.GetFormattedVersion() + " @" + Define.GetExeFilePath());
                sw.WriteLine(Environment.OSVersion.VersionString);
            }
            var apppath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            System.Diagnostics.Process.Start(Path.Combine(apppath, Define.FeedbackAppName), tpath);
            Environment.Exit(1);
        }
    }
}
