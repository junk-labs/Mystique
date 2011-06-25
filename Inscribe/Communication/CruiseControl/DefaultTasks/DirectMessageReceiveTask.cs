using System.Collections.Generic;
using Dulcet.Twitter.Rest;
using Inscribe.Model;

namespace Inscribe.Communication.CruiseControl.DefaultTasks
{
    public class DirectMessageReceiveTask : ReceiveTaskBase
    {
        private AccountInfo _accountInfo;
        public AccountInfo AccountInfo
        {
            get { return this._accountInfo; }
        }

        public DirectMessageReceiveTask(AccountInfo info)
        {
            this._accountInfo = info;
        }

        protected override IEnumerable<Dulcet.Twitter.TwitterStatusBase> GetTweets()
        {
            return ApiHelper.ExecApi(() => this.AccountInfo.GetDirectMessages(count: TwitterDefine.DmReceiveMaxCount));
        }

        protected override int ReceiveCount
        {
            get { return TwitterDefine.DmReceiveMaxCount; }
        }
    }
}
