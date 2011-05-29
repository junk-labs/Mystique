using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace Mystique.Views.Converters.Particular
{
    public class CollectionToCountConverter : OneWayConverter<IEnumerable<object>, int>
    {
        public override int ToTarget(IEnumerable<object> input, object parameter)
        {
            return input.Count();
        }
    }
}
