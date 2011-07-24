using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Packaging;

namespace Inscribe.Common
{
    static class Packaging
    {
        public static Uri ToPackUri(this string relativePath)
        {
            if (!relativePath.StartsWith("/"))
                relativePath = "/" + relativePath;
            return PackUriHelper.Create(new Uri("application:///"),
                new Uri(relativePath, UriKind.Relative));
        }

        public static Uri ToPackUri(this Uri relativePath)
        {
            return ToPackUri(relativePath.OriginalString);
        }
    }
}
