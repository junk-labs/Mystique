using System.Windows.Data;

namespace Mystique.Views.Converters.KeyAssign
{
    public class KeyAssignConverter : OneWayConverter<object, string>
    {
        public override string ToTarget(object input, object parameter)
        {
            var id = parameter as string;
            return Inscribe.Configuration.KeyAssignment.KeyAssign.LookupKeyFromId(id);
        }
    }
}
