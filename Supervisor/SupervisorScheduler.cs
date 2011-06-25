using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Supervisor.Random;

namespace Supervisor
{
    /// <summary>
    /// スーパーバイザスケジューラ
    /// </summary>
    public class SupervisorScheduler
    {
        /// <summary>
        /// 次回の更新時に追加されるタスク一覧
        /// </summary>
        private ConcurrentQueue<ScheduledTask> addCandidates;

        /// <summary>
        /// スケジュールされているタスク一覧
        /// </summary>
        private List<ScheduledTask> scheduledTasks;

        /// <summary>
        /// スケジュールを実行するスレッド
        /// </summary>
        private Thread schedulerThread;

        /// <summary>
        /// SFMT
        /// </summary>
        private SFMT random;

        public SupervisorScheduler()
        {
            this.addCandidates = new ConcurrentQueue<ScheduledTask>();
            this.scheduledTasks = new List<ScheduledTask>();
            this.schedulerThread = null;
            this.random = new SFMT();
            this.Density = 1.0;
        }

        /// <summary>
        /// 現在のタスクレートリミットを使い切れるウィンドウタイム(msec)
        /// </summary>
        public int WindowTime { get; set; }

        /// <summary>
        /// 残りタスクレートリミット(回)
        /// </summary>
        public int TaskRateLimit { get; set; }

        private double _targetMu = 0.5;
        /// <summary>
        /// 目標レート平均(0-1)
        /// </summary>
        public double TargetMu
        {
            get { return this._targetMu; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentOutOfRangeException("TargetMu", "目標レート平均は0-1で指定する必要があります。");
                this._targetMu = value;
            }
        }

        /// <summary>
        /// スケジューラスレッドが稼働しているか確認します。(非スレッドセーフ)
        /// </summary>
        public bool IsSchedulerRunning
        {
            get { return this.schedulerThread != null; }
        }

        private double _density;
        /// <summary>
        /// 実行密度
        /// </summary>
        protected double Density
        {
            get { return this._density; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentOutOfRangeException("Density", "実行密度は0-1で指定される必要があります。");
                this._density = value;
            }
        }

        /// <summary>
        /// このスケジューラにスケジュールを追加します。
        /// </summary>
        public void AddSchedule(ScheduledTask task)
        {
            this.addCandidates.Enqueue(task);
        }

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
                this.OnFallingASleep();
                // 待機時間導出、待機
                var wait = (int)(this.WindowTime / (this.TaskRateLimit * this.Density));
                Thread.Sleep(wait);
                this.OnWakeup();
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
                if (this.scheduledTasks.Count() == 0)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                // R, T, ∑Ri, μを導出
                var rand = this.random.NextDouble();
                var tasks = this.scheduledTasks.OrderBy(t => t.Rate);
                var sigma = this.scheduledTasks.Sum(t => t.Rate);
                var mu = this.scheduledTasks.Average(t => t.Rate);

                // タスクを比率に従いランダム選択
                double sum = 0;
                foreach (var task in tasks)
                {
                    sum += (task.Rate / sigma);
                    if (sum < rand)
                    {
                        this.OnTaskRun();
                        Task.Factory.StartNew(task.DoWork);
                        break;
                    }
                }

                // μの値により実行密度を変化
                var nd = this.Density;
                if (mu < this.TargetMu)
                    nd += Math.Pow(mu - this.TargetMu, 2);
                else
                    nd -= Math.Pow(mu - this.TargetMu, 2);
                if (nd < 0)
                    nd = 0;
                else if (nd > 1)
                    nd = 1;
                this.Density = nd;
            }
        }

        protected virtual void OnFallingASleep()
        {
        }

        protected virtual void OnWakeup()
        {
        }

        protected virtual void OnTaskRun()
        {
        }
    }
}
