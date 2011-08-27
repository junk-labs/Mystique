using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Dulcet.Twitter;
using Inscribe.Configuration;
using Inscribe.Subsystems;
using Inscribe.Text;
using Inscribe.Threading;
using Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild;
using Livet;
using Inscribe.Common;

namespace Inscribe.Storage
{
    /// <summary>
    /// ツイートの存在状態
    /// </summary>
    public enum TweetExistState
    {
        Unreceived,
        EmptyExists,
        Exists,
        ServerDeleted,
    }

    /// <summary>
    /// ツイートデータ(ViewModel)保持ベースクラス
    /// </summary>
    public static class TweetStorage
    {
        /// <summary>
        /// 登録済みステータスディクショナリ
        /// </summary>
        static ConcurrentDictionary<long, TweetViewModel> dictionary = new ConcurrentDictionary<long, TweetViewModel>();

        /// <summary>
        /// 仮登録ステータスディクショナリ
        /// </summary>
        static ConcurrentDictionary<long, TweetViewModel> empties = new ConcurrentDictionary<long, TweetViewModel>();

        /// <summary>
        /// 削除予約されたツイートIDリスト
        /// </summary>
        static ConcurrentBag<long> deleteReserveds = new ConcurrentBag<long>();

        /// <summary>
        /// ツイートストレージの作業用スレッドディスパッチャ
        /// </summary>
        static QueueTaskDispatcher operationDispatcher;

        static TweetStorage()
        {
            operationDispatcher = new QueueTaskDispatcher(1);
            ThreadHelper.Halt += () => operationDispatcher.Dispose();
        }

        /// <summary>
        /// ツイートを受信したか、また明示的削除などが行われたかを確認します。
        /// </summary>
        public static TweetExistState Contains(long id)
        {
            if (dictionary.ContainsKey(id))
                return TweetExistState.Exists;
            else if (deleteReserveds.Contains(id))
                return TweetExistState.ServerDeleted;
            else if (empties.ContainsKey(id))
                return TweetExistState.EmptyExists;
            else
                return TweetExistState.Unreceived;
        }

        /// <summary>
        /// ツイートデータを取得します。
        /// </summary>
        /// <param name="id">ツイートID</param>
        /// <param name="createEmpty">存在しないとき、空のViewModelを作って登録して返す</param>
        public static TweetViewModel Get(long id, bool createEmpty = false)
        {
            TweetViewModel ret;
            if (dictionary.TryGetValue(id, out ret))
                return ret;
            if (empties.TryGetValue(id, out ret))
                return ret;
            if (createEmpty)
            {
                var nvm = new TweetViewModel(id);
                empties.AddOrUpdate(id, nvm);
                return nvm;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 登録されているステータスを抽出します。
        /// </summary>
        /// <param name="predicate">抽出条件</param>
        /// <returns>条件にマッチするステータス、または登録されているすべてのステータス</returns>
        public static IEnumerable<TweetViewModel> GetAll(Func<TweetViewModel, bool> predicate = null)
        {
            if (predicate == null)
                return dictionary.Values.ToArray();
            else
                return dictionary.Values.Where(predicate).ToArray();
        }

        /// <summary>
        /// 登録されているツイートの個数を取得します。
        /// </summary>
        public static int Count()
        {
            return dictionary.Count;
        }

        /// <summary>
        /// 受信したツイートを登録します。<para />
        /// 諸々の処理を自動で行います。
        /// </summary>
        public static TweetViewModel Register(TwitterStatusBase statusBase)
        {
            if (dictionary.ContainsKey(statusBase.Id))
                return dictionary[statusBase.Id];
            var status = statusBase as TwitterStatus;
            if (status != null)
            {
                return RegisterStatus(status);
            }
            else
            {
                var dmsg = statusBase as TwitterDirectMessage;
                if (dmsg != null)
                {
                    return RegisterDirectMessage(dmsg);
                }
                else
                {
                    throw new InvalidOperationException("不明なステータスを受信しました: " + statusBase);
                }
            }
        }

        /// <summary>
        /// ステータスの追加に際しての処理
        /// </summary>
        private static TweetViewModel RegisterStatus(TwitterStatus status)
        {
            if (status.RetweetedOriginal != null)
            {
                // リツイートのオリジナルステータスを登録
                var vm = Register(status.RetweetedOriginal);

                // リツイートユーザーに登録
                var user = UserStorage.Get(status.User);
                var tuser = UserStorage.Get(status.RetweetedOriginal.User);
                if (vm.RegisterRetweeted(user))
                {
                    if (!vm.IsStatusInfoContains)
                        vm.SetStatus(status.RetweetedOriginal);
                    // 自分が関係していれば
                    if (AccountStorage.Contains(status.RetweetedOriginal.User.ScreenName)
                        || AccountStorage.Contains(user.TwitterUser.ScreenName))
                        EventStorage.OnRetweeted(vm, user);
                }
            }

            // 返信先の登録
            if (status.InReplyToStatusId != 0)
            {
                Get(status.InReplyToStatusId, true).RegisterInReplyToThis(status.Id);
            }

            UserStorage.Register(status.User);
            var registered = RegisterCore(status);

            if (TwitterHelper.IsMentionOfMe(status))
            {
                EventStorage.OnMention(registered);
            }
            return registered;
        }

        /// <summary>
        /// ダイレクトメッセージの追加に際しての処理
        /// </summary>
        private static TweetViewModel RegisterDirectMessage(TwitterDirectMessage dmsg)
        {
            UserStorage.Register(dmsg.Sender);
            UserStorage.Register(dmsg.Recipient);
            var vm = RegisterCore(dmsg);
            EventStorage.OnDirectMessage(vm);
            return vm;
        }

        private static object __regCoreLock__ = new object();

        /// <summary>
        /// ステータスベースの登録処理
        /// </summary>
        private static TweetViewModel RegisterCore(TwitterStatusBase statusBase)
        {
            TweetViewModel viewModel;
            if (empties.ContainsKey(statusBase.Id))
            {
                // 既にViewModelが生成済み
                viewModel = empties[statusBase.Id];
                if (!viewModel.IsStatusInfoContains)
                    viewModel.SetStatus(statusBase);
                empties.Remove(statusBase.Id);
            }
            else
            {
                // 全く初めて触れるステータス
                viewModel = new TweetViewModel(statusBase);
            }
            if (Setting.Instance.TimelineFilteringProperty.MuteFilterCluster == null ||
                !Setting.Instance.TimelineFilteringProperty.MuteFilterCluster.Filter(statusBase))
            {
                if (Setting.Instance.TimelineFilteringProperty.MuteBlockedUsers)
                {
                    // 何か一つのBlockにでも引っかかったらダメ
                    if (AccountStorage.Accounts.Any(a => a.IsBlocking(statusBase.User.NumericId)))
                        return viewModel;
                }
                // プリプロセッシング
                PreProcess(statusBase);
                lock (__regCoreLock__)
                {
                    if (!deleteReserveds.Contains(statusBase.Id))
                    {
                        if (dictionary.ContainsKey(statusBase.Id))
                            return viewModel; // すでにKrile内に存在する
                        else
                            dictionary.AddOrUpdate(statusBase.Id, viewModel);
                    }
                }
                if (!deleteReserveds.Contains(statusBase.Id))
                    Task.Factory.StartNew(() => RaiseStatusAdded(viewModel));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("*** trash: " + statusBase.ToString());
            }
            return viewModel;
        }

        /// <summary>
        /// ステータスのプリプロセッシング
        /// </summary>
        private static void PreProcess(TwitterStatusBase status)
        {
            try
            {
                if (status.Entities != null)
                {
                    // extracting t.co
                    var urls = status.Entities.GetChildNode("urls");
                    if (urls != null)
                    {
                        // indicesの始まりが遅い順に置換していく
                        urls.GetChildNodes("item")
                            .Where(i => i.GetChildNode("indices") != null)
                            .Where(i => i.GetChildNode("indices").GetChildValues("item") != null)
                            .OrderByDescending(i => i.GetChildNode("indices").GetChildValues("item")
                                .Select(s => int.Parse(s.Value)).First())
                            .ForEach(i =>
                        {
                            var expand = i.GetChildValue("expanded_url").Value;
                            if (String.IsNullOrWhiteSpace(expand))
                                expand = i.GetChildValue("url").Value;
                            if (!String.IsNullOrWhiteSpace(expand))
                            {
                                var indices = i.GetChildNode("indices").GetChildValues("item")
                                    .Select(v => int.Parse(v.Value)).OrderBy(v => v).ToArray();
                                if (indices.Length == 2)
                                {
                                    status.Text = status.Text.Substring(0, indices[0]) +
                                        expand + status.Text.Substring(indices[1]);
                                }
                            }
                        });
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// ツイートの削除
        /// </summary>
        /// <param name="id">削除するツイートID</param>
        public static void Remove(long id)
        {
            // 削除する
            TweetViewModel remobj = null;
            deleteReserveds.Add(id);
            lock (__regCoreLock__)
            {
                empties.Remove(id);
                if (dictionary.ContainsKey(id))
                {
                    remobj = dictionary[id];
                    dictionary.Remove(id);
                    Task.Factory.StartNew(() => RaiseStatusRemoved(remobj));
                }
            }
            if (remobj != null)
            {
                // リツイート判定
                var status = remobj.Status as TwitterStatus;
                if (status != null && status.RetweetedOriginal != null)
                {
                    var ros = TweetStorage.Get(status.RetweetedOriginal.Id);
                    if (ros != null)
                        ros.RemoveRetweeted(UserStorage.Get(status.User));
                }
            }
        }

        /// <summary>
        /// ツイートの内部状態が変化したことを通知します。<para />
        /// (例えば、ふぁぼられたりRTされたり返信貰ったりなど。)
        /// </summary>
        public static void NotifyTweetStateChanged(TweetViewModel tweet)
        {
            Task.Factory.StartNew(() => RaiseStatusStateChanged(tweet));
        }

        #region TweetStorageChangedイベント

        public static event EventHandler<TweetStorageChangedEventArgs> TweetStorageChanged;
        private static Notificator<TweetStorageChangedEventArgs> _TweetStorageChangedEvent;
        public static Notificator<TweetStorageChangedEventArgs> TweetStorageChangedEvent
        {
            get
            {
                if (_TweetStorageChangedEvent == null)
                    _TweetStorageChangedEvent = new Notificator<TweetStorageChangedEventArgs>();
                return _TweetStorageChangedEvent;
            }
            set { _TweetStorageChangedEvent = value; }
        }

        private static void OnTweetStorageChanged(TweetStorageChangedEventArgs e)
        {
            var threadSafeHandler = Interlocked.CompareExchange(ref TweetStorageChanged, null, null);
            if (threadSafeHandler != null) threadSafeHandler(null, e);
            TweetStorageChangedEvent.Raise(e);
        }

        #endregion

        static void RaiseStatusAdded(TweetViewModel added)
        {
            // Mention通知設定がないか、
            // 自分へのMentionでない場合にのみRegisterする
            if (!Setting.Instance.NotificationProperty.NotifyMention ||
                !TwitterHelper.IsMentionOfMe(added.Status))
                NotificationCore.RegisterNotify(added);
            OnTweetStorageChanged(new TweetStorageChangedEventArgs(TweetActionKind.Added, added));
            NotificationCore.DispatchNotify(added);
        }

        static void RaiseStatusRemoved(TweetViewModel removed)
        {
            OnTweetStorageChanged(new TweetStorageChangedEventArgs(TweetActionKind.Removed, removed));
        }

        static void RaiseStatusStateChanged(TweetViewModel changed)
        {
            OnTweetStorageChanged(new TweetStorageChangedEventArgs(TweetActionKind.Changed, changed));
        }

        static void RaiseRefreshTweets()
        {
            OnTweetStorageChanged(new TweetStorageChangedEventArgs(TweetActionKind.Refresh));
        }

        internal static void UpdateMute()
        {
            if (Setting.Instance.TimelineFilteringProperty.MuteFilterCluster == null) return;
            var ng = GetAll(t => Setting.Instance.TimelineFilteringProperty.MuteFilterCluster.Filter(t.Status)).ToArray();
            foreach (var t in ng)
            {
                if (!AccountStorage.Contains(t.Status.User.ScreenName))
                    Remove(t.bindingId);
            }
        }
    }
    

    public class TweetStorageChangedEventArgs : EventArgs
    {
        public TweetStorageChangedEventArgs(TweetActionKind act, TweetViewModel added = null)
        {
            this.ActionKind = act;
            this.Tweet = added;
        }

        public TweetViewModel Tweet { get; set; }

        public TweetActionKind ActionKind { get; set; }
    }

    public enum TweetActionKind
    {
        /// <summary>
        /// ツイートが追加された
        /// </summary>
        Added,
        /// <summary>
        /// ツイートが削除された
        /// </summary>
        Removed,
        /// <summary>
        /// ツイートの固有情報が変更された
        /// </summary>
        Changed,
        /// <summary>
        /// ストレージ全体に変更が入った
        /// </summary>
        Refresh,
    }
}
