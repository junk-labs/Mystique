using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inscribe.Storage;
using Dulcet.Twitter;
using Dulcet.Twitter.Rest;
using Inscribe.Communication;

namespace Inscribe.Filter.Filters.Paticular
{
    public class FilterMentionTree : FilterBase
    {

        long origTracePoint;
        long tracePoint;

        private FilterMentionTree() { }

        public FilterMentionTree(long id)
        {
            origTracePoint = id;
            tracePoint = id;
            Task.Factory.StartNew(() => RecursiveCheckId(id)).ContinueWith(_ => RaiseRequireReaccept());
        }

        private void RecursiveCheckId(long id)
        {
            var cont = TweetStorage.Contains(id);
            if (cont == TweetExistState.Exists)
            {
                // データをチェックして、先があれば再帰
                var tweet = TweetStorage.Get(id);
                if (tweet == null) return;
                var ts = tweet.Status as TwitterStatus;
                if (ts == null) return;
                if (ts.InReplyToStatusId == 0) return;
                this.tracePoint = ts.InReplyToStatusId;
                RecursiveCheckId(ts.InReplyToStatusId);
            }
            else if (cont == TweetExistState.ServerDeleted)
            {
                // 消されてるからダメ
                return;
            }
            else
            {
                // tweetを受信しようか
                Action receive = null;
                receive = () =>
                {
                    try
                    {
                        var status = ApiHelper.ExecApi(() => AccountStorage.GetRandom().GetStatus(id));
                        if (status != null)
                        {
                            TweetStorage.Register(status);
                            Task.Factory.StartNew(() => RecursiveCheckId(id)).ContinueWith(_ => RaiseRequireReaccept());
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionStorage.Register(e, ExceptionCategory.TwitterError, "ツイート " + id + " の受信に失敗しました。", receive);
                    }
                };
                Task.Factory.StartNew(() => receive());
            }
        }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            return TraceId(status.Id);
        }

        private bool TraceId(long id)
        {
            var vm = TweetStorage.Get(id);
            if (vm == null || !vm.IsStatusInfoContains)
                return false;
            if (vm.Status.Id == tracePoint)
                return true;
            var ts = vm.Status as TwitterStatus;
            if (ts.InReplyToStatusId == 0)
                return false;
            else
                return TraceId(ts.InReplyToStatusId);
        }

        public override string Identifier
        {
            get { return "mtree"; }
        }

        public override IEnumerable<object> GetArgumentsForQueryify()
        {
            yield return this.origTracePoint;
        }

        public override string Description
        {
            get { return "指定したIDからの返信ツリー"; }
        }

        public override string FilterStateString
        {
            get { return "ID " + this.origTracePoint + " の返信ツリー"; }
        }
    }
}
