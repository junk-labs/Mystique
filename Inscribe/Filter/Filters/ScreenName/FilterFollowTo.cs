using System.Linq;
using Inscribe.Storage;

namespace Inscribe.Filter.Filters.ScreenName
{
    public class FilterFollowTo : ScreenNameUserFilterBase
    {
        private FilterFollowTo() { }
        public FilterFollowTo(string needle)
        {
            this.needle = needle;
        }

        public override string Identifier
        {
            get { return "follow_to"; }
        }

        public override System.Collections.Generic.IEnumerable<string> Aliases
        {
            get
            {
                yield return "follower";
                yield return "followers";
            }
        }

        public override string Description
        {
            get { return "指定アカウントのフォロワー"; }
        }

        public override string FilterStateString
        {
            get { return "アカウント @" + this.needle + " のフォロワー"; }
        }

        public override bool FilterUser(Dulcet.Twitter.TwitterUser user)
        {
            return AccountStorage.Accounts.Where(i => Match(i.ScreenName, needle))
                .Any(i => i.IsFollowedBy(user.NumericId));
        }
    }
}
