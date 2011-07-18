using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.Text;
using System.Text.RegularExpressions;
using Inscribe.Storage;
using Dulcet.Twitter;
using Inscribe.Filter.Core;

namespace Inscribe.Filter.Filters.Particular
{
    public class FilterConversation : FilterBase
    {
        private string user1;

        [GuiVisible("ユーザー1")]
        public string User1
        {
            get { return user1; }
            set { user1 = value; }
        }

        private string user2;

        [GuiVisible("ユーザー2")]
        public string User2
        {
            get { return user2; }
            set { user2 = value; }
        }

        private FilterConversation() { }

        public FilterConversation(string user1, string user2)
        {
            this.user1 = user1;
            this.user2 = user2;
        }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            // conversation control
            var vm = TweetStorage.Get(status.Id);
            if (status is TwitterDirectMessage)
            {
                var dm = (TwitterDirectMessage)status;
                return
                    (dm.Sender.ScreenName.Equals(user1, StringComparison.CurrentCultureIgnoreCase) &&
                     dm.Recipient.ScreenName.Equals(user2, StringComparison.CurrentCultureIgnoreCase)) ||
                    (dm.Sender.ScreenName.Equals(user2, StringComparison.CurrentCultureIgnoreCase) &&
                     dm.Recipient.ScreenName.Equals(user1, StringComparison.CurrentCultureIgnoreCase));
            }
            else
            {
                if (RegularExpressions.AtRegex.Matches(status.Text).Cast<Match>()
                    .Any(m => m.Value.Equals(user1, StringComparison.CurrentCultureIgnoreCase) ||
                        m.Value.Equals(user2, StringComparison.CurrentCultureIgnoreCase)))
                    return true;
                if (vm.InReplyFroms.Select(id => TweetStorage.Get(id))
                    .Any(irvm => irvm.Status.User.ScreenName.Equals(user1, StringComparison.CurrentCultureIgnoreCase) ||
                        irvm.Status.User.ScreenName.Equals(user2, StringComparison.CurrentCultureIgnoreCase)))
                    return true;
                else
                    return false;
            }
        }

        public override string Identifier
        {
            get { return "conv"; }
        }

        public override IEnumerable<object> GetArgumentsForQueryify()
        {
            yield return this.user1;
            yield return this.user2;
        }

        public override string Description
        {
            get { return "2ユーザー間の会話"; }
        }

        public override string FilterStateString
        {
            get { return "@" + user1 + " と @" + user2 + " の会話"; }
        }
    }
}
