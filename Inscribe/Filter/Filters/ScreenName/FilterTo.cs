using System;
using System.Linq;
using System.Text.RegularExpressions;
using Dulcet.Twitter;
using Inscribe.Filter.Core;
using Inscribe.Text;

namespace Inscribe.Filter.Filters.ScreenName
{
    public class FilterTo : ScreenNameFilterBase
    {
        private bool isStrict;
        [GuiVisible("返信先がこのユーザーの場合のみ合致")]
        public bool IsStrict
        {
            get { return this.isStrict; }
            set
            {
                this.isStrict = value;
                RaiseRequireReaccept();
            }
        }

        private FilterTo() { }
        public FilterTo(string needle) : this(needle, false) { }

        public FilterTo(string needle, bool isStrict)
        {
            this.needle = needle;
            this.isStrict = isStrict;
        }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            var s = status as TwitterStatus;
            if (s != null)
            {
                if (isStrict)
                {
                    return !String.IsNullOrEmpty(s.InReplyToUserScreenName) &&
                        Match(s.InReplyToUserScreenName, this.needle);
                }
                else
                {
                    return RegularExpressions.AtRegex.Matches(status.Text)
                        .Cast<Match>().Any(m => Match(m.Groups[1].Value, needle));
                }
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
            get { return "@" + this.needle + " への返信/DM" + (isStrict ? "(厳密)" : ""); }
        }

        public override System.Collections.Generic.IEnumerable<object> GetArgumentsForQueryify()
        {
            yield return needle;
            if (IsStrict)
                yield return true;
        }
    }
}