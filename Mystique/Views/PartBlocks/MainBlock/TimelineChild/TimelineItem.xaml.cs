using System.Windows.Controls;
using Inscribe.ViewModels.Timeline;

namespace Mystique.Views.PartBlocks.MainBlock.TimelineChild
{
    /// <summary>
    /// TimelineItem.xaml の相互作用ロジック
    /// </summary>
    public partial class TimelineItem : UserControl
    {
        public TimelineItem()
        {
            InitializeComponent();
            this.DataContextChanged += (o, e) => RefreshActualWidth();
            this.LayoutRoot.SizeChanged += (o, e) => RefreshActualWidth();
        }

        private void RefreshActualWidth()
        {
            var vm = this.DataContext as TabDependentTweetViewModel;
            if (vm != null)
                vm.TooltipWidth = LayoutRoot.ActualWidth;
        }
    }
}
