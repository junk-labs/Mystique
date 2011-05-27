using System.Windows;
using System.Windows.Data;

namespace Mystique.Views.Converters
{
    class StringEqVisibleConverter : OneWayConverter<object, Visibility>
    {
        public override Visibility ToTarget(object input, object parameter)
        {
            return (parameter as string) == input.ToString() ?
                Visibility.Visible :
                Visibility.Collapsed;
        }
    }
}
