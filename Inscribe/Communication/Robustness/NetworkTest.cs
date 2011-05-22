using System.Net.NetworkInformation;

namespace Inscribe.Communication.Robustness
{
    public class NetworkTest : TestBase
    {
        protected override bool Try()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }
    }
}
