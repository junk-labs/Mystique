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
        public static Regex URLRegex = new Regex(@"(https?:\/\/(?:[()]*[\w;/?:@&=+$,-_.!~*'%#]+)+)",
            RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// ハッシュタグ用のregex
        /// </summary>
        public static Regex HashRegex = new Regex(@"(?<!\w)([#＃]\w+)",
            RegexOptions.Compiled | RegexOptions.Singleline);
    }
}
