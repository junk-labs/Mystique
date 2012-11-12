using System.Collections.Generic;
using System.Threading.Tasks;
using Dulcet.Twitter.Rest;
using Inscribe.Common;
using Inscribe.Filter.Core;
using Inscribe.Storage;

namespace Inscribe.Filter.Filters.Numeric
{
    public class FilterStatusId : FilterBase
    {
        private LongRange _range;

        [GuiVisible("ステータスID範囲")]
        public LongRange Range
        {
            get { return _range ?? LongRange.FromPivotValue(0); }
            set { _range = value; }
        }

        private FilterStatusId() { }

        public FilterStatusId(LongRange range)
        {
            this.Range = range;
        }

        public FilterStatusId(long pivot, bool getStatus)
        {
            this.Range = LongRange.FromPivotValue(pivot);

            if (getStatus)
            {
                if (TweetStorage.Contains(pivot) != TweetExistState.Exists)
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            var status = ApiHelper.ExecApi(() => AccountStorage.GetRandom().GetStatus(pivot));
                            if (status != null)
                            {
                                TweetStorage.Register(status);
                                RaisePartialRequireReaccept(status);
                            }
                        }
                        catch { }
                    });
            }
        }

        public FilterStatusId(long pivot)
            : this(pivot, false)
        { }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            return this.Range.Check(status.Id);
        }

        public override string Identifier
        {
            get { return "id"; }
        }

        public override IEnumerable<object> GetArgumentsForQueryify()
        {
            yield return this.Range;
        }

        public override string Description
        {
            get { return "ツイートのID"; }
        }

        public override string FilterStateString
        {
            get { return "ツイートID:" + this.Range.ToString(); }
        }
    }
}
