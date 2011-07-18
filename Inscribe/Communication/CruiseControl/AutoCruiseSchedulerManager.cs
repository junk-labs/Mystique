using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Inscribe.Model;
using Inscribe.Storage;
using Livet;

namespace Inscribe.Communication.CruiseControl
{
    public static class AutoCruiseSchedulerManager
    {
        #region SchedulerUpdatedイベント

        public static event EventHandler<EventArgs> SchedulerUpdated;
        private static Notificator<EventArgs> _SchedulerUpdatedEvent;
        public static Notificator<EventArgs> SchedulerUpdatedEvent
        {
            get
            {
                if (_SchedulerUpdatedEvent == null) _SchedulerUpdatedEvent = new Notificator<EventArgs>();
                return _SchedulerUpdatedEvent;
            }
            set { _SchedulerUpdatedEvent = value; }
        }

        private static void OnSchedulerUpdated(EventArgs e)
        {
            var threadSafeHandler = Interlocked.CompareExchange(ref SchedulerUpdated, null, null);
            if (threadSafeHandler != null) threadSafeHandler(null, e);
            SchedulerUpdatedEvent.Raise(e);
        }

        #endregion

        private static ConcurrentDictionary<AccountInfo, AccountScheduler> schedulers;

        static AutoCruiseSchedulerManager()
        {
            schedulers = new ConcurrentDictionary<AccountInfo, AccountScheduler>();
        }

        public static void Begin()
        {
            AccountStorage.AccountsChanged += new EventHandler<EventArgs>(AccountsChanged);
            AccountsChanged(null, EventArgs.Empty);
        }

        static void AccountsChanged(object sender, EventArgs e)
        {
            var contains = schedulers.Keys;
            var accounts = AccountStorage.Accounts;
            var newbies = accounts.Except(contains);
            var olds = contains.Except(accounts);
            if (newbies.Count() > 0)
                newbies.ForEach(i =>
                {
                    var item = new AccountScheduler(i);
                    schedulers.AddOrUpdate(i, item);
                    item.StartSchedule();
                });
            if (olds.Count() > 0)
                olds.ForEach(i =>
                {
                    var item = schedulers[i];
                    schedulers.Remove(i);
                    item.StopSchedule();
                });
            OnSchedulerUpdated(EventArgs.Empty);
        }

        public static AccountScheduler GetScheduler(AccountInfo info)
        {
            AccountScheduler sched;
            if (schedulers.TryGetValue(info, out sched))
                return sched;
            else
                return null;
        }
    }
}
