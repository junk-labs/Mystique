using System;
using System.Text.RegularExpressions;
using Inscribe.Filter.Core;
using Inscribe.Storage;

namespace Inscribe.Filter
{
    public static class MatchingUtil
    {
        public static bool MatchAccount(string haystack, string needle)
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
                    return haystack.Equals(needle, StringComparison.CurrentCultureIgnoreCase);
                }
                else if (startsWith)
                {
                    return haystack.StartsWith(needle, StringComparison.CurrentCultureIgnoreCase);
                }
                else if (endsWith)
                {
                    return haystack.EndsWith(needle, StringComparison.CurrentCultureIgnoreCase);
                }
                else
                {
                    return haystack.IndexOf(needle, StringComparison.CurrentCultureIgnoreCase) >= 0;
                }
            }
        }
        
        public static bool Match(string haystack, string needle, bool isCaseSensitive)
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
    }
}
