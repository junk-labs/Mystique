using System.Windows.Data;
using Inscribe;

namespace Mystique.Views.Converters.Particular
{
    class TweetStringToCountConverter : OneWayConverter<string, int>
    {
        public override int ToTarget(string input, object parameter)
        {
            return TwitterDefine.TweetMaxLength - input.Length;
        }
    }
}
