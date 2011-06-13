using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.Filter.Filters.Common;
using Inscribe.Storage;

namespace Inscribe.Filter.Filters.Arg1
{
    public class FilterRetweetFrom : ScreenNameFilterBase
    {
        private FilterRetweetFrom() { }

        public FilterRetweetFrom(string screen)
        {
            this.needle = needle;
        }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            var vm = TweetStorage.Get(status.Id);
            if (vm == null) return false;
            return vm.RetweetedUsers.Any(u => Match(u.TwitterUser.ScreenName, needle));
        }

        public override string Identifier
        {
            get { throw new NotImplementedException(); }
        }

        public override IEnumerable<object> GetArgumentsForQueryify()
        {
            throw new NotImplementedException();
        }

        public override string Description
        {
            get { throw new NotImplementedException(); }
        }

        public override string FilterStateString
        {
            get { throw new NotImplementedException(); }
        }
    }
}
