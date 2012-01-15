using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Inscribe.Authentication;
using Inscribe.Util;
using Livet;

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
            get { return accounts.ToArray(); }
        }

        /// <summary>
        /// アカウントを登録します。
        /// </summary>
        /// <param name="accountInfo">登録するアカウント情報</param>
        public static void RegisterAccount(AccountInfo accountInfo)
        {
            if (accountInfo == null)
                throw new ArgumentNullException("accountInfo");
            accounts.Add(accountInfo);
            OnAccountsChanged(EventArgs.Empty);
            // アカウント情報のキャッシュ
            // Task.Factory.StartNew(() => accountInfo.UserViewModel);
        }

        /// <summary>
        /// 既に存在するアカウント情報を更新します。
        /// </summary>
        /// <param name="prevId">旧アカウント情報</param>
        /// <param name="newinfo">新しいアカウント情報</param>
        public static void Update(AccountInfo prev, AccountInfo newinfo)
        {
            accounts.LockOperate(() =>
            {
                var idx = accounts.IndexOf(prev);
                if (idx < 0) throw new ArgumentException("旧アカウント情報が見つかりません。");
                accounts[idx] = newinfo;
            });
            OnAccountsChanged(EventArgs.Empty);
        }

        public static bool DeleteAccount(AccountInfo info)
        {

            if (info != null && accounts.Contains(info))
            {
                accounts.Remove(info);
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
            if (screenName != null && screenName.StartsWith("@"))
                screenName = screenName.Substring(1);
            return accounts.FirstOrDefault(a => a.ScreenName.Equals(screenName, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// ユーザーIDからユーザーアカウント情報を取得します。
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>アカウント情報</returns>
        public static AccountInfo Get(long userId)
        {
            return accounts.FirstOrDefault(a => a.NumericId == userId);
        }

        /// <summary>
        /// アカウントが含まれているか確認します。
        /// </summary>
        public static bool Contains(string screenName)
        {
            return Get(screenName) != null;
        }

        /// <summary>
        /// アカウントが含まれているか確認します。
        /// </summary>
        public static bool Contains(long id)
        {
            return Get(id) != null;
        }

        public enum MoveDirection
        {
            Up,
            Down,
        }

        /// <summary>
        /// アカウントの順序を変更します。
        /// </summary>
        public static void MoveAccount(AccountInfo info, MoveDirection direction)
        {
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
            OnAccountsChanged(EventArgs.Empty);
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
