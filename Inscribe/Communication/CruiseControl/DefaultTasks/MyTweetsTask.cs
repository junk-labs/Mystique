using System.Collections.Generic;
using Dulcet.Twitter.Rest;
using Inscribe.Authentication;
using Inscribe.Common;

namespace Inscribe.Communication.CruiseControl.DefaultTasks
{
    public class MyTweetsTask : ReceiveTaskBase
    {
        private AccountInfo _accountInfo;
        public AccountInfo AccountInfo
        {
            get { return this._accountInfo; }
        }

        public MyTweetsTask(AccountInfo info)
        {
            this._accountInfo = info;
        }

        protected override int ReceiveCount
        {
            get { return TwitterDefine.HomeReceiveMaxCount; }
        }

        protected override IEnumerable<Dulcet.Twitter.TwitterStatusBase> GetTweets()
        {
            return ApiHelper.ExecApi(() => AccountInfo.GetUserTimeline(screenName: AccountInfo.ScreenName, count: TwitterDefine.HomeReceiveMaxCount, includeRts: true));
        }
    }
}
