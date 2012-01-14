using System.Collections.Generic;
using System.Linq;
using Dulcet.Twitter;
using Inscribe.Filter.Core;
using Inscribe.Storage;

namespace Inscribe.Filter.Filters.Numeric
{
    public class FilterUserId : UserFilterBase
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

        public FilterUserId(long pivot) : this(LongRange.FromPivotValue(pivot)) { }

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
                    var u = UserStorage.Get(this._range.From.Value);
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

        public override bool FilterUser(TwitterUser user)
        {
            return this.Range.Check(user.NumericId);
        }
    }
}
