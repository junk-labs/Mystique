using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Inscribe.Filter.Core;

namespace Inscribe.Filter.Filters.Text
{
    public abstract class TextFilterBase : FilterBase
    {
        [GuiVisible("検索テキスト")]
        public string Needle
        {
            get { return this.needle; }
            set
            {
                this.needle = value;
                RaiseRequireReaccept();
            }
        }
        protected string needle = String.Empty;

        [GuiVisible("大小文字を区別")]
        public bool IsCaseSensitive
        {
            get { return this.isCaseSensitive; }
            set
            {
                this.isCaseSensitive = value;
                RaiseRequireReaccept();
            }
        }
        protected bool isCaseSensitive = false;

        protected virtual bool Match(string haystack, string needle, bool isCaseSensitive)
        {
            if (needle.StartsWith("/"))
            {
                // regular expressions
                if (isCaseSensitive)
                    return Regex.IsMatch(haystack, needle.Substring(1));
                else
                    return Regex.IsMatch(haystack, needle.Substring(1), RegexOptions.IgnoreCase);
            }
            else
            {
                bool startsWith = false;
                bool endsWith = false;

                if (needle.StartsWith("\\/"))
                {
                    // 先頭のスラッシュエスケープを削除
                    needle = needle.Substring(1);
                }

                if (needle.StartsWith("^"))
                {
                    startsWith = true;
                    needle = needle.Substring(1);
                }
                else if (needle.StartsWith("\\^"))
                {
                    needle = needle.Substring(1);
                }

                if (needle.EndsWith("$"))
                {
                    if (needle.EndsWith("\\$"))
                    {
                        needle = needle.Substring(0, needle.Length - 2) + "$";
                    }
                    else
                    {
                        endsWith = true;
                        needle = needle.Substring(0, needle.Length - 1);
                    }
                }
                var unescaped = needle.UnescapeFromQuery();
                if (startsWith && endsWith)
                {
                    // complete
                    if (isCaseSensitive)
                        return haystack.Equals(needle, StringComparison.CurrentCulture);
                    else
                        return haystack.Equals(needle, StringComparison.CurrentCultureIgnoreCase);
                }
                else if (startsWith)
                {
                    if (isCaseSensitive)
                        return haystack.StartsWith(needle, StringComparison.CurrentCulture);
                    else
                        return haystack.StartsWith(needle, StringComparison.CurrentCultureIgnoreCase);
                }
                else if (endsWith)
                {
                    if (isCaseSensitive)
                        return haystack.EndsWith(needle, StringComparison.CurrentCulture);
                    else
                        return haystack.EndsWith(needle, StringComparison.CurrentCultureIgnoreCase);
                }
                else
                {
                    if (isCaseSensitive)
                        return haystack.IndexOf(needle, StringComparison.CurrentCulture) >= 0;
                    else
                        return haystack.IndexOf(needle, StringComparison.CurrentCultureIgnoreCase) >= 0;
                }
            }
        }

        public override IEnumerable<object> GetArgumentsForQueryify()
        {
            yield return needle;
            if (isCaseSensitive)
                yield return isCaseSensitive;
        }

        public override string FilterStateString
        {
            get { return this.Description + "から\"" + this.needle + "\"を検索" + (this.isCaseSensitive ? "(大小区別)" : ""); }
        }
    }
}
