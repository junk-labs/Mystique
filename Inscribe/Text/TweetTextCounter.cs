using System.Linq;
using System.Text.RegularExpressions;

namespace Inscribe.Text
{
    public static class TweetTextCounter
    {
        public static int Count(string input)
        {
            // URL is MAX 19 Chars.
            int prevIndex = 0;
            int totalCount = 0;
            foreach (var m in RegularExpressions.UrlRegex.Matches(input).OfType<Match>())
            {
                totalCount += m.Index - prevIndex;
                prevIndex = m.Index + m.Groups[0].Value.Length;
                if (m.Groups[0].Value.Length < TwitterDefine.UrlMaxLength)
                    totalCount += m.Groups[0].Value.Length;
                else
                    totalCount += TwitterDefine.UrlMaxLength;
            }
            totalCount += input.Length - prevIndex;
            return totalCount;
        }
    }
}
