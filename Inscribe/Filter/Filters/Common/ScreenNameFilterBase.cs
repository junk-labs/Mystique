using System;
using Inscribe.Filter.Core;
using Inscribe.Storage;

namespace Inscribe.Filter.Filters.Common
{
    public abstract class ScreenNameFilterBase : TextFilterBase
    {
        protected bool Match(string haystack, string needle)
        {
            if (needle == "*")
            {
                return AccountStorage.Contains(haystack);
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
    }
}
