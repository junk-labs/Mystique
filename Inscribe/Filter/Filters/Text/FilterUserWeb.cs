
namespace Inscribe.Filter.Filters.Text
{
    public class FilterUserWeb : TextFilterBase, IUserFilter
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
            return FilterUser(status.User);
        }

        public override string Identifier
        {
            get { return "web"; }
        }

        public override string Description
        {
            get { return "ユーザーWeb"; }
        }

        public bool FilterUser(Dulcet.Twitter.TwitterUser user)
        {
            return this.Match(user.Web, this.needle, this.isCaseSensitive);
        }
    }
}
