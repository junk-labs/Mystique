using System.Collections.Generic;
using Inscribe.Authentication;

namespace Inscribe.Communication.CruiseControl.DefaultTasks
{
    public class FavoritesReceiveTask : ReceiveTaskBase
    {
        private AccountInfo _accountInfo;
        public AccountInfo AccountInfo
        {
            get { return this._accountInfo; }
        }

        public FavoritesReceiveTask(AccountInfo info)
        {
            this._accountInfo = info;
        }

        protected override IEnumerable<Dulcet.Twitter.TwitterStatusBase> GetTweets()
        {
            return InjectionPoint._GetFavoritesInjection.Execute(AccountInfo);
        }

        protected override int ReceiveCount
        {
            get { return TwitterDefine.DefaultReceiveCount; }
        }
    }
}
