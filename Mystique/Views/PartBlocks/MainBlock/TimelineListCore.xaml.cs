using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Inscribe.Common;
using Inscribe.Storage;
using Inscribe.ViewModels.PartBlocks.MainBlock;
using Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild;
using Mystique.Views.PartBlocks.MainBlock.TimelineChild;
using System.Windows.Documents;

namespace Mystique.Views.PartBlocks.MainBlock
{
    /// <summary>
    /// TimelineListCore.xaml の相互作用ロジック
    /// </summary>
    public partial class TimelineListCore : UserControl
    {
        public TimelineListCore()
        {
            InitializeComponent();
        }

        private void List_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var vm = this.DataContext as TimelineListCoreViewModel;
            if (vm == null) return;
            var sv = GetScrollViewer(this.List);
            if (sv != null)
                vm.ScrollIndex = (int)sv.VerticalOffset;
        }

        private ScrollViewer GetScrollViewer(DependencyObject o)
        {
            if (o == null)
                throw new ArgumentNullException("object is null.");
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

        #region Timeline Item Code Behinds

        private void NormalLayoutRoot_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateActualWidth(sender as Grid);
        }

        private void NormalLayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateActualWidth(sender as Grid);
        }

        private void UpdateActualWidth(Grid grid)
        {
            if (grid == null) return;
            var vm = grid.DataContext as TabDependentTweetViewModel;
            if (vm != null)
                vm.TooltipWidth = grid.ActualWidth;
        }

        #endregion

        #region Timeline-Selected Item Code Behinds

        bool captured = false;
        Point ip = new Point();
        MouseButtonEventArgs origEventArgs;
        private void ItemMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (captured)
                {
                    captured = false;
                }
                else
                {
                    origEventArgs = e;
                    captured = true;
                    ip = e.GetPosition(this);
                    e.Handled = true;
                }
            }
        }

        private void ItemMouseMove(object sender, MouseEventArgs e)
        {
            if (captured && e.GetPosition(this).DistanceDouble(ip) >= 4)
            {
                origEventArgs.Handled = false;
                var rtb = sender as DynamicRichTextBox;
                if (rtb == null) return;
                rtb.EntryOnMouseDown(origEventArgs);
            }
        }

        private void ItemMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.GetPosition(this).DistanceDouble(ip) < 4 &&
                captured)
            {
                var tm = ((FlowDocument)sender).DataContext as TabDependentTweetViewModel;
                if (tm != null && tm.SurfaceClickCommand.CanExecute)
                {
                    e.Handled = true;
                    tm.SurfaceClickCommand.Execute();
                }
                captured = false;
            }
        }

        private void cCopy_Click(object sender, RoutedEventArgs e)
        {
            var rtb = sender as DynamicRichTextBox;
            if (rtb == null) return;
            try
            {
                rtb.Copy();
                var cbtext = Clipboard.GetText();
                if (cbtext != null && cbtext.EndsWith(Environment.NewLine))
                    Clipboard.SetText(cbtext.Substring(0, cbtext.Length - 1));
            }
            catch (Exception ex)
            {
                ExceptionStorage.Register(ex, ExceptionCategory.UserError, "コピーに失敗しました。");
            }
        }

        private void cSelectAll_Click(object sender, RoutedEventArgs e)
        {
            var rtb = sender as DynamicRichTextBox;
            if (rtb == null) return;
            rtb.SelectAll();
        }

        private void cSearchInKrile_Click(object sender, RoutedEventArgs e)
        {
            var rtb = sender as DynamicRichTextBox;
            if (rtb == null) return;
            var tm = DataContext as TabDependentTweetViewModel;
            if (tm != null)
                tm.Parent.AddTopTimeline(new[] { new Inscribe.Filter.Filters.Text.FilterText(rtb.Selection.Text) });
        }

        private void cGoogle_Click(object sender, RoutedEventArgs e)
        {
            var cm = sender as ContextMenu;
            var rtb = cm.PlacementTarget as DynamicRichTextBox;
            // var rtb = sender as DynamicRichTextBox;
            if (rtb == null) return;
            try
            {
                Browser.Start(
                    "http://www.google.co.jp/search?q=" + rtb.Selection.Text);
                // Dulcet.Util.HttpUtility.UrlEncode(BodyText.Selection.Text));
            }
            catch { }
        }

        private void BodyText_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var rtb = sender as DynamicRichTextBox;
            if (rtb == null) return;
            var tvm = rtb.DataContext as TabDependentTweetViewModel;
            if (tvm == null) return;
            tvm.SelectedText = rtb.Selection != null ? rtb.Selection.Text : null;
        }

        #endregion
    }
}
