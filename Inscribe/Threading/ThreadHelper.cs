using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Inscribe.Threading
{
    public static class ThreadHelper
    {
        private static object syncRoot = new object();

        private static bool onHalt = false;
        public static bool IsHalted
        {
            get { return onHalt; }
        }

        public static event Action Halt = () => { };

        /// <summary>
        /// すべての実行スレッドを停止します。
        /// </summary>
        public static void HaltThreads()
        {
            lock (syncRoot)
            {
                if (onHalt) return;
                onHalt = true;
                Halt();
            }
        }
    }
}
