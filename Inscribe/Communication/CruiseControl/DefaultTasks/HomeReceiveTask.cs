using System.Collections.Generic;
using Dulcet.Twitter;
using Inscribe.Authentication;

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

        protected override IEnumerable<TwitterStatusBase> GetTweets()
        {
            return InjectionPoint._GetHomeTimelineInjection.Execute(AccountInfo);
        }

        protected override int ReceiveCount
        {
            get { return TwitterDefine.HomeReceiveMaxCount; }
        }
    }
}
