using System;
using System.Windows;
using Inscribe.Core;
using Inscribe.Threading;
using Livet;
using System.Text;
using System.IO;
using Inscribe;
using Inscribe.Filter.Core;
using Mystique.Update;

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
            UpdateReceiver.Start();
        }

        void Exitting(object sender, ExitEventArgs e)
        {
            ThreadHelper.HaltThreads();
        }

        //集約エラーハンドラ
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //TODO:ロギング処理など
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
            System.IO.File.AppendAllText(Path.Combine(Path.GetDirectoryName(Define.GetExecutingPath()), "unhandled.txt"), body.ToString());
            // Environment.Exit(1);
        }
    }
}
