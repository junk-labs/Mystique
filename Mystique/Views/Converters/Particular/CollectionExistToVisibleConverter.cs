using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Mystique.Views.Converters.Particular
{
    public class CollectionExistToVisibleConverter : OneWayConverter<IEnumerable<object>, Visibility>
    {
        public override Visibility ToTarget(IEnumerable<object> input, object parameter)
        {
            return input.Count() > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
