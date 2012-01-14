
namespace Inscribe.Filter.Filters.Text
{
    public class FilterUserName : TextFilterBase, IUserFilter
    {
        private FilterUserName() { }

        public FilterUserName(string needle) : this(needle, false) { }

        public FilterUserName(string needle, bool isCaseSensitive)
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
            get { return "name"; }
        }

        public override string Description
        {
            get { return "ユーザー名"; }
        }

        public bool FilterUser(Dulcet.Twitter.TwitterUser user)
        {
            return this.Match(user.UserName, this.needle, this.isCaseSensitive);
        }
    }
}
