using System.Windows;
using System.Windows.Data;

namespace Mystique.Views.Converters
{
    public class BooleanToVisibleConverter : OneWayConverter<bool, Visibility>
    {
        public override Visibility ToTarget(bool input, object parameter)
        {
            return input ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public class BooleanToInvisibleConverter : OneWayConverter<bool, Visibility>
    {
        public override Visibility ToTarget(bool input, object parameter)
        {
            return input ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
