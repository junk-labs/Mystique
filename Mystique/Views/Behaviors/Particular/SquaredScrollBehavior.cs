using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using Inscribe.Configuration;

namespace Mystique.Views.Behaviors.Particular
{
    /// <summary>
    /// 倍速スクロール
    /// </summary>
    public class SquaredScrollBehavior : Behavior<ItemsControl>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.PreviewMouseWheel += PreviewMouseWheel;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.PreviewMouseWheel -= PreviewMouseWheel;
        }

        void PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // delta ** 2
            if (Setting.Instance.TimelineExperienceProperty.UseFastScrolling &&
                e.LeftButton != MouseButtonState.Pressed)
            {
                e.Handled = true;
                int dp = e.Delta / 120;
                var sv = GetScrollViewer(this.AssociatedObject);
                if (sv != null)
                {
                    sv.ScrollToVerticalOffset(sv.VerticalOffset - (dp * Math.Abs(dp)) * 3);
                }
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
            var sv = o as ScrollViewer;
            if (sv != null)
                return sv;

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
