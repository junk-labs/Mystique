
namespace Inscribe.Commnuication.CruiseControl.Core
{
    public abstract class ScheduledTask
    {
        /// <summary>
        /// スケジューリング レート
        /// </summary>
        public abstract double Rate { get; }

        private bool _isAlive = true;
        /// <summary>
        /// このスケジュールが生存しているか
        /// </summary>
        public bool IsAlive
        {
            get { return this._isAlive; }
        }

        /// <summary>
        /// 仕事を実行します。
        /// </summary>
        public abstract void DoWork();

        /// <summary>
        /// このタスクを破棄します。
        /// </summary>
        protected void RemoveSchedule()
        {
            this._isAlive = false;
        }
    }
}
