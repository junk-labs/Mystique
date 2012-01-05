using System.Linq;
using Dulcet.Twitter;
using Inscribe.Filter.Core;
using Inscribe.Storage;

namespace Inscribe.Filter.Filters.ScreenName
{
    public class FilterFollowFrom : ScreenNameFilterBase
    {
        private bool acceptBlocking = false;
        [GuiVisible("フォロー中ユーザーによるブロック中ユーザーのRTを受け入れる")]
        public bool AcceptBlocking
        {
            get { return this.acceptBlocking; }
            set
            {
                if (this.acceptBlocking == value) return;
                this.acceptBlocking = value;
                RaiseRequireReaccept();
            }
        }

        private FilterFollowFrom() { }
        public FilterFollowFrom(string needle) : this(needle, false) { }

        public FilterFollowFrom(string needle, bool acceptBlocking)
        {
            this.needle = needle;
            this.acceptBlocking = acceptBlocking;
        }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            var ts = status as TwitterStatus;
            if (ts != null && ts.RetweetedOriginal != null && !acceptBlocking)
            {
                return AccountStorage.Accounts.Where(i => Match(i.ScreenName, needle))
                    .Any(i => i.IsFollowing(status.User.NumericId)) &&
                       AccountStorage.Accounts.Where(i => Match(i.ScreenName, needle))
                    .All(i => !i.IsBlocking(ts.RetweetedOriginal.User.NumericId));
            }
            else
            {
                return AccountStorage.Accounts.Where(i => Match(i.ScreenName, needle))
                    .Any(i => i.IsFollowing(status.User.NumericId));
            }
        }

        public override string Identifier
        {
            get { return "follow_from"; }
        }

        public override System.Collections.Generic.IEnumerable<string> Aliases
        {
            get
            {
                yield return "following";
                yield return "followings";
            }
        }

        public override string Description
        {
            get { return "指定アカウントのフォロー"; }
        }

        public override string FilterStateString
        {
            get { return "アカウント @" + this.needle + " のフォロー"; }
        }
    }
}
