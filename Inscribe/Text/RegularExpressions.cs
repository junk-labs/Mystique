using System.Text.RegularExpressions;

namespace Inscribe.Text
{
    public static class RegularExpressions
    {
        /// <summary>
        /// @userid用のregex
        /// </summary>
        public static Regex AtRegex = new Regex(@"(?<![A-Za-z0-9_])@([A-Za-z0-9_]+(?:/[A-Za-z0-9_]*)?)(?![A-Za-z0-9_@])",
            RegexOptions.Compiled | RegexOptions.Singleline);

        /// <summary>
        /// URL用のregex
        /// </summary>
        /*
        public static Regex UrlRegex = new Regex(@"(https?:\/\/(?:[()]*[\w;/?:@&=+$,-_.!~*'%#]+)+)",
            RegexOptions.Singleline | RegexOptions.Compiled);
        */
        // Regex from http://www.din.or.jp/~ohzaki/perl.htm#URI
        public static Regex UrlRegex = new Regex(@"((?:https?|shttp)://(?:(?:[-_.!~*'()a-zA-Z0-9;:&=+$,]|%[0-9A-Fa-f][0-9A-Fa-f])*@)?(?:(?:[a-zA-Z0-9](?:[-a-zA-Z0-9]*[a-zA-Z0-9])?\.)*[a-zA-Z](?:[-a-zA-Z0-9]*[a-zA-Z0-9])?\.?|[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+)(?::[0-9]*)?(?:/(?:[-_.!~*'()a-zA-Z0-9:@&=+$,]|%[0-9A-Fa-f][0-9A-Fa-f])*(?:;(?:[-_.!~*'()a-zA-Z0-9:@&=+$,]|%[0-9A-Fa-f][0-9A-Fa-f])*)*(?:/(?:[-_.!~*'()a-zA-Z0-9:@&=+$,]|%[0-9A-Fa-f][0-9A-Fa-f])*(?:;(?:[-_.!~*'()a-zA-Z0-9:@&=+$,]|%[0-9A-Fa-f][0-9A-Fa-f])*)*)*)?(?:\?(?:[-_.!~*'()a-zA-Z0-9;/?:@&=+$,]|%[0-9A-Fa-f][0-9A-Fa-f])*)?(?:#(?:[-_.!~*'()a-zA-Z0-9;/?:@&=+$,]|%[0-9A-Fa-f][0-9A-Fa-f])*)?)",
            RegexOptions.Compiled | RegexOptions.Singleline);

        /// <summary>
        /// ハッシュタグ用のregex
        /// </summary>
        public static Regex HashRegex = new Regex(@"(?<!\w)([#＃]\w+)",
            RegexOptions.Compiled | RegexOptions.Singleline);
    }
}
