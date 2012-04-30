using Inscribe.Common;

namespace Inscribe.Filter.Filters.ScreenName
{
    public class FilterBlockFromStar : FilterBlockFrom
    {
        private FilterBlockFromStar() : base(null) { }

        public FilterBlockFromStar(string needle) : base(needle) { }

        public override bool IsOnlyForTranscender { get { return true; } }

        public override string Identifier
        {
            get { return base.Identifier + "*"; }
        }

        public override string Description
        {
            get { return base.Description + "*"; }
        }

        public override string FilterStateString
        {
            get { return base.FilterStateString + "*"; }
        }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            return FilterUser(TwitterHelper.GetSuggestedUser(status));
        }
    }
}
