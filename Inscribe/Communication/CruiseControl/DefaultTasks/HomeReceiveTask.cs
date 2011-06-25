using System.Collections.Generic;
using Dulcet.Twitter.Rest;
using Inscribe.Model;
using Dulcet.Twitter;

namespace Inscribe.Communication.CruiseControl.DefaultTasks
{
    public class HomeReceiveTask : ReceiveTaskBase
    {
        private AccountInfo _accountInfo;
        public AccountInfo AccountInfo
        {
            get { return this._accountInfo; }
        }

        public HomeReceiveTask(AccountInfo related)
        {
            this._accountInfo = related;
        }

        protected override IEnumerable<Dulcet.Twitter.TwitterStatusBase> GetTweets()
        {
            return ApiHelper.ExecApi(() => AccountInfo.GetHomeTimeline(count: TwitterDefine.HomeReceiveMaxCount));
        }

        protected override int ReceiveCount
        {
            get { return TwitterDefine.HomeReceiveMaxCount; }
        }
    }
}
