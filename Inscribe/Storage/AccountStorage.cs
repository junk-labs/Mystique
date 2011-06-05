using System;
using System.Collections.Generic;
using System.Linq;
using Inscribe.Data;
using Inscribe.Model;
using Livet;
using System.Threading;

namespace Inscribe.Storage
{
    public static class AccountStorage
    {
        static SafeList<AccountInfo> accounts = new SafeList<AccountInfo>();

        /// <summary>
        /// 登録されているすべてのアカウント情報
        /// </summary>
        public static IEnumerable<AccountInfo> Accounts
        {
            get { return accounts; }
        }

        /// <summary>
        /// アカウントを登録します。
        /// </summary>
        /// <param name="accountInfo">登録するアカウント情報</param>
        public static void RegisterAccount(AccountInfo accountInfo)
        {
            accounts.Add(accountInfo);
            OnAccountsChanged(EventArgs.Empty);
        }

        /// <summary>
        /// 既に存在するアカウント情報を更新します。
        /// </summary>
        /// <param name="prevId">旧アカウント情報</param>
        /// <param name="newinfo">新しいアカウント情報</param>
        public static void Update(string prevId, AccountInfo newinfo)
        {
            accounts.LockOperate(() =>
            {
                var prev = Get(prevId);
                if(prev == null)
                    throw new ArgumentException("アカウント @" + prevId + " は存在しません。");
                var idx = accounts.IndexOf(prev);
                if (idx < 0) throw new ArgumentException("旧アカウント情報が見つかりません。");
                accounts[idx] = newinfo;
            });
            OnAccountsChanged(EventArgs.Empty);
        }

        /// <summary>
        /// アカウント情報を削除します。
        /// </summary>
        /// <param name="screenName">削除するアカウント情報のスクリーン名</param>
        public static bool DeleteAccount(string screenName)
        {
            var del = Get(screenName);
            if (del != null)
            {
                accounts.Remove(del);
                OnAccountsChanged(EventArgs.Empty);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// アカウントをランダムに取得します。
        /// </summary>
        /// <param name="predicate">アカウントが満たすべき条件</param>
        /// <param name="weakPredicate">弱条件とする(条件を満たさないとき、ランダムでアカウントを返す</param>
        public static AccountInfo GetRandom(Func<AccountInfo, bool> predicate = null, bool weakPredicate = false)
        {
            AccountInfo retai = null;
            if (predicate != null)
            {
                retai = accounts.Shuffle().FirstOrDefault(predicate);
                if (retai != null || !weakPredicate)
                    return retai;
            }
            return accounts.Shuffle().FirstOrDefault();
        }

        /// <summary>
        /// スクリーン名からユーザーアカウント情報を取得します。
        /// </summary>
        /// <param name="screenName">スクリーン名(ignorecase一致)</param>
        /// <returns>アカウント情報</returns>
        public static AccountInfo Get(string screenName)
        {
            return accounts.FirstOrDefault(a => a.ScreenName.Equals(screenName, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// アカウントが含まれているか確認します。
        /// </summary>
        public static bool Contains(string screenName)
        {
            return Get(screenName) != null;
        }

        public enum MoveDirection
        {
            Up,
            Down,
        }

        /// <summary>
        /// アカウントの順序を変更します。
        /// </summary>
        public static void MoveAccount(string id, MoveDirection direction)
        {
            accounts.LockOperate(() =>
            {
                var info = Get(id);
                if (info == null)
                    throw new ArgumentException("アカウント @" + id + " は存在しません。");
                var idx = accounts.IndexOf(info);
                if (idx < 0)
                    throw new InvalidOperationException();
                switch (direction)
                {
                    case MoveDirection.Up:
                        if (idx > 0)
                        {
                            accounts.RemoveAt(idx);
                            accounts.Insert(idx - 1, info);
                        }
                        break;
                    case MoveDirection.Down:
                        if (idx < accounts.Count - 1)
                        {
                            accounts.RemoveAt(idx);
                            accounts.Insert(idx + 1, info);
                        }
                        break;
                    default:
                        throw new ArgumentException("移動方向指定がちゃんちゃらおかしい :" + direction.ToString());
                }
            });
        }


        #region AccountsChangedイベント

        public static event EventHandler<EventArgs> AccountsChanged;
        private static Notificator<EventArgs> _AccountsChangedEvent;
        public static Notificator<EventArgs> AccountsChangedEvent
        {
            get
            {
                if (_AccountsChangedEvent == null) _AccountsChangedEvent = new Notificator<EventArgs>();
                return _AccountsChangedEvent;
            }
            set { _AccountsChangedEvent = value; }
        }

        private static void OnAccountsChanged(EventArgs e)
        {
            var threadSafeHandler = Interlocked.CompareExchange(ref AccountsChanged, null, null);
            if (threadSafeHandler != null) threadSafeHandler(null, e);
            AccountsChangedEvent.Raise(e);
        }

        #endregion
    }
}
