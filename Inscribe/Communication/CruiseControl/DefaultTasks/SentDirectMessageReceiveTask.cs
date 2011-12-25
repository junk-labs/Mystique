using System.Collections.Generic;
using Inscribe.Authentication;

namespace Inscribe.Communication.CruiseControl.DefaultTasks
{
    public class SentDirectMessageReceiveTask : ReceiveTaskBase
    {
        private AccountInfo _accountInfo;
        public AccountInfo AccountInfo
        {
            get { return this._accountInfo; }
        }

        public SentDirectMessageReceiveTask(AccountInfo info)
        {
            this._accountInfo = info;
        }

        protected override IEnumerable<Dulcet.Twitter.TwitterStatusBase> GetTweets()
        {
            return InjectionPoint._GetSentDirectMessagesInjection.Execute(AccountInfo);
        }

        protected override int ReceiveCount
        {
            get { return TwitterDefine.DmReceiveMaxCount; }
        }
    }
}
