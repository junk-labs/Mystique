using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dulcet.Twitter;
using Inscribe.Threading;
using Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild;
using Livet;

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
        static QueueTaskDispatcher operationDispatcher = new QueueTaskDispatcher(1);

        static TweetStorage()
        {
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
                return dictionary.Values.AsParallel().Where(predicate).ToArray();
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
        public static void Register(TwitterStatusBase statusBase)
        {
            if (dictionary.ContainsKey(statusBase.Id)) return;
            var status = statusBase as TwitterStatus;
            if (status != null)
            {
                RegisterStatus(status);
            }
            else
            {
                var dmsg = statusBase as TwitterDirectMessage;
                if (dmsg != null)
                {
                    RegisterDirectMessage(dmsg);
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
        private static void RegisterStatus(TwitterStatus status)
        {
            if (status.RetweetedOriginal != null)
            {
                // リツイートのオリジナルステータスを登録
                Register(status.RetweetedOriginal);

                // リツイートユーザーに登録
                var vm = Get(status.RetweetedOriginal.Id, true);
                var user = UserStorage.Get(status.User);
                vm.RegisterRetweeted(user);
                if (!vm.IsStatusInfoContains)
                    vm.SetStatus(status.RetweetedOriginal);
                EventStorage.OnRetweeted(vm, user);
            }

            // 返信先の登録
            if (status.InReplyToStatusId != 0)
            {
                Get(status.InReplyToStatusId, true).RegisterInReplyToThis(status.Id);
            }
            UserStorage.Register(status.User);
            RegisterCore(status);
        }

        /// <summary>
        /// ダイレクトメッセージの追加に際しての処理
        /// </summary>
        private static void RegisterDirectMessage(TwitterDirectMessage dmsg)
        {
            UserStorage.Register(dmsg.Sender);
            UserStorage.Register(dmsg.Recipient);
            RegisterCore(dmsg);
        }

        /// <summary>
        /// ステータスベースの登録処理
        /// </summary>
        private static void RegisterCore(TwitterStatusBase statusBase)
        {
            operationDispatcher.Enqueue(() =>
            {
                if (deleteReserveds.Contains(statusBase.Id) || dictionary.ContainsKey(statusBase.Id)) return;
                if (empties.ContainsKey(statusBase.Id))
                {
                    // 既にViewModelが生成済み
                    var vm = empties[statusBase.Id];
                    if (!vm.IsStatusInfoContains)
                        vm.SetStatus(statusBase);
                    empties.Remove(statusBase.Id);
                    dictionary.AddOrUpdate(statusBase.Id, vm);
                    Task.Factory.StartNew(() => RaiseStatusAdded(vm));
                }
                else
                {
                    // 全く初めて触れるステータス
                    var newViewModel = new TweetViewModel(statusBase);
                    dictionary.AddOrUpdate(statusBase.Id, newViewModel);
                    Task.Factory.StartNew(() => RaiseStatusAdded(newViewModel));
                }
            });
        }

        /// <summary>
        /// ツイートの削除
        /// </summary>
        /// <param name="id">削除するツイートID</param>
        public static void Remove(long id)
        {
            // とりあえず削除候補に登録しておく
            deleteReserveds.Add(id);
            operationDispatcher.Enqueue(() =>
            {
                // 削除する
                empties.Remove(id);
                if (dictionary.ContainsKey(id))
                {
                    var remobj = dictionary[id];
                    dictionary.Remove(id);
                    Task.Factory.StartNew(() => RaiseStatusRemoved(remobj));
                }
            });
        }

        /// <summary>
        /// ツイートの内部状態が変化したことを通知します。<para />
        /// (例えば、ふぁぼられたりRTされたり返信貰ったりなど。)
        /// </summary>
        public static void NotifyTweetStateChanged(TweetViewModel tweet)
        {
            Task.Factory.StartNew(() => RaiseStatusStateChanged(tweet));
        }

        #region Notifications

        private static readonly Notificator<TweetStorageChangedEventArgs> _notificator = new Notificator<TweetStorageChangedEventArgs>();
        public static Notificator<TweetStorageChangedEventArgs> Notificator { get { return _notificator; } }

        static void RaiseStatusAdded(TweetViewModel added)
        {
            Notificator.Raise(new TweetStorageChangedEventArgs(TweetActionKind.Added, added));
        }

        static void RaiseStatusRemoved(TweetViewModel removed)
        {
            Notificator.Raise(new TweetStorageChangedEventArgs(TweetActionKind.Removed, removed));
        }

        static void RaiseStatusStateChanged(TweetViewModel changed)
        {
            Notificator.Raise(new TweetStorageChangedEventArgs(TweetActionKind.Changed, changed));
        }

        static void RaiseRefreshTweets()
        {
            Notificator.Raise(new TweetStorageChangedEventArgs(TweetActionKind.Refresh));
        }


        #endregion
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
