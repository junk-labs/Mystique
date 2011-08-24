
namespace Inscribe.Filter.Filters.Text
{
    public class FilterUserName : TextFilterBase
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
            return this.Match(status.User.UserName, this.needle, this.isCaseSensitive);
        }

        public override string Identifier
        {
            get { return "name"; }
        }

        public override string Description
        {
            get { return "ユーザー名"; }
        }
    }
}
