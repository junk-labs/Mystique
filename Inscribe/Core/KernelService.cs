using System;
using System.Windows;
using Inscribe.Common;
using Inscribe.Configuration;
using Inscribe.ViewModels;

namespace Inscribe.Core
{
    public static class KernelService
    {
        private static MainWindowViewModel _mainWindowViewModel = null;
        public static MainWindowViewModel MainWindowViewModel
        {
            get { return _mainWindowViewModel; }
            set
            {
                if (_mainWindowViewModel == null)
                    _mainWindowViewModel = value;
                else
                    throw new FieldAccessException("Field is already initialized.");
            }
        }

        private static bool _isInShutdown = false;
        public static void AppShutdown()
        {
            _isInShutdown = true;
            if (Setting.IsInitialized)
            {
                var rect = Nightmare.WinAPI.NativeWindowControl.GetWindowPlacement(Application.Current.MainWindow);
                Setting.Instance.StateProperty.WindowPosition = rect;
                Setting.Instance.StateProperty.WindowState = Application.Current.MainWindow.WindowState;
            }
            Setting.Instance.KernelProperty.KillByErrorCount = 0;
            Setting.Instance.Save();
            ThreadHelper.HaltThreads();
            Application.Current.Dispatcher.InvokeShutdown();
            Application.Current.Shutdown();
        }

        public static bool IsAppInShutdown
        {
            get { return _isInShutdown; }
        }
    }
}
