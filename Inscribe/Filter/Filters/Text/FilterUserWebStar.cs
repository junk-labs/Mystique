using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.Common;

namespace Inscribe.Filter.Filters.Text
{
    public class FilterUserWebStar : FilterUserWeb
    {
        private FilterUserWebStar() : base(null) { }

        public FilterUserWebStar(string needle) : base(needle, false) { }

        public FilterUserWebStar(string needle, bool isCaseSensitive) : base(needle, isCaseSensitive) { }

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