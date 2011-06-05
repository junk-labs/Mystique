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

namespace Mystique.Views.Common
{
    /// <summary>
    /// ImageStackingView.xaml の相互作用ロジック
    /// </summary>
    public partial class ImageStackingView : UserControl
    {
        public ImageStackingView()
        {
            InitializeComponent();
        }
    }

    public class DoubleToMarginConverter : OneWayConverter<double, Thickness>
    {
        public override Thickness ToTarget(double input, object parameter)
        {
            return new Thickness(input, input, 0, 0);
        }
    }

}
