﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.ViewModels;
using Inscribe.Data;
using Dulcet.Twitter;
using System.Threading.Tasks;
using Inscribe.Configuration;
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
        static SafeDictionary<long, TweetViewModel> dictionary = new SafeDictionary<long, TweetViewModel>();

        /// <summary>
        /// 仮登録ステータスディクショナリ
        /// </summary>
        static SafeDictionary<long, TweetViewModel> empties = new SafeDictionary<long, TweetViewModel>();

        /// <summary>
        /// 削除予約されたツイートIDリスト
        /// </summary>
        static SafeLinkedList<long> deleteReserveds = new SafeLinkedList<long>();

        /// <summary>
        /// ツイートストレージの作業用スレッドディスパッチャ
        /// </summary>
        static QueueTaskDispatcher operationDispatcher = new QueueTaskDispatcher(1);

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
                var nvm = new TweetViewModel();
                empties.Add(id, nvm);
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
                return dictionary.ValueToArray();
            else
                return dictionary.ValueToArrayParallel(predicate);
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
                // たぶん empty にはならないけど、何らかの理由でemptyになった時に備えて true を置いておく
                Get(status.RetweetedOriginal.Id, true).RegisterRetweeted(UserStorage.Get(status.User));
                // TODO: イベント通知
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
                    vm.SetStatus(statusBase);
                    empties.Remove(statusBase.Id);
                    dictionary.Add(statusBase.Id, vm);
                    Task.Factory.StartNew(() => RaiseStatusAdded(vm));
                }
                else
                {
                    // 全く初めて触れるステータス
                    var newViewModel = new TweetViewModel(statusBase);
                    dictionary.Add(statusBase.Id, newViewModel);
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
            deleteReserveds.AddLast(id);
            operationDispatcher.Enqueue(() =>
            {
                // 削除する
                empties.Remove(id);
                dictionary.Remove(id);
                Task.Factory.StartNew(() => RaiseStatusRemoved(id));
            });
        }

        #region Notifications

        private static readonly Notificator<TweetStorageChangedEventArgs> _notificator = new Notificator<TweetStorageChangedEventArgs>();
        public static Notificator<TweetStorageChangedEventArgs> Notificator { get { return _notificator; } }

        static void RaiseStatusAdded(TweetViewModel added)
        {
            Notificator.Raise(new TweetStorageChangedEventArgs(TweetActionKind.Added, added));
        }

        static void RaiseStatusRemoved(long removedId)
        {
            Notificator.Raise(new TweetStorageChangedEventArgs(TweetActionKind.Removed, id: removedId));
        }

        static void RaiseRefreshTweets()
        {
            Notificator.Raise(new TweetStorageChangedEventArgs(TweetActionKind.Refresh));
        }

        #endregion
    }

    public class TweetStorageChangedEventArgs : EventArgs
    {
        public TweetStorageChangedEventArgs(TweetActionKind act, TweetViewModel added = null, long id = 0)
        {
            this.ActionKind = act;
            this.Added = added;
            this.Id = id;
        }

        public TweetViewModel Added { get; set; }

        public long Id { get; set; }

        public TweetActionKind ActionKind { get; set; }
    }

    public enum TweetActionKind
    {
        Added,
        Removed,
        Refresh,
    }
}