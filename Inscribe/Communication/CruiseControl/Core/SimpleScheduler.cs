using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Inscribe.Communication.CruiseControl.Core
{
    public abstract class SimpleScheduler
    {
        /// <summary>
        /// 次回の更新時に追加されるタスク一覧
        /// </summary>
        private ConcurrentQueue<ScheduledTask> addCandidates;

        /// <summary>
        /// スケジュールされているタスク一覧とレートリミット per hours
        /// </summary>
        private List<ScheduledTask> scheduledTasks;

        /// <summary>
        /// スケジュールを実行するスレッド
        /// </summary>
        private Thread schedulerThread;

        public SimpleScheduler()
        {
            this.addCandidates = new ConcurrentQueue<ScheduledTask>();
            this.scheduledTasks = new List<ScheduledTask>();
            this.schedulerThread = null;
        }

        /// <summary>
        /// スケジューラスレッドが稼働しているか確認します。(非スレッドセーフ)
        /// </summary>
        public bool IsSchedulerRunning
        {
            get { return this.schedulerThread != null; }
        }

        /// <summary>
        /// このスケジューラにスケジュールを追加します。
        /// </summary>
        public void AddSchedule(ScheduledTask task)
        {
            this.addCandidates.Enqueue(task);
        }

        public abstract int RateLimitPerHour { get; }

        /// <summary>
        /// スケジューラを開始します。(非スレッドセーフ)
        /// </summary>
        public void StartSchedule()
        {
            if (this.schedulerThread != null)
            {
                throw new InvalidOperationException("Thread is already running.");
            }
            this.schedulerThread = new Thread(SchedulerCore);
            this.schedulerThread.Start();
        }

        /// <summary>
        /// スケジューラを停止します。(非スレッドセーフ)
        /// </summary>
        public void StopSchedule()
        {
            if (this.schedulerThread == null)
                return;
            this.schedulerThread.Abort();
            this.schedulerThread = null;
        }

        /// <summary>
        /// スケジューリングアルゴリズムコア
        /// </summary>
        private void SchedulerCore()
        {
            while (true)
            {
                // キュー済みの追加待ちスケジュールを追加
                while (this.addCandidates.Count > 0)
                {
                    ScheduledTask st;
                    if (this.addCandidates.TryDequeue(out st))
                        this.scheduledTasks.Add(st);
                    else
                        break;
                }

                // 既に死んでるタスクを排除
                this.scheduledTasks.RemoveAll(s => !s.IsAlive);

                // タ ス ク が な い
                if (!this.scheduledTasks.Any())
                {
                    Thread.Sleep(10000);
                    continue;
                }

                foreach (var task in scheduledTasks)
                {
                    Task.Factory.StartNew(task.DoWork);
                }
                Thread.Sleep(60 * 60 * 1000 / RateLimitPerHour);
            }
        }
    }
}
