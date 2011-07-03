using System.Linq;
using Inscribe.Storage;

namespace Inscribe.Filter.Filters.ScreenName
{
    public class FilterFollowTo : ScreenNameFilterBase
    {
        private FilterFollowTo() { }
        public FilterFollowTo(string needle)
        {
            this.needle = needle;
        }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            return AccountStorage.Accounts.Where(i => Match(i.ScreenName, needle))
                .Any(i => i.IsFollowedBy(status.User.NumericId));
        }

        public override string Identifier
        {
            get { return "follow_to"; }
        }

        public override string Description
        {
            get { return "指定アカウントのフォロワー"; }
        }

        public override string FilterStateString
        {
            get { return "アカウント @" + this.needle + " のフォロワー"; }
        }
    }
}
