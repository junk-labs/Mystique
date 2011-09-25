using System.Windows;
using System.Windows.Data;

namespace Mystique.Views.Converters.Particular
{
    public class NotZeroToVisibleConverter : OneWayConverter<int, Visibility>
    {
        public override Visibility ToTarget(int input, object parameter)
        {
            return input == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
