using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Mystique.Views.Converters
{
    public class DoubleToGridLengthConverter : OneWayConverter<double, GridLength>
    {
        public override GridLength ToTarget(double input, object parameter)
        {
            return new GridLength(input);
        }
    }
}
