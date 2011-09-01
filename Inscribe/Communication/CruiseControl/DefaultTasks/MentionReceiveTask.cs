using System.Collections.Generic;
using Dulcet.Twitter.Rest;
using Inscribe.Authentication;
using Inscribe.Common;

namespace Inscribe.Communication.CruiseControl.DefaultTasks
{
    public class MentionReceiveTask : ReceiveTaskBase
    {
        private AccountInfo _accountInfo;
        public AccountInfo AccountInfo
        {
            get { return this._accountInfo; }
        }
        
        public MentionReceiveTask(AccountInfo info)
        {
            this._accountInfo = info;
        }

        protected override IEnumerable<Dulcet.Twitter.TwitterStatusBase> GetTweets()
        {
            return ApiHelper.ExecApi(() => this.AccountInfo.GetMentions(count: TwitterDefine.MentionReceiveMaxCount));
        }

        protected override int ReceiveCount
        {
            get { return TwitterDefine.MentionReceiveMaxCount; }
        }
    }
}
