using System.Linq;
using Dulcet.Twitter;
using Inscribe.Common;
using Inscribe.Storage;

namespace Inscribe.Filter.Filters.ScreenName
{
    public class FilterFollowFromStar : ScreenNameUserFilterBase
    {
        private FilterFollowFromStar() { }

        public FilterFollowFromStar(string needle)
        {
            this.needle = needle;
        }

        public override bool IsOnlyForTranscender { get { return true; } }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            return FilterUser(TwitterHelper.GetSuggestedUser(status));
        }

        public override string Identifier
        {
            get { return "follow_from*"; }
        }

        public override System.Collections.Generic.IEnumerable<string> Aliases
        {
            get
            {
                yield return "following*";
                yield return "followings*";
            }
        }

        public override string Description
        {
            get { return "指定アカウントのフォロー*"; }
        }

        public override string FilterStateString
        {
            get { return "アカウント @" + this.needle + " のフォロー*"; }
        }

        public override bool FilterUser(TwitterUser user)
        {
            return AccountStorage.Accounts.Where(i => Match(i.ScreenName, needle))
                .Any(i => i.IsFollowing(user.NumericId));
        }
    }
}
