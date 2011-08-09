using System.Windows.Data;
using Inscribe.Subsystems;

namespace Mystique.Views.Converters.KeyAssign
{
    public class KeyAssignConverter : OneWayConverter<object, string>
    {
        public override string ToTarget(object input, object parameter)
        {
            var id = parameter as string;
            return KeyAssignCore.LookupKeyFromId(id);
        }
    }
}
