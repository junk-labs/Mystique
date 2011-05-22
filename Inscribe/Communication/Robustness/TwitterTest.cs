using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.Storage;
using Dulcet.Twitter.Rest;
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
