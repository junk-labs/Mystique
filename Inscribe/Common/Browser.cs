using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe.Common
{
    public static class Browser
    {
        public static void Start(string navigate)
        {
            try
            {
                System.Diagnostics.Process.Start(navigate);
            }
            catch { } // 握りつぶす
        }
    }
}
