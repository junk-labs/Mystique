using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dulcet.Twitter;
using Dulcet.Twitter.Rest;
using Inscribe.Filter.Core;
using Inscribe.Common;
using Inscribe.Storage;
using System.Linq;

namespace Inscribe.Filter.Filters.Particular
{
    public class FilterMentionTree : FilterBase
    {
        long origTracePoint;

        long tracePoint;

        [GuiVisible("トレースポイントID")]
        public long OrigTracePoint
        {
            get { return origTracePoint; }
            set
            {
                origTracePoint = value;
                tracePoint = value;
                RecursiveCheckId(tracePoint);
            }
        }

        private FilterMentionTree() { }

        public FilterMentionTree(long id)
        {
            origTracePoint = id;
            tracePoint = id;
            Task.Factory.StartNew(() => RecursiveCheckId(id));
        }

        private void RecursiveCheckId(long id)
        {
            if (id == 0)
            {
                RaiseRequireReaccept();
                return;
            }
            var cont = TweetStorage.Contains(id);
            if (cont == TweetExistState.Exists)
            {
                // データをチェックして、先があれば再帰
                var tweet = TweetStorage.Get(id);
                if (tweet == null)
                {
                    RaiseRequireReaccept();
                    return;
                }
                var ts = tweet.Status as TwitterStatus;
                if (ts != null && ts.InReplyToStatusId != 0)
                {
                    this.tracePoint = ts.InReplyToStatusId;
                    RaisePartialRequireReaccept(ts);
                    RecursiveCheckId(ts.InReplyToStatusId);
                    tweet.RefreshInReplyToInfo(); // 返信情報の更新を通知
                }
                else
                {
                    RaiseRequireReaccept();
                }
            }
            else if (cont == TweetExistState.ServerDeleted)
            {
                // 消されてるからダメ
                RaiseRequireReaccept();
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
                            var vm = TweetStorage.Register(status);
                            Task.Factory.StartNew(() => RecursiveCheckId(status.Id));
                            Task.Factory.StartNew(() =>
                                TweetStorage.GetAll(tvm => (tvm.Status is TwitterStatus) && ((TwitterStatus)tvm.Status).InReplyToStatusId == id)
                                    .ForEach(tvm => tvm.RefreshInReplyToInfo()));
                        }
                        else
                        {
                            RaiseRequireReaccept();
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionStorage.Register(e, ExceptionCategory.TwitterError, "ツイート " + id + " の受信に失敗しました。", receive);
                        RaiseRequireReaccept();
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
            if (ts == null || ts.InReplyToStatusId == 0)
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
