using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Inscribe.ViewModels.PartBlocks.MainBlock;

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
    }
}
