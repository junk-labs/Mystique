
namespace Inscribe.Filter.Filters.ScreenName
{
    public class FilterUser : ScreenNameFilterBase
    {
        private FilterUser() { }

        public FilterUser(string needle)
        {
            this.needle = needle;
        }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            return Match(status.User.ScreenName, needle);
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
    }
}
