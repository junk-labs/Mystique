using System.Windows;
using System.Windows.Controls;
using Inscribe.ViewModels.Common;

namespace Mystique.Views.Common
{
    /// <summary>
    /// UrlTooltip.xaml の相互作用ロジック
    /// </summary>
    public partial class UrlTooltip : UserControl
    {
        public UrlTooltip(string url)
        {
            InitializeComponent();
            this.DataContext = new UrlTooltipViewModel(url);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var dc = this.DataContext as UrlTooltipViewModel;
            if (dc != null)
                dc.BeginResolve();
        }
    }
}
