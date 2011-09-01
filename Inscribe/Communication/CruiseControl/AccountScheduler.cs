using System;
using System.Threading.Tasks;
using Dulcet.Twitter.Rest;
using Inscribe.Authentication;
using Inscribe.Commnuication.CruiseControl.Core;
using Inscribe.Common;
using Inscribe.Communication.CruiseControl.DefaultTasks;
using Inscribe.Storage;
using Inscribe.Threading;

namespace Inscribe.Communication.CruiseControl
{
    public class AccountScheduler : SupervisorScheduler
    {
        private AccountInfo _accountInfo;
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
            Task.Factory.StartNew(() =>
                {
                    try
                    {
                        // テストを飛ばす
                        ApiHelper.ExecApi(() => info.Test());
                    }
                    catch { }
                });
            ThreadHelper.Halt += () => this.StopSchedule();
        }

        protected override void OnFallingASleep()
        {
            this.TaskRateLimit = this._accountInfo.RateLimitRemaining
                - (int)(this._accountInfo.RateLimitMax * (1 - this._accountInfo.AccoutProperty.AutoCruiseApiConsumeRate));
            int wndTime = (int)this._accountInfo.RateLimitReset.Subtract(DateTime.Now).TotalMilliseconds;
            if (wndTime < 0)
                this.WindowTime = 0;
            else
                this.WindowTime = wndTime;
        }

        protected override void OnWakeup()
        {
            try
            {
                this.TargetMu = this._accountInfo.AccoutProperty.AutoCruiseDefaultMu;
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
