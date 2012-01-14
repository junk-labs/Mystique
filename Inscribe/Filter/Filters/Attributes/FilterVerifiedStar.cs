using Inscribe.Common;

namespace Inscribe.Filter.Filters.Attributes
{
    public class FilterVerifiedStar : FilterVerified
    {
        public FilterVerifiedStar() { }

        public override string Identifier
        {
            get { return "verified*"; }
        }

        public override bool IsOnlyForTranscender
        {
            get { return true; }
        }

        public override string Description
        {
            get { return "公式認証ユーザーのツイート*"; }
        }

        public override string FilterStateString
        {
            get { return "公式認証ユーザー*"; }
        }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            return FilterUser(TwitterHelper.GetSuggestedUser(status));
        }
    }
}
