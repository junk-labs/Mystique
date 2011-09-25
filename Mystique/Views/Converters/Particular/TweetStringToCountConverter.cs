using System.Windows.Data;
using Inscribe;
using Inscribe.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mystique.Views.Converters.Particular
{
    public class TweetStringToCountConverter : OneWayConverter<string, int>
    {
        public override int ToTarget(string input, object parameter)
        {
            return TwitterDefine.TweetMaxLength - TweetTextCounter.Count(input);
        }
    }
}
