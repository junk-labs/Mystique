using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Inscribe.Data;
using Inscribe.Configuration;
using System.Threading.Tasks;
using Inscribe.Threading;

namespace Inscribe.Communication.Lists
{
    /// <summary>
    /// リスト受信のマネージメントクラス
    /// </summary>
    public static class ListReceiverManager
    {
        private static ReaderWriterLockWrap rvLocker = new ReaderWriterLockWrap();

        private static Dictionary<string, ListReceiver> receivers = new Dictionary<string, ListReceiver>();

        private static ReaderWriterLockWrap rcLocker = new ReaderWriterLockWrap();

        private static Dictionary<string, int> referenceCount = new Dictionary<string, int>();

        private static Timer secTimer = new Timer(Tick, null, 1000, 1000);

        static ListReceiverManager()
        {
            ThreadHelper.Halt += () => secTimer.Dispose();
        }

        public static void RegisterReceive(string list)
        {
            var nlist = NormalizeName(list);
            using (rcLocker.GetWriterLock())
            {
                if (referenceCount.ContainsKey(list))
                {
                    referenceCount[list]++;
                }
                else
                {
                    using (rvLocker.GetWriterLock())
                    {
                        referenceCount.Add(list, 1);
                    }
                    receivers.Add(list, new ListReceiver(list));
                }
            }
        }

        public static void RemoveReceive(string list)
        {
            var nlist = NormalizeName(list);
            using (rcLocker.GetWriterLock())
            {
                if (referenceCount.ContainsKey(list))
                {
                    referenceCount[list]--;
                    if (referenceCount[list] == 0)
                    {
                        referenceCount.Remove(list);
                        using (rvLocker.GetWriterLock())
                        {
                            receivers.Remove(list);
                        }
                    }
                }
            }
        }

        private static string NormalizeName(string name)
        {
            if (name.Length < 4 || name[0] != '@')
                throw new FormatException("リスト名フォーマットが異常です:" + name);
            var delimiter = name.IndexOf('/');
            try
            {
                var uname = name.Substring(1, delimiter - 1);
                var lname = name.Substring(delimiter + 1);
                return "@" + uname.ToLower() + "/" + lname.ToLower().Replace("_", "-");
            }
            catch
            {
                throw new FormatException("リスト名フォーマットが異常です:" + name);
            }

        }

        /// <summary>
        /// フル リスト名を作成します。
        /// </summary>
        public static string BuildListName(string user, string list)
        {
            if (String.IsNullOrEmpty(user))
                throw new ArgumentNullException("user");
            if (String.IsNullOrEmpty("list"))
                throw new ArgumentNullException("list");
            if (user[0] == '@')
                return user + "/" + list;
            else
                return "@" + user + "/" + list;
        }

        private static long listCounter = 0;

        private static void Tick(object o)
        {
            var lc = Interlocked.Increment(ref listCounter);
            if (lc > Setting.Instance.ConnectionProperty.ListReceiveIntervalSec)
            {
                var lcp = Interlocked.Exchange(ref listCounter, 0);
                // 既に初期化されている
                if (lcp == 0)
                    return;
                else
                {
                    // 受信処理
                    using (rvLocker.GetReaderLock())
                    {
                        Parallel.ForEach(receivers.Values, r => r.ReceiveList());
                    }
                }
            }
        }
    }
}
