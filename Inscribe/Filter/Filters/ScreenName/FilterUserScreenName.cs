
namespace Inscribe.Filter.Filters.ScreenName
{
    public class FilterUserScreenName : ScreenNameUserFilterBase
    {
        private FilterUserScreenName() { }
        public FilterUserScreenName(string needle)
        {
            this.needle = needle;
        }

        public override string Identifier
        {
            get { return "user"; }
        }

        public override System.Collections.Generic.IEnumerable<string> Aliases
        {
            get { yield return "u"; }
        }

        public override string Description
        {
            get { return "ユーザー@ID"; }
        }

        public override bool FilterUser(Dulcet.Twitter.TwitterUser user)
        {
            return Match(user.ScreenName, needle);
        }
    }
}
