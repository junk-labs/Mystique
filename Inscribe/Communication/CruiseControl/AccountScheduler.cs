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
    public class AccountScheduler : SupervisorScheduler
    {
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

        protected override void OnWakeup()
        {
            try
            {
                this.TargetMu = this._accountInfo.AccountProperty.AutoCruiseDefaultMu;
            }
            catch (Exception e)
            {
                ExceptionStorage.Register(e, ExceptionCategory.ConfigurationError, "設定が破損しています。");
                this.TargetMu = 0.5;
            }
        }

        protected override int MinWindowTime
        {
            get
            {
                return TwitterDefine.MinWindowTime;
            }
        }

        protected override double MinDensity
        {
            get
            {
                return TwitterDefine.MinDensity;
            }
        }
    }
}
