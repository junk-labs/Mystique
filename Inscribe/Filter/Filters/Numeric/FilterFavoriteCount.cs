using System.Collections.Generic;
using System.Linq;
using Inscribe.Filter.Core;
using Inscribe.Storage;

namespace Inscribe.Filter.Filters.Numeric
{
    public class FilterFavoriteCount : FilterBase
    {
        private LongRange range;

        [GuiVisible("Fav数範囲")]
        public LongRange Range
        {
            get { return range ?? LongRange.FromPivotValue(0); }
            set { range = value; }
        }

        private FilterFavoriteCount() { }

        public FilterFavoriteCount(LongRange range)
        {
            this.range = range;
        }

        public FilterFavoriteCount(long pivot)
        {
            this.range = LongRange.FromPivotValue(pivot);
        }
        
        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            return this.Range.Check(TweetStorage.Get(status.Id).FavoredUsers.Count());
        }

        public override string Identifier
        {
            get { return "fav_count"; }
        }

        public override IEnumerable<object> GetArgumentsForQueryify()
        {
            yield return this.range;
        }

        public override string Description
        {
            get { return "被お気に入り数"; }
        }

        public override string FilterStateString
        {
            get { return "被Fav数が " + this.Range.ToString() + " であるもの"; }
        }
    }
}
