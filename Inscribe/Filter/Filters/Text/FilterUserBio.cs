
namespace Inscribe.Filter.Filters.Text
{
    public class FilterUserBio : TextFilterBase, IUserFilter
    {
        private FilterUserBio() { }

        public FilterUserBio(string needle) : this(needle, false) { }

        public FilterUserBio(string needle, bool isCaseSensitive)
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
            get { return "bio"; }
        }

        public override System.Collections.Generic.IEnumerable<string> Aliases
        {
            get
            {
                yield return "desc";
                yield return "description";
            }
        }

        public override string Description
        {
            get { return "ユーザーのBio(Description)"; }
        }

        public bool FilterUser(Dulcet.Twitter.TwitterUser user)
        {
            return this.Match(user.Bio, this.needle, this.isCaseSensitive);
        }
    }
}
