using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dulcet.Twitter;

namespace Inscribe.Filter.Filters.Attributes
{
    public class FilterRetweeted : FilterBase
    {
        public FilterRetweeted() { }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            var s = status as TwitterStatus;
            return s != null && s.RetweetedOriginal != null;
        }

        public override string Identifier
        {
            get { return "retweeted"; }
        }

        public override IEnumerable<object> GetArgumentsForQueryify()
        {
            yield break;
        }

        public override string Description
        {
            get { return "リツイートされたステータス"; }
        }

        public override string FilterStateString
        {
            get { return "RTされたステータス"; }
        }
    }
}
