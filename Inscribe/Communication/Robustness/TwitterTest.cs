using Dulcet.Twitter.Rest;
using Inscribe.Storage;

namespace Inscribe.Communication.Robustness
{
    public class TwitterTest : TestBase
    {
        protected override bool Try()
        {
            try
            {
                return AccountStorage.GetRandom().Test();
            }
            catch { return false; }
        }
    }
}
