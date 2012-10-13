using System.Linq;
using System.Text.RegularExpressions;

namespace Inscribe.Text
{
    public static class TweetTextCounter
    {
        public static int Count(string input)
        {
            // URL is MAX 20 Chars (if URL has HTTPS scheme, URL is MAX 21 Chars)
            int prevIndex = 0;
            int totalCount = 0;
            foreach (var m in RegularExpressions.UrlRegex.Matches(input).OfType<Match>())
            {
                totalCount += m.Index - prevIndex;
                prevIndex = m.Index + m.Groups[0].Value.Length;

                bool isHasHttpsScheme = m.Groups[0].Value.Contains("https");


                if (m.Groups[0].Value.Length < TwitterDefine.UrlMaxLength + ((isHasHttpsScheme) ? 1 : 0))
                    totalCount += m.Groups[0].Value.Length;
                else
                    totalCount += TwitterDefine.UrlMaxLength + ((isHasHttpsScheme) ? 1 : 0);
            }
            totalCount += input.Length - prevIndex;
            return totalCount;
        }
    }
}
