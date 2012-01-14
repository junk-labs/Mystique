using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.Core;

namespace Inscribe.Filter.Filters.Transcend
{
    public class ExceptedFilter : FilterBase
    {
        public override bool IsOnlyForTranscender
        {
            get { return true; }
        }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            // 自分自身が登録されているフィルタ以外の一覧を取得する
            var filters = KernelService.MainWindowViewModel.ColumnOwnerViewModel.Columns
                .SelectMany(c => c.TabItems.Select(t => t.TabProperty.TweetSources))
                .Where(f => f.SelectMany(i => Explode(i))
                    .OfType<ExceptedFilter>()
                    .FirstOrDefault() == null)
                .SelectMany(i => i);
            // どのフィルタにもキャプチャされない
            return filters.Where(f => f.Filter(status)).FirstOrDefault() == null;
        }

        private IEnumerable<FilterBase> Explode(IFilter filter)
        {
            var cluster = filter as FilterCluster;
            if (cluster != null && cluster.Filters != null)
                return cluster.Filters.SelectMany(Explode);
            else
                return new[] { filter as FilterBase };
        }

        public override string Identifier
        {
            get { return "uncaptured"; }
        }

        public override IEnumerable<object> GetArgumentsForQueryify()
        {
            yield break;
        }

        public override string Description
        {
            get { return "このフィルタが使われているタブを除く、どのタブにも振り分けられないツイートをフィルタします。(※重いです)"; }
        }

        public override string FilterStateString
        {
            get { return "どのタブにも振り分けられないツイート"; }
        }
    }
}
