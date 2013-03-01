using System;
using Dulcet.Twitter.Rest;
using Inscribe.Storage;

namespace Inscribe.Communication.Robustness
{
    [Obsolete("No longer supported.", true)]
    public class TwitterTest : TestBase
    {
        protected override bool Try()
        {
            // Nothing to do.
            return false;
        }
    }
}
