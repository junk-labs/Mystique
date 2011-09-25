using System.Collections.Generic;
using System.Linq;
using Inscribe.Filter.Core;
using Inscribe.Storage;

namespace Inscribe.Filter.Filters.Numeric
{
    public class FilterFavoriteCount : FilterBase
    {
        private WeakEventBinder<EventDescriptionEventArgs> web;

        private LongRange _range;

        [GuiVisible("Fav数範囲")]
        public LongRange Range
        {
            get { return _range ?? LongRange.FromPivotValue(0); }
            set { _range = value; }
        }

        private FilterFavoriteCount()
        {
            web = new WeakEventBinder<EventDescriptionEventArgs>(EventStorage.EventRegisteredEvent);
            web.Notify += (_, edev) =>
                {
                    if (edev.EventDescription.Kind == EventKind.Favorite || edev.EventDescription.Kind == EventKind.Unfavorite)
                    {
                        this.RaisePartialRequireReaccept(edev.EventDescription.TargetTweet.Status);
                    }
                };
        }

        public FilterFavoriteCount(LongRange range) : this()
        {
            this.Range = range;
        }

        public FilterFavoriteCount(long pivot) : this()
        {
            this.Range = LongRange.FromPivotValue(pivot);
        }
        
        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            var ts = TweetStorage.Get(status.Id);
            if (ts == null) return false;
            return this.Range.Check(ts.FavoredUsers.Count());
        }

        public override string Identifier
        {
            get { return "fav_count"; }
        }

        public override IEnumerable<object> GetArgumentsForQueryify()
        {
            yield return this.Range;
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
