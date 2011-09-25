using System.Collections.Generic;
using System.Linq;
using Inscribe.Filter.Core;
using Inscribe.Storage;
using Dulcet.Twitter;

namespace Inscribe.Filter.Filters.Numeric
{
    public class FilterRetweetCount : FilterBase
    {
        private WeakEventBinder<EventDescriptionEventArgs> eweb;

        private WeakEventBinder<TweetStorageChangedEventArgs> tweb;

        private LongRange _range;

        [GuiVisible("Retweet数範囲")]
        public LongRange Range
        {
            get { return _range ?? LongRange.FromPivotValue(0); }
            set { _range = value; }
        }

        private FilterRetweetCount()
        {
            eweb = new WeakEventBinder<EventDescriptionEventArgs>(EventStorage.EventRegisteredEvent);
            eweb.Notify += (_, edev) =>
            {
                if (edev.EventDescription.Kind == EventKind.Retweet)
                {
                    this.RaisePartialRequireReaccept(edev.EventDescription.TargetTweet.Status);
                }
            };
            tweb = new WeakEventBinder<TweetStorageChangedEventArgs>(TweetStorage.TweetStorageChangedEvent);
            tweb.Notify += (_, tcev) =>
                {
                    if (tcev.ActionKind == TweetActionKind.Removed && tcev.Tweet.Status is TwitterStatus && tcev.Tweet.IsPublishedByRetweet)
                    {
                        this.RaisePartialRequireReaccept(((TwitterStatus)tcev.Tweet.Status).RetweetedOriginal);
                    }
                };
        }

        public FilterRetweetCount(LongRange range)
            : this()
        {
            this.Range = range;
        }

        public FilterRetweetCount(long pivot)
            : this()
        {
            this.Range = LongRange.FromPivotValue(pivot);
        }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            var ts = TweetStorage.Get(status.Id);
            if (ts == null) return false;
            return this.Range.Check(ts.RetweetedUsers.Count());
        }

        public override string Identifier
        {
            get { return "rt_count"; }
        }

        public override IEnumerable<object> GetArgumentsForQueryify()
        {
            yield return this.Range;
        }

        public override string Description
        {
            get { return "被リツイート数"; }
        }

        public override string FilterStateString
        {
            get { return "被RT数が " + this.Range.ToString() + " であるもの"; }
        }
    }
}
