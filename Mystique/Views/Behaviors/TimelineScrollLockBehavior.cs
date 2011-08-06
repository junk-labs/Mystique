using System.Collections.Specialized;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Threading;
using Inscribe.Configuration;
using Inscribe.Configuration.Settings;
using Inscribe.ViewModels.PartBlocks.MainBlock;
using Mystique.Views.PartBlocks.MainBlock;

namespace Mystique.Views.Behaviors
{
    public class TimelineScrollLockBehavior : Behavior<TimelineListCore>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        void AssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.AssociatedObject.Loaded -= AssociatedObject_Loaded;
            this.AssociatedObject.List.DataContextChanged += List_DataContextChanged;
            UpdateDataContext();
        }

        void List_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            UpdateDataContext();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.Loaded -= AssociatedObject_Loaded;
            this.AssociatedObject.List.DataContextChanged -= List_DataContextChanged;
            base.OnDetaching();
        }

        INotifyCollectionChanged prevCollection = null;
        private void UpdateDataContext()
        {
            if(this.prevCollection != null)
                this.prevCollection.CollectionChanged -= prevCollection_CollectionChanged;
            var vm = this.AssociatedObject.DataContext as TimelineListCoreViewModel;
            if (vm == null)
                this.prevCollection = null;
            else
                this.prevCollection = vm.TweetsSource as INotifyCollectionChanged;
            if(this.prevCollection != null)
                this.prevCollection.CollectionChanged += prevCollection_CollectionChanged;
        }

        void prevCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var vm = this.AssociatedObject.DataContext as TimelineListCoreViewModel;
                switch (Setting.Instance.TimelineExperienceProperty.ScrollLockMode)
                {
                    case ScrollLock.None:
                        // ロックなし
                        return;
                    case ScrollLock.OnMouseCaptured:
                        // マウスキャプチャ時はロックする
                        if (!vm.Parent.IsMouseOver) return;
                        break;
                    //case TimelineExperienceProperty.ScrollLock.OnScrolled:
                    // リストのV位置が先頭に無い場合はロックする
                    // Dispatcherに問い合わせないといけないためコストがかかりすぎる
                    // →保留
                    // return;
                    case ScrollLock.OnSelected:
                        if (vm.SelectedTweetViewModel == null) return;
                        break;
                    case ScrollLock.Always:
                        // 常にスクロールロック
                        break;
                }
                // scroll down
                SetScroll(e.NewItems.Count);
            }
        }

        // Dispatcherの負荷を軽減したい

        /// <summary>
        /// スクロール待ちのオブジェクト数<para />
        /// 0ならInvokeされていない
        /// </summary>
        int scrollWaitCount = 0;

        private void SetScroll(int distance)
        {
            int nv = Interlocked.Increment(ref scrollWaitCount);
            if (nv == 1)
            {
                // キューされている操作はなし
                Dispatcher.BeginInvoke(SetScrollSynchronized, DispatcherPriority.DataBind);
            }
        }

        private void SetScrollSynchronized()
        {
            var sv = GetScrollViewer(this.AssociatedObject.List);
            var move = sv.VerticalOffset + Interlocked.Exchange(ref scrollWaitCount, 0);
            if (sv != null)
            {
                sv.ScrollToVerticalOffset(move);
                System.Diagnostics.Debug.WriteLine("Scroll:" + move);
            }
        }

        /// <summary>
        /// 指定されたListBoxからScrollViewerを取得します。
        /// </summary>
        /// <param name="child">対象ListBox</param>
        /// <returns>ScrollViewerインスタンス またはnull</returns>
        private ScrollViewer GetScrollViewer(DependencyObject o)
        {
            // Return the DependencyObject if it is a ScrollViewer
            if (o is ScrollViewer)
            {
                return o as ScrollViewer;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }
            return null;
        }
    }
}
