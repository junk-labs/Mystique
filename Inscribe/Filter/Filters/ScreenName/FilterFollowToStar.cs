using Inscribe.Common;
using System.Linq;

namespace Inscribe.Filter.Filters.ScreenName
{
    public class FilterFollowToStar : FilterFollowTo
    {
        private FilterFollowToStar() : base(null) { }

        public FilterFollowToStar(string needle) : base(needle) { }

        public override bool IsOnlyForTranscender { get { return true; } }

        public override string Identifier
        {
            get { return base.Identifier + "*"; }
        }

        public override System.Collections.Generic.IEnumerable<string> Aliases
        {
            get { return base.Aliases.Select(s => s + "*"); }
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
