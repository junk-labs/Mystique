using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.Filter.Filters.Common;

namespace Inscribe.Filter.Filters.Arg1
{
    public class FilterUserName : TextFilterBase
    {
        private FilterUserName() { }

        public FilterUserName(string needle) : this(needle, false) { }

        public FilterUserName(string needle, bool isCaseSensitive)
        {
            this.needle = needle;
            this.isCaseSensitive = isCaseSensitive;
        }


        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            return this.Match(status.User.UserName, this.needle, this.isCaseSensitive);
        }

        public override string Identifier
        {
            get { return "uname"; }
        }

        public override string Description
        {
            get { return "ユーザー名"; }
        }
    }
}
