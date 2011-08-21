using System.Text.RegularExpressions;

namespace Inscribe.Text
{
    public static class RegularExpressions
    {
        /// <summary>
        /// @userid用のregex
        /// </summary>
        public static Regex AtRegex = new Regex(@"@([A-Za-z0-9_]+(?:/[A-Za-z0-9_]*)?)",
            RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// URL用のregex
        /// </summary>
        /*
        public static Regex UrlRegex = new Regex(@"(https?:\/\/(?:[()]*[\w;/?:@&=+$,-_.!~*'%#]+)+)",
            RegexOptions.Singleline | RegexOptions.Compiled);
        */
        // Regex from http://daringfireball.net/2010/07/improved_regex_for_matching_urls
        public static Regex UrlRegex = new Regex(@"(?i)\b((?:https?://|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))",
            RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// ハッシュタグ用のregex
        /// </summary>
        public static Regex HashRegex = new Regex(@"(?<!\w)([#＃]\w+)",
            RegexOptions.Compiled | RegexOptions.Singleline);
    }
}
