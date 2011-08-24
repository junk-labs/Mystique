
namespace Inscribe.Filter.Filters.Text
{
    public class FilterUserWeb : TextFilterBase
    {
        private FilterUserWeb() { }

        public FilterUserWeb(string needle) : this(needle, false) { }

        public FilterUserWeb(string needle, bool isCaseSensitive)
        {
            this.needle = needle;
            this.isCaseSensitive = isCaseSensitive;
        }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            return this.Match(status.User.Web, this.needle, this.isCaseSensitive);
        }

        public override string Identifier
        {
            get { return "web"; }
        }

        public override string Description
        {
            get { return "ユーザーWeb"; }
        }
    }
}
