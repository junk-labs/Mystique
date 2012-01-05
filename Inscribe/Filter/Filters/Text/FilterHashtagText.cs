using System.Linq;
using System.Text.RegularExpressions;
using Inscribe.Text;

namespace Inscribe.Filter.Filters.Text
{
    public class FilterHashtagText : TextFilterBase
    {
        private FilterHashtagText() { }

        public FilterHashtagText(string needle) : this(needle, false) { }

        public FilterHashtagText(string needle, bool isCaseSensitive)
        {
            this.needle = needle;
            this.isCaseSensitive = isCaseSensitive;
        }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            return RegularExpressions.HashRegex.Matches(status.Text).OfType<Match>()
                .Any(m => this.Match(m.Value, this.needle, this.isCaseSensitive));
        }

        public override string Identifier
        {
            get { return "tag"; }
        }

        public override System.Collections.Generic.IEnumerable<string> Aliases
        {
            get { yield return "hashtag"; }
        }

        public override string Description
        {
            get { return "ハッシュタグ"; }
        }
    }
}
