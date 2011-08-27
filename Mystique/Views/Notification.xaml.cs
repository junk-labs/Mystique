using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Inscribe.Configuration;
using Nightmare.Forms;
using Inscribe.Storage;

namespace Mystique.Views
{
    /// <summary>
    /// Notification.xaml の相互作用ロジック
    /// </summary>
    public partial class Notification : Window
    {
        #region Notification Window Manager

        private static List<Notification> notifications = new List<Notification>();

        private static object notifyLock = new object();

        #endregion

        private DateTime CreateDateTime = DateTime.Now;

        private static long globalIdentifierCount = 0;

        private long Identifier;

        public Notification()
        {
            InitializeComponent();
            this.Identifier = Interlocked.Increment(ref globalIdentifierCount);
            MeasureLocation();
            this.Owner = Application.Current.MainWindow;
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(Setting.Instance.NotificationProperty.NotifyWindowShowLength);
                this.Dispatcher.BeginInvoke(() => this.Close());
            });
        }

        private void MeasureLocation()
        {
            int offsetIndex = 0;
            // メインウィンドウの存在するウィンドウを取得
            Screen screen = null;
            if (Setting.Instance.NotificationProperty.NotifyInMainWindowDisplay)
                screen = Screen.FromHandle(new WindowInteropHelper(Application.Current.MainWindow).Handle);
            else
                screen = Screen.PrimaryScreen;

            // クリティカル セクション
            lock (notifyLock)
            {
                if (Setting.Instance.NotificationProperty.ShowMultiple)
                {
                    try
                    {
                        // ウィンドウ最大表示数を数える
                        // Height = (48 + 10) * x
                        // Margin:10px
                        offsetIndex = Enumerable.Range(0, (int)(screen.WorkingArea.Height / 58)).Concat(new[] { -1 })
                            .First(i => i == -1 ||
                                i == notifications.Count ||
                                notifications[i] == null);

                        if (offsetIndex == -1)
                        {
                            // overflow
                            offsetIndex = Enumerable.Range(0, notifications.Count)
                                .OrderBy(i => notifications[i].CreateDateTime).First();
                            notifications[offsetIndex].Close();
                        }

                        if (offsetIndex == notifications.Count)
                        {
                            notifications.Add(this);
                        }
                        else
                        {
                            notifications[offsetIndex] = this;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentOutOfRangeException("Debug?::NI::" + offsetIndex.ToString() + "/" + notifications.Count.ToString(), ex);
                    }
                }
                else
                {
                    if (notifications.Count > 0 && notifications[0] != null)
                        notifications[0].Close();
                }
            }

            if (Setting.Instance.NotificationProperty.NotifyLocation == Inscribe.Configuration.Settings.NotifyLocation.LeftTop ||
                Setting.Instance.NotificationProperty.NotifyLocation == Inscribe.Configuration.Settings.NotifyLocation.RightTop)
            {
                // Window location starts from top
                this.Top = screen.WorkingArea.Top + offsetIndex * (48 + 10) + 10;
            }
            else
            {
                // Window location starts from bottom
                this.Top = screen.WorkingArea.Bottom - offsetIndex * (48 + 10) - 58;
            }
            if (Setting.Instance.NotificationProperty.NotifyLocation == Inscribe.Configuration.Settings.NotifyLocation.LeftTop ||
                Setting.Instance.NotificationProperty.NotifyLocation == Inscribe.Configuration.Settings.NotifyLocation.LeftBottom)
            {
                // Window location in Left
                this.Left = screen.WorkingArea.Left + 10;
            }
            else
            {
                // Window location in Right
                this.Left = screen.WorkingArea.Right - 360;
            }
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            lock (notifyLock)
            {
                var idx = Enumerable.Range(0, notifications.Count).Concat(new[] { -1 })
                    .Where(i => i < 0 || (notifications[i] != null && notifications[i].Identifier == this.Identifier)).First();
                if (idx != -1)
                {
                    notifications[idx] = null;
                    System.Diagnostics.Debug.WriteLine(this.Identifier + " is successfully removed from " + idx + ".");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(this.Identifier + " is unreleased.");
                }
            }
        }
    }
}
