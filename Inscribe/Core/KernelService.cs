using System;
using System.Windows;
using Inscribe.Common;
using Inscribe.Configuration;
using Inscribe.ViewModels;
using Inscribe.ViewModels.PartBlocks.InputBlock;
using System.Windows.Documents;
using System.Collections.Generic;

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
            Setting.Instance.Save();
            ThreadHelper.HaltThreads();
            Application.Current.Dispatcher.InvokeShutdown();
            Application.Current.Shutdown();
        }

        public static bool IsAppInShutdown
        {
            get { return _isInShutdown; }
        }

        private static bool isStandby = false;
        private static List<PluginMenuItem> menuholder = new List<PluginMenuItem>();
        public static void AddMenu(string header, Action command)
        {
            if (header == null)
                throw new NullReferenceException("header");
            if (command == null)
                throw new NullReferenceException("command");
            if (isStandby)
            {
                MainWindowViewModel.InputBlockViewModel.pluginMenus.Add(new PluginMenuItem(header, command));
                MainWindowViewModel.InputBlockViewModel.RefreshPluginMenu();
            }
            else
            {
                menuholder.Add(new PluginMenuItem(header, command));
            }
        }

        internal static void CallbackWindowPrepared()
        {
            isStandby = true;
            menuholder.ForEach(i =>
                MainWindowViewModel.InputBlockViewModel.pluginMenus.Add(i));
            MainWindowViewModel.InputBlockViewModel.RefreshPluginMenu();
        }
    }
}
