using System;
using System.Windows.Data;

namespace Mystique.Views.Converters.Particular
{
    public class StringRemoveLineFeedConverter : OneWayConverter<string, string>
    {
        public override string ToTarget(string input, object parameter)
        {
            return input.Replace("\r\n", "\n").Replace("\r", " ").Replace("\n", " ");
        }
    }
}
