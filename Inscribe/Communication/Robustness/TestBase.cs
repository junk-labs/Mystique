using System.Threading;

namespace Inscribe.Communication.Robustness
{
    /// <summary>
    /// 特定の操作を反復的に試行するベースです。
    /// </summary>
    public abstract class TestBase
    {
        protected abstract bool Try();

        private int _maxThreadSleep = 1000 * 8;
        public int MaxThreadSleep
        {
            get { return this._maxThreadSleep; }
            set { this._maxThreadSleep = value; }
        }

        private int _minThreadSleep = 500;
        public int MinThreadSleep
        {
            get { return this._minThreadSleep; }
            set { this._minThreadSleep = value; }
        }

        /// <summary>
        /// 試行が成功するまでスレッドをブロックします。
        /// </summary>
        public void Test()
        {
            int tsleep = this.MinThreadSleep;
            while (!Try())
            {
                Thread.Sleep(tsleep);
                tsleep *= 2;
                if (tsleep > MaxThreadSleep)
                    tsleep = MaxThreadSleep;
            }
        }

        /// <summary>
        /// 最大試行回数試行を試します。
        /// </summary>
        /// <param name="maxTryCount">最大試行回数</param>
        /// <returns>試行が成功すればtrue</returns>
        public bool Test(int maxTryCount)
        {
            int tsleep = this.MinThreadSleep;
            int c = 0;
            while (!Try())
            {
                Thread.Sleep(tsleep);
                if (++c > maxTryCount)
                    return false;
                tsleep *= 2;
                if (tsleep > MaxThreadSleep)
                    tsleep = MaxThreadSleep;
            }
            return true;
        }
    }
}
