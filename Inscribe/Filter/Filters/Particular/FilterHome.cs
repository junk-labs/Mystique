using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dulcet.Twitter;
using Inscribe.Authentication;
using Inscribe.Filter.Core;
using Inscribe.Storage;
using Inscribe.Text;

namespace Inscribe.Filter.Filters.Particular
{
    public class FilterHome : UserFilterBase
    {
        private string needle;
        [GuiVisible("検索@ID", "自分のアカウントのみ有効")]
        public string Needle
        {
            get { return this.needle ?? String.Empty; }
            set
            {
                if (this.needle == value) return;
                this.needle = value;
                RaiseRequireReaccept();
            }
        }
        private FilterHome() { }
        public FilterHome(string account)
        {
            this.needle = account;
        }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            var s = status as TwitterStatus;
            if (s == null)
            {
                var d = status as TwitterDirectMessage;
                if (d == null)
                    return false;
                else
                    return MatchingUtil.MatchAccount(d.Recipient.ScreenName, needle) || MatchingUtil.MatchAccount(d.Sender.ScreenName, needle);
            }
            else
            {
                return AccountStorage.Accounts
                    .Where(i => MatchingUtil.MatchAccount(i.ScreenName, needle))
                    .Any(i => IsMemberOfTimeline(s, i));
            }
        }

        private bool IsMemberOfTimeline(TwitterStatus status, AccountInfo info)
        {
            // 自分のツイートかどうか
            if (status.User.NumericId == info.NumericId)
                return true;
            // 自分への返信かどうか
            if (RegularExpressions.AtRegex.Matches(status.Text)
                .OfType<Match>()
                .Any(s => s.Value.Equals(info.ScreenName, StringComparison.CurrentCultureIgnoreCase)))
                return true;
            // 自分のフォローしている相手か
            return info.IsFollowing(status.User.NumericId) &&
                // 先頭が@でないか、またはフォローしている相手
                (!status.Text.StartsWith("@") || info.IsFollowing(status.InReplyToUserId));
        }

        public override bool FilterUser(Dulcet.Twitter.TwitterUser user)
        {
            return AccountStorage.Accounts.Where(i => MatchingUtil.MatchAccount(i.ScreenName, needle))
                .Any(i => i.IsFollowing(user.NumericId));
        }

        public override string Identifier
        {
            get { return "home"; }
        }

        public override IEnumerable<object> GetArgumentsForQueryify()
        {
            yield return Needle;
        }

        public override string Description
        {
            get { return "ホームタイムラインに表示されるべきツイート"; }
        }

        public override string FilterStateString
        {
            get { return "@" + needle + " のタイムラインに表示されるツイート"; }
        }
    }
}