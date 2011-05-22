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

        private static bool OnHalt = false;

        public static event Action Halt = () => { };

        /// <summary>
        /// すべての実行スレッドを停止します。
        /// </summary>
        public static void HaltThreads()
        {
            lock (syncRoot)
            {
                if (OnHalt) return;
                OnHalt = true;
                Halt();
            }
        }
    }
}
