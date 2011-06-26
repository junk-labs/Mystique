using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Mystique.Views.Converters.Particular
{
    class TwoOrMoreVisibleConverter : OneWayConverter<int, Visibility>
    {
        public override Visibility ToTarget(int input, object parameter)
        {
            return input >= 2 ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
