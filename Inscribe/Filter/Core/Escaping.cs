using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe.Filter.Core
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
        /// 文字をクオートします。
        /// </summary>
        /// <param name="argstr">エスケープ済みの文字列</param>
        public static string Quote(this string escaped)
        {
            return "\"" + escaped + "\"";
        }
    }
}
