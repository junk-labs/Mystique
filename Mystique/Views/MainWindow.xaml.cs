using System.Windows;
using Inscribe.Configuration;
using Inscribe.Core;
using Inscribe.Subsystems;
using Inscribe.Subsystems.KeyAssign;
using Inscribe;
using System;
using Inscribe.Common;

namespace Mystique.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // ウィンドウ設定の読み込みと保存

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("IME:" + this.IsInputMethodEnabled);
            if (!Setting.Instance.StateProperty.WindowPosition.IsEmpty)
            {
                /*
                this.Left = Setting.Instance.StateProperty.WindowPosition.Left;
                this.Top = Setting.Instance.StateProperty.WindowPosition.Top;
                this.Width = Setting.Instance.StateProperty.WindowPosition.Width;
                this.Height = Setting.Instance.StateProperty.WindowPosition.Height;
                */
                Nightmare.WinAPI.NativeWindowControl.SetWindowPlacement(this, Setting.Instance.StateProperty.WindowPosition);
                this.WindowState = Setting.Instance.StateProperty.WindowState;
            }
            if (Setting.Instance.KernelProperty.LastWriteVersion != 0 && Define.GetNumericVersion() > Setting.Instance.KernelProperty.LastWriteVersion)
            {
                var result = MessageBox.Show(this,
                    "ご利用中のKrileの v" + Define.GetFormattedVersion() + " への更新が完了しました。" + Environment.NewLine +
                    "更新された点をWebで閲覧しますか？",
                    "アップデートの報告",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Browser.Start(Define.ReleaseNoteUrl);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (KernelService.IsAppInShutdown ||
                MessageBox.Show(this, "Krileを終了してもよろしいですか？", "Krileの終了", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                if (Setting.IsInitialized)
                {
                    var rect = Nightmare.WinAPI.NativeWindowControl.GetWindowPlacement(this);
                    Setting.Instance.StateProperty.WindowPosition = rect;
                    Setting.Instance.StateProperty.WindowState = this.WindowState;
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            KeyAssignCore.HandleEvent(e, AssignRegion.General);
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            KeyAssignCore.HandlePreviewEvent(e, AssignRegion.General);
        }
    }
}
