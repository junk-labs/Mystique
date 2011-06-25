using System;
using System.Threading.Tasks;
using Dulcet.Twitter.Rest;
using Inscribe.Communication.CruiseControl.DefaultTasks;
using Inscribe.Model;
using Supervisor;
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
            this.TaskRateLimit = this._accountInfo.RateLimitRemaining;
            int wndTime = (int)this._accountInfo.RateLimitReset.Subtract(DateTime.Now).TotalMilliseconds;
            if (wndTime < 0)
                this.WindowTime = 0;
            else
                this.WindowTime = wndTime;
        }

        protected override void OnWakeup()
        {
            this.TargetMu = this._accountInfo.AccoutProperty.AutoCruiseDefaultMu;
        }
    }
}
