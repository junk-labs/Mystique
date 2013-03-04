using System;
using System.Threading.Tasks;
using Dulcet.Twitter.Rest;
using Inscribe.Authentication;
using Inscribe.Communication.CruiseControl.Core;
using Inscribe.Common;
using Inscribe.Communication.CruiseControl.DefaultTasks;
using Inscribe.Storage;

namespace Inscribe.Communication.CruiseControl
{
    public class AccountScheduler : SimpleScheduler
    {
        public override int RateLimitPerHour
        {
            get { return 15; }
        }

        private readonly AccountInfo _accountInfo;

        public AccountInfo AccountInfo
        {
            get { return this._accountInfo; }
        }

        public AccountScheduler(AccountInfo info)
        {
            this._accountInfo = info;
            this.AddSchedule(new HomeReceiveTask(info));
            this.AddSchedule(new MentionReceiveTask(info));
            this.AddSchedule(new DirectMessageReceiveTask(info));
            this.AddSchedule(new SentDirectMessageReceiveTask(info));
            this.AddSchedule(new FavoritesReceiveTask(info));
            this.AddSchedule(new MyTweetsTask(info));
            ThreadHelper.Halt += this.StopSchedule;
        }
    }
}
