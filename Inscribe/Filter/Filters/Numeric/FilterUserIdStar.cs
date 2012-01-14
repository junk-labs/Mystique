using Inscribe.Common;
using Inscribe.Filter.Core;
using Inscribe.Storage;

namespace Inscribe.Filter.Filters.Numeric
{
    public class FilterUserIdStar : FilterUserId
    {
        private FilterUserIdStar() : base(0) { }

        public FilterUserIdStar(LongRange range) : base(range) { }

        public FilterUserIdStar(long pivot) : base(LongRange.FromPivotValue(pivot)) { }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            return FilterUser(TwitterHelper.GetSuggestedUser(status));
        }

        public override string Identifier
        {
            get { return "uid*"; }
        }

        public override bool IsOnlyForTranscender
        {
            get { return true; }
        }

        public override string Description
        {
            get { return "ユーザーの数値ID*"; }
        }

        public override string FilterStateString
        {
            get
            {
                if (this.Range != null && this.Range.From != null && this.Range.RangeType == RangeType.Pivot)
                {
                    var u = UserStorage.Get(this.Range.From.Value);
                    if (u == null)
                    {
                        return "ユーザー数値ID*:" + this.Range.ToString() + "(逆引き: Krile内に見つかりません)";
                    }
                    else
                    {
                        return "ユーザー数値ID*:" + this.Range.ToString() + "(逆引き: @" + u.TwitterUser.ScreenName + ")";
                    }
                }
                else
                {
                    return "ユーザー数値ID*:" + this.Range.ToString();
                }
            }
        }
    }
}
