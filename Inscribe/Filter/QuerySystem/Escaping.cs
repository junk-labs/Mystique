using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe.Filter.QuerySystem
{
    public static class Escaping
    {
        public static string EscapeForQuery(this string unescaped)
        {
            // \ => \\
            // " => \"
            return unescaped.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        public static string UnescapeFromQuery(this string escaped)
        {
            // \" => "
            // \\ => \
            return escaped.Replace("\\\"", "\"").Replace("\\\\", "\\");
        }
    }
}
