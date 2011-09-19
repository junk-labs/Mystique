using System.Collections.Generic;
using Inscribe.Authentication;

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
            return InjectionPoint._GetFavoritesInjection.Execute(AccountInfo);
        }
    }
}
