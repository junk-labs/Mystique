using System;
using System.Windows;
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
            Application.Current.Shutdown();
        }

        public static bool IsAppInShutdown
        {
            get { return _isInShutdown; }
        }
    }
}
