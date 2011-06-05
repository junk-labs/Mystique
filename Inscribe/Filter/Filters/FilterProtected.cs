using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe.Filter.Filters
{
    public class FilterProtected : FilterBase
    {
        public FilterProtected() { }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            return status.User.IsProtected;
        }

        public override string Identifier
        {
            get { return "protected"; }
        }

        public override IEnumerable<object> GetArgumentsForQueryify()
        {
            yield break;
        }

        public override string Description
        {
            get { return "プロテクトユーザーのツイート"; }
        }

        public override string FilterStateString
        {
            get { return "プロテクトユーザー"; }
        }
    }
}
