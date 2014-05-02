using System;
using System.Windows;
using Inscribe;
using Inscribe.Common;
using Inscribe.Configuration;
using Inscribe.Core;
using Inscribe.Subsystems;
using Inscribe.Subsystems.KeyAssign;
using Nightmare.WinAPI;

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 設定のロード
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
            // 設定のバリデーション
            if (Setting.Instance.ColoringProperty.DefaultTextColor.A == 0x0)
            {
                var result = MessageBox.Show(this,
                    "色設定が破損している可能性があります。(本文のα値が0です)" + Environment.NewLine +
                    "配色設定を初期化しますか？", "色設定の復元", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (result == MessageBoxResult.Yes)
                {
                    Setting.Instance.ColoringProperty = new Inscribe.Configuration.Settings.ColoringProperty();
                    Setting.RaiseSettingValueChanged();
                }
            }

        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // コンテントロード完了
            if(Setting.Instance.KernelProperty.LastWriteVersion == 0)
            {
                if (!Define.IsOperatingSystemSupported)
                {
                    MessageBox.Show(this, "あなたのオペレーティングシステムでの動作は未確認です。" + Environment.NewLine +
                        "Windows Vista/7 での実行を強くお勧めします。", "未確認のWindows", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                var result = MessageBox.Show(this,
                    "Twitterクライアント \"Krile\"(クルル) をダウンロードしていただき、ありがとうございます。" + Environment.NewLine +
                    "ご利用になる前に、FAQに目を通していただくことをお勧めします。" + Environment.NewLine +
                    "(" + Define.FaqUrl + ")" + Environment.NewLine +
                    "今すぐFAQを参照しますか？", "Welcome to Krile",
                    MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    Browser.Start(Define.FaqUrl);
                }
            }
            else if (Define.GetNumericVersion() > Setting.Instance.KernelProperty.LastWriteVersion)
            {
                var result = MessageBox.Show(this,
                    "ご利用中のKrileの v" + Define.GetFormattedVersion() + " への更新が完了しました。" + Environment.NewLine +
                    "更新された点をWebで閲覧しますか？",
                    "アップデートの報告",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    if (Define.IsNightlyVersion)
                        Browser.Start(Define.NightlyReleaseNoteUrl);
                    else
                        Browser.Start(Define.ReleaseNoteUrl);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // シャットダウン
            if (!KernelService.IsAppInShutdown)
            {
                if (!User32.IsKeyPressed(VirtualKey.VK_SHIFT))
                {
                    if (MessageBox.Show(this, "Krileを終了してもよろしいですか？", "Krileの終了", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }

            if (Setting.IsInitialized)
            {
                var rect = Nightmare.WinAPI.NativeWindowControl.GetWindowPlacement(this);
                Setting.Instance.StateProperty.WindowPosition = rect;
                Setting.Instance.StateProperty.WindowState = this.WindowState;
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
