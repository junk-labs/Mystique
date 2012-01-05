using System;
using System.Linq;
using System.Text.RegularExpressions;
using Dulcet.Twitter;
using Inscribe.Text;

namespace Inscribe.Filter.Filters.ScreenName
{
    public class FilterTo : ScreenNameFilterBase
    {
        private FilterTo() { }
        public FilterTo(string needle)
        {
            this.needle = needle;
        }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            var s = status as TwitterStatus;
            if (s != null)
            {
                if (!String.IsNullOrEmpty(s.InReplyToUserScreenName))
                    return Match(s.InReplyToUserScreenName, this.needle);
                else
                    return RegularExpressions.AtRegex.Matches(status.Text)
                        .Cast<Match>().Any(m => Match(m.Groups[1].Value, needle));
            }
            var dm = status as TwitterDirectMessage;
            if (dm != null)
            {
                return Match(dm.Recipient.ScreenName, this.needle);
            }
            return false;
        }

        public override string Identifier
        {
            get { return "to"; }
        }

        public override System.Collections.Generic.IEnumerable<string> Aliases
        {
            get { yield return "mention"; }
        }

        public override string Description
        {
            get { return "ツイートやDMの返信先ユーザー"; }
        }

        public override string FilterStateString
        {
            get { return "@" + this.needle + " への返信/DM"; }
        }
    }
}