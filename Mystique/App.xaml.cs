using System;
using System.Windows;
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
#if RELEASE
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
#endif
            Initializer.Init();
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

            // Environment.Exit(1);
        }
    }
}
