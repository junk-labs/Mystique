using System.Collections.Generic;
using System.Linq;
using Inscribe.Filter.Core;
using Inscribe.Storage;

namespace Inscribe.Filter.Filters.Numeric
{
    public class FilterRetweetCount : FilterBase
    {
        private LongRange _range;

        [GuiVisible("Retweet数範囲")]
        public LongRange Range
        {
            get { return _range ?? LongRange.FromPivotValue(0); }
            set { _range = value; }
        }

        private FilterRetweetCount() { }

        public FilterRetweetCount(LongRange range)
        {
            this.Range = range;
        }

        public FilterRetweetCount(long pivot)
        {
            this.Range = LongRange.FromPivotValue(pivot);
        }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            return this.Range.Check(TweetStorage.Get(status.Id).RetweetedUsers.Count());
        }

        public override string Identifier
        {
            get { return "rt_count"; }
        }

        public override IEnumerable<object> GetArgumentsForQueryify()
        {
            yield return this.Range;
        }

        public override string Description
        {
            get { return "被リツイート数"; }
        }

        public override string FilterStateString
        {
            get { return "被RT数が " + this.Range.ToString() + " であるもの"; }
        }
    }
}
