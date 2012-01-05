using System.Linq;
using System.Text.RegularExpressions;
using Inscribe.Text;

namespace Inscribe.Filter.Filters.Text
{
    public class FilterUrlText : TextFilterBase
    {
        private FilterUrlText() { }

        public FilterUrlText(string needle) : this(needle, false) { }

        public FilterUrlText(string needle, bool isCaseSensitive)
        {
            this.needle = needle;
            this.isCaseSensitive = isCaseSensitive;
        }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            return RegularExpressions.UrlRegex.Matches(status.Text).OfType<Match>()
                .Any(m => this.Match(m.Value, this.needle, this.isCaseSensitive));
        }

        public override string Identifier
        {
            get { return "url"; }
        }

        public override string Description
        {
            get { return "本文中のURL"; }
        }
    }
}
