using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Inscribe.Model;
using Inscribe.Storage;
using Inscribe.Threading;
using Livet;

namespace Inscribe.Communication.UserStreams
{
    /// <summary>
    /// Management connection of User Streams.
    /// </summary>
    public static class ConnectionManager
    {
        private static ConcurrentDictionary<AccountInfo, UserStreamsConnection> connections = new ConcurrentDictionary<AccountInfo, UserStreamsConnection>();

        private static object _cmLocker = new object();

        #region ConnectionStateChangedイベント

        public static event EventHandler<EventArgs> ConnectionStateChanged;
        private static Notificator<EventArgs> _ConnectionStateChangedEvent;
        public static Notificator<EventArgs> ConnectionStateChangedEvent
        {
            get
            {
                if (_ConnectionStateChangedEvent == null) _ConnectionStateChangedEvent = new Notificator<EventArgs>();
                return _ConnectionStateChangedEvent;
            }
            set { _ConnectionStateChangedEvent = value; }
        }

        internal static void OnConnectionStateChanged(EventArgs e)
        {
            var threadSafeHandler = Interlocked.CompareExchange(ref ConnectionStateChanged, null, null);
            if (threadSafeHandler != null) threadSafeHandler(null, e);
            ConnectionStateChangedEvent.Raise(e);
        }

        #endregion

        /// <summary>
        /// 明示的な変更があった場合に接続を更新します。
        /// </summary>
        public static void RefreshReceivers()
        {
            lock (_cmLocker)
            {
                var exists = connections.Keys;
                var infos = AccountStorage.Accounts
                    .Where(i => i.AccoutProperty.UseUserStreams)
                    .ToArray();
                var addeds = infos.Except(exists).ToArray();
                var removes = exists.Except(infos).ToArray();
                var keeps = exists.Except(removes).ToArray();
                Parallel.ForEach(addeds, i =>
                {
                    // 新規接続
                    UserInformationManager.ReceiveInidividualInfo(i);
                    RefreshConnection(i);
                });
                foreach (var i in removes)
                {
                    // 登録削除
                    if (connections.ContainsKey(i))
                    {
                        var ccon = connections[i];
                        connections.Remove(i);
                    }
                }
                Parallel.ForEach(keeps, i =>
                {
                    RefreshConnection(i);
                });
            }
        }

        /// <summary>
        /// 接続を開始します。<para />
        /// すでに接続が存在する場合は、すでに存在している接続を破棄します。
        /// </summary>
        public static bool RefreshConnection(AccountInfo info)
        {
            if (info == null)
                throw new ArgumentNullException("info", "AccountInfo is not set.");
            var ncon = new UserStreamsConnection(info);
            if (connections.ContainsKey(info))
            {
                var pcon = connections[info];
                // 以前の接続がある
                connections[info] = ncon;
                if (pcon != null)
                    pcon.Dispose();
            }
            else
            {
                connections.AddOrUpdate(info, ncon);
            }
            var queries = lookupDictionary.Where(v => v.Value == info).Select(v => v.Key).ToArray();
            try
            {
                ncon.Connect(queries);
                return true;
            }
            catch (Exception e)
            {
                connections[info] = null;
                ncon.Dispose();
                ExceptionStorage.Register(e, ExceptionCategory.TwitterError,
                    "User Streams接続に失敗しました。", () =>
                        {
                            if (connections.ContainsKey(info) && connections[info] == null)
                                RefreshConnection(info);
                        });
                return false;
            }
        }

        #region Streaming-Query Control

        private static ConcurrentDictionary<string, AccountInfo> lookupDictionary = new ConcurrentDictionary<string, AccountInfo>();

        private static ConcurrentDictionary<string, int> referenceCount = new ConcurrentDictionary<string, int>();

        private static object addQueryLocker = new object();

        public static bool AddQuery(string query)
        {
            lock (addQueryLocker)
            {
                if (referenceCount.ContainsKey(query))
                {
                    referenceCount[query]++;
                    return true;
                }
                else
                {
                    if (!StartListenQuery(query))
                        return false;
                    referenceCount.AddOrUpdate(query, 1, (q, i) => i++);
                    return true;
                }
            }
        }

        private static bool StartListenQuery(string query)
        {
            if (connections.Count == 0)
            {
                // コネクションカウントが0の時はとりあえず受け付ける
                // まだ接続されていないんじゃないかな？
                var info = AccountStorage.Accounts
                    .Where(a => a.AccoutProperty.UseUserStreams)
                    .OrderByDescending(a => lookupDictionary.Where(v => v.Value == a).Count())
                    .FirstOrDefault();
                if (info == null)
                    return false;
                lookupDictionary.AddOrUpdate(query, info);
                return true;
            }
            else
            {
                var info = AccountStorage.Accounts
                    .Where(a => a.AccoutProperty.UseUserStreams && connections.ContainsKey(a))
                    .OrderByDescending(a => lookupDictionary.Where(v => v.Value == a).Count())
                    .FirstOrDefault();
                if (info == null)
                    return false;
                lookupDictionary.AddOrUpdate(query, info);
                RefreshConnection(info);
                return true;
            }
        }

        public static void RemoveQuery(string query)
        {
            if (query == null || !referenceCount.ContainsKey(query))
                return;
            referenceCount[query]--;
            if (referenceCount[query] == 0)
            {
                referenceCount.Remove(query);
                var info = lookupDictionary[query];
                lookupDictionary.Remove(query);
                RefreshConnection(info);
            }
        }

        public static void RemoveQueries(string[] queries)
        {
            queries.Where(q => referenceCount.ContainsKey(q))
                .Where(q => (referenceCount[q]-- == 0))
                .Select(q =>
                {
                    referenceCount.Remove(q);
                    var i = lookupDictionary[q];
                    lookupDictionary.Remove(q);
                    return i;
                }).Distinct()
                .ForEach(i => Task.Factory.StartNew(() => RefreshConnection(i)));
        }

        private static void RefreshQuery()
        {
            lookupDictionary.Keys
                .Where(k => !lookupDictionary[k].AccoutProperty.UseUserStreams)
                .ForEach(k =>
                {
                    lookupDictionary.Remove(k);
                    StartListenQuery(k);
                });
        }

        #endregion

        #region Reconnection Control

        private static Timer reconPatrolTimer = new Timer(DoPatrol, null, 5 * 60 * 1000, 5 * 60 * 1000);

        /// <summary>
        /// 不正に切断されているストリーミング接続があった場合、再接続を行います。
        /// </summary>
        /// <param name="o"></param>
        private static void DoPatrol(object o)
        {
            try
            {
                AccountStorage.Accounts
                    .Where(i => connections.ContainsKey(i) && (connections[i] == null || !connections[i].IsAlive))
                    .ForEach(i => Task.Factory.StartNew(() => RefreshConnection(i)));
            }
            catch (Exception e)
            {
                ExceptionStorage.Register(e, ExceptionCategory.InternalError,
                    "User Streamsの接続監視中にエラーが発生しました。", () => DoPatrol(null));
            }
        }

        /// <summary>
        /// 接続が切断されたことを通知します。
        /// </summary>
        internal static void NotifyDisconnected(AccountInfo accountInfo, Dulcet.Twitter.Streaming.StreamingConnection con)
        {
            if (accountInfo == null)
                throw new ArgumentException("accountInfo");
            accountInfo.ConnectionState = Model.ConnectionState.Disconnected;
            OnConnectionStateChanged(EventArgs.Empty);

            if (ThreadHelper.IsHalted) // アプリケーションが終了中
                return;

            UserStreamsConnection pusc;
            if (!connections.TryGetValue(accountInfo, out pusc) || pusc == null)
                return; // User Streams接続を行わない

            if (pusc.CheckUsingConnection(con))
            {
                // 切断通知されたコネクションを使用しているようだ
                // -> エラー切断であるので再接続
                RefreshConnection(accountInfo);
            }
            else
            {
                pusc.Dispose();
            }
        }

        #endregion
    }
}
