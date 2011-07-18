using System.Collections.Generic;
using System.Linq;
using Inscribe.Filter.Core;
using Inscribe.Storage;

namespace Inscribe.Filter.Filters.Numeric
{
    public class FilterRetweetCount : FilterBase
    {
        private LongRange range;

        [GuiVisible("Retweet数範囲")]
        public LongRange Range
        {
            get { return range ?? LongRange.FromPivotValue(0); }
            set { range = value; }
        }

        private FilterRetweetCount() { }

        public FilterRetweetCount(LongRange range)
        {
            this.range = range;
        }

        public FilterRetweetCount(long pivot)
        {
            this.range = LongRange.FromPivotValue(pivot);
        }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            return this.range.Check(TweetStorage.Get(status.Id).RetweetedUsers.Count());
        }

        public override string Identifier
        {
            get { return "rt_count"; }
        }

        public override IEnumerable<object> GetArgumentsForQueryify()
        {
            yield return this.range;
        }

        public override string Description
        {
            get { return "被リツイート数"; }
        }

        public override string FilterStateString
        {
            get { return "被RT数が " + this.range.ToString() + " であるもの"; }
        }
    }
}
