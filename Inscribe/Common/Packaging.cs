using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe.Common
{
    static class Packaging
    {
        public static Uri ToPackUri(this string relativePath)
        {
            if (!relativePath.StartsWith("/"))
                relativePath = "/" + relativePath;
            return new Uri("pack://application:,,," + relativePath);
        }

        public static Uri ToPackUri(this Uri relativePath)
        {
            return ToPackUri(relativePath.OriginalString);
        }

    }
}
