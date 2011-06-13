using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.Filter.Filters.Common;

namespace Inscribe.Filter.Filters.Arg1
{
    public class FilterUserBio : TextFilterBase
    {
        private FilterUserBio() { }

        public FilterUserBio(string needle) : this(needle, false) { }

        public FilterUserBio(string needle, bool isCaseSensitive)
        {
            this.needle = needle;
            this.isCaseSensitive = isCaseSensitive;
        }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            return this.Match(status.User.Bio, this.needle, this.isCaseSensitive);
        }

        public override string Identifier
        {
            get { return "u_bio"; }
        }

        public override string Description
        {
            get { return "ユーザー説明"; }
        }
    }
}
