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

namespace Mystique.Views.PartBlocks.NotifyBlock
{
    /// <summary>
    /// NotifyBlock.xaml の相互作用ロジック
    /// </summary>
    public partial class NotifyBlock : UserControl
    {
        public NotifyBlock()
        {
            InitializeComponent();
            EventPopupButton.SizeChanged += (o, e) => UpdateEventPopupWidth();
            UpdateEventPopupWidth();
        }

        private void UpdateEventPopupWidth()
        {
            EventPopup.Width = EventPopupButton.ActualWidth;
        }
    }

    public class MaximizeToInvisibleConverter : OneWayConverter<WindowState, Visibility>
    {
        public override Visibility ToTarget(WindowState input, object parameter)
        {
            return input == WindowState.Maximized ? Visibility.Collapsed : Visibility.Visible;
        }
    }

}
