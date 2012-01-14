
namespace Inscribe.Filter.Filters.Text
{
    public class FilterUserLocation : TextFilterBase, IUserFilter
    {
        private FilterUserLocation() { }

        public FilterUserLocation(string needle) : this(needle, false) { }

        public FilterUserLocation(string needle, bool isCaseSensitive)
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
            get { return "loc"; }
        }

        public override string Description
        {
            get { return "ユーザーのLocation"; }
        }

        public bool FilterUser(Dulcet.Twitter.TwitterUser user)
        {
            return this.Match(user.Location, this.needle, this.isCaseSensitive);
        }
    }
}
