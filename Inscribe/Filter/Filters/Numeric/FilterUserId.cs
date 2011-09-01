using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.Filter.Core;
using Inscribe.Storage;

namespace Inscribe.Filter.Filters.Numeric
{
    public class FilterUserId : FilterBase
    {
        private LongRange _range;

        [GuiVisible("ユーザー数値ID範囲")]
        public LongRange Range
        {
            get { return _range ?? LongRange.FromPivotValue(0); }
            set { _range = value; }
        }

        private FilterUserId() { }

        public FilterUserId(LongRange range)
        {
            this.Range = range;
        }

        public FilterUserId(long pivot)
        {
            this.Range = LongRange.FromPivotValue(pivot);
        }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            return this.Range.Check(status.User.NumericId);
        }

        public override string Identifier
        {
            get { return "uid"; }
        }

        public override IEnumerable<object> GetArgumentsForQueryify()
        {
            yield return this.Range;
        }

        public override string Description
        {
            get { return "ユーザーの数値ID"; }
        }

        public override string FilterStateString
        {
            get
            {
                if (this._range != null && this._range.From != null && this.Range.RangeType == RangeType.Pivot)
                {
                    var u = UserStorage.GetAll().Where(uvm => uvm.TwitterUser.NumericId == this._range.From).FirstOrDefault();
                    if (u == null)
                    {
                        return "ユーザー数値ID:" + this.Range.ToString() + "(逆引き: Krile内に見つかりません)";
                    }
                    else
                    {
                        return "ユーザー数値ID:" + this.Range.ToString() + "(逆引き: @" + u.TwitterUser.ScreenName + ")";
                    }
                }
                else
                {
                    return "ユーザー数値ID:" + this.Range.ToString();
                }
            }
        }
    }
}
