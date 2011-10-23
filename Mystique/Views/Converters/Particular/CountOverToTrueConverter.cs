using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Mystique.Views.Converters.Particular
{
    public class CountOverToTrueConverter : OneWayConverter<int, bool>
    {
        public override bool ToTarget(int input, object parameter)
        {
            return input > int.Parse(parameter as string);
        }
    }
}
