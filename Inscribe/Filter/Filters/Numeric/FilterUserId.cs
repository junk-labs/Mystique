using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.Filter.Core;

namespace Inscribe.Filter.Filters.Numeric
{
    public class FilterUserId : FilterBase
    {
        private LongRange range;

        [GuiVisible("ユーザー数値ID範囲")]
        public LongRange Range
        {
            get { return range ?? LongRange.FromPivotValue(0); }
            set { range = value; }
        }

        private FilterUserId() { }

        public FilterUserId(LongRange range)
        {
            this.range = range;
        }

        public FilterUserId(long pivot)
        {
            this.range = LongRange.FromPivotValue(pivot);
        }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            return this.range.Check(status.User.NumericId);
        }

        public override string Identifier
        {
            get { return "uid"; }
        }

        public override IEnumerable<object> GetArgumentsForQueryify()
        {
            yield return range;
        }

        public override string Description
        {
            get { return "ユーザーの数値ID"; }
        }

        public override string FilterStateString
        {
            get { return "ユーザー数値ID:" + this.range.ToString(); }
        }
    }
}
