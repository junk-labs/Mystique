using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.Model;
using Dulcet.Twitter.Rest;

namespace Inscribe.Communication.CruiseControl.DefaultTasks
{
    public class FavoritesReceiveTask : ReceiveTaskBase
    {
        private AccountInfo _accountInfo;
        public AccountInfo AccountInfo
        {
            get { return this.AccountInfo; }
        }

        public FavoritesReceiveTask(AccountInfo info)
        {
            this._accountInfo = info;
        }

        protected override IEnumerable<Dulcet.Twitter.TwitterStatusBase> GetTweets()
        {
            return ApiHelper.ExecApi(() => this.AccountInfo.GetFavorites());
        }

        protected override int ReceiveCount
        {
            get { return TwitterDefine.DefaultReceiveCount; }
        }
    }
}
