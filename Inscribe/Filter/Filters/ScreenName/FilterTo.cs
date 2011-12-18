using System;
using Dulcet.Twitter;

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
                if (String.IsNullOrEmpty(s.InReplyToUserScreenName))
                    return false;
                else
                    return Match(s.InReplyToUserScreenName, this.needle);
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

        public override string Description
        {
            get { return "ツイートやDMの返信先アカウント"; }
        }

        public override string FilterStateString
        {
            get { return "@" + this.needle + " への返信/DM"; }
        }
    }
}