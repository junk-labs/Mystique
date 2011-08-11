using System.Windows;
using System.Windows.Data;

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
