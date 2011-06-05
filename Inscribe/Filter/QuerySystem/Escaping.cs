using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe.Filter.QuerySystem
{
    public static class Escaping
    {
        /// <summary>
        /// クエリ化するためにエスケープを行います。
        /// </summary>
        public static string EscapeForQuery(this string unescaped)
        {
            // \ => \\
            // " => \"
            return unescaped.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        /// <summary>
        /// クエリからフィルタオブジェクトへの引数にするためにアンエスケープを行います。
        /// </summary>
        public static string UnescapeFromQuery(this string escaped)
        {
            // \" => "
            // \\ => \
            return escaped.Replace("\\\"", "\"").Replace("\\\\", "\\");
        }

        /// <summary>
        /// 文字をクオートしなければならない場合、クオートを行います。
        /// </summary>
        /// <param name="argstr">エスケープ済みの文字列</param>
        public static string Quote(this string escaped)
        {
            int i; bool b;
            if (escaped.Where(c => requireQuotes.Contains(c)).Count() > 0 || int.TryParse(escaped, out i) || bool.TryParse(escaped, out b))
            {
                return "\"" + escaped + "\"";
            }
            else
            {
                return escaped;
            }
        }

        private static char[] requireQuotes = new[] { ' ', '\t', ',', '(', ')', '|', '&', '"', '\\' };
    }
}
