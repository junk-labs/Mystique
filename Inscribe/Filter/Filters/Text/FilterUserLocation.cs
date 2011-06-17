using System;

namespace Inscribe.Filter.Filters.Text
{
    public class FilterUserLocation : TextFilterBase
    {
        private FilterUserLocation() { }

        public FilterUserLocation(string needle) : this(needle, false) { }

        public FilterUserLocation(string needle, bool isCaseSensitive)
        {
            this.needle = needle;
            this.isCaseSensitive = isCaseSensitive;
        }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            return this.Match(status.User.Location, this.needle, this.isCaseSensitive);
        }

        public override string Identifier
        {
            get { throw new NotImplementedException(); }
        }

        public override string Description
        {
            get { throw new NotImplementedException(); }
        }
    }
}
