using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mystique.Core
{
    internal static class Initializer
    {
        internal static void Init()
        {
            Dulcet.Network.Http.Expect100Continue = false;
            Dulcet.Network.Http.MaxConnectionLimit = Int32.MaxValue;
        }
    }
}
