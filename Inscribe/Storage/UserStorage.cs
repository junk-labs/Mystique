using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dulcet.Twitter;
using Dulcet.Twitter.Rest;
using Inscribe.Util;
using Inscribe.ViewModels.PartBlocks.MainBlock;

namespace Inscribe.Storage
{
    public static class UserStorage
    {
        private static ReaderWriterLockWrap lockWrap;
        private static Dictionary<long, UserViewModel> dictionary;
        private static object semaphoreAccessLocker = new object();
        private static Dictionary<string, ManualResetEvent> strSemaphores;
        private static Dictionary<long, ManualResetEvent> numSemaphores;

        static UserStorage()
        {
            lockWrap = new ReaderWriterLockWrap();
            dictionary = new Dictionary<long, UserViewModel>();
            strSemaphores = new Dictionary<string,ManualResetEvent>();
            numSemaphores = new Dictionary<long,ManualResetEvent>();
        }

        /// <summary>
        /// キャッシュにユーザー情報が存在していたら、すぐに返します。<para />
        /// キャッシュに存在しない場合はNULLを返します。
        /// </summary>
        [Obsolete("for performance reason, should use other overload.")]
        public static UserViewModel Lookup(string userScreenName)
        {
            if (userScreenName == null)
                throw new ArgumentNullException("userScreenName");
            using (lockWrap.GetReaderLock())
            {
                return dictionary
                    .Where(u => u.Value.TwitterUser.ScreenName == userScreenName)
                    .Select(u => u.Value)
                    .FirstOrDefault();
            }
        }

        public static UserViewModel Lookup(long id)
        {
            UserViewModel ret;
            using (lockWrap.GetReaderLock())
            {
                if (dictionary.TryGetValue(id, out ret))
                    return ret;
                else
                    return null;
            }
        }

        /// <summary>
        /// User ViewModelを生成して、キャッシュに追加します。
        /// </summary>
        public static void Register(TwitterUser user)
        {
            Task.Factory.StartNew(() => Get(user));
        }

        /// <summary>
        /// User ViewModelを取得します。<para />
        /// 内部キャッシュを更新します。
        /// </summary>
        /// <param name="user">ユーザー情報(nullは指定できません)</param>
        public static UserViewModel Get(TwitterUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            var newvm = new UserViewModel(user);
            using (lockWrap.GetWriterLock())
            {
                if (dictionary.ContainsKey(user.NumericId))
                    dictionary[user.NumericId] = newvm;
                else
                    dictionary.Add(user.NumericId, newvm);
            }
            return newvm;
        }

        /// <summary>
        /// User ViewModelを取得します。<para />
        /// nullを返すことがあります。
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="useCache">内部キャッシュが可能であれば使用する</param>
        /// <returns></returns>
        public static UserViewModel Get(long userId, bool useCache = true)
        {
            UserViewModel ret = null;
            if (useCache)
            {
                ret = Lookup(userId);
                if (ret != null)
                    return ret;
            }
            return DownloadUser(userId);
        }

        /// <summary>
        /// User ViewModelを取得します。<para />
        /// nullを返すことがあります。
        /// </summary>
        /// <param name="userScreenName">ユーザースクリーン名</param>
        /// <param name="useCache">内部キャッシュが可能であれば使用する</param>
        /// <returns></returns>
        [Obsolete("for performance reason, should use other overload.")]
        public static UserViewModel Get(string userScreenName, bool useCache = true)
        {
            if (String.IsNullOrEmpty(userScreenName))
                throw new ArgumentNullException("userScreenName", "userScreenNameがNullであるか、または空白です。");
            UserViewModel ret = null;
            if (useCache)
            {
                ret = Lookup(userScreenName);
                if (ret != null)
                    return ret;
            }
            return DownloadUser(userScreenName);
        }

        /// <summary>
        /// ユーザー情報をダウンロードし、キャッシュを更新します。
        /// </summary>
        private static UserViewModel DownloadUser(long userId)
        {
            ManualResetEvent mre;
            lock (semaphoreAccessLocker)
            {
                if (!numSemaphores.TryGetValue(userId, out mre))
                {
                    numSemaphores.Add(userId, new ManualResetEvent(false));
                }
            }
            if (mre != null)
            {
                mre.WaitOne();
                return Get(userId);
            }
            try
            {
                var acc = AccountStorage.GetRandom(ai => true, true);
                if (acc != null)
                {
                    try
                    {
                        var ud = acc.GetUser(userId);
                        if (ud != null)
                        {
                            var uvm = new UserViewModel(ud);
                            using (lockWrap.GetWriterLock())
                            {
                                if (dictionary.ContainsKey(ud.NumericId))
                                    dictionary[ud.NumericId] = uvm;
                                else
                                    dictionary.Add(ud.NumericId, uvm);
                            }
                            return uvm;
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionStorage.Register(e, ExceptionCategory.TwitterError, "ユーザー情報の受信に失敗しました。(ユーザー #" + userId + " を アカウント @" + acc.ScreenName + " で受信しようとしました。)");
                    }
                    return null;
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                lock (semaphoreAccessLocker)
                {
                    if (numSemaphores.ContainsKey(userId))
                    {
                        numSemaphores[userId].Set();
                        numSemaphores.Remove(userId);
                    }
                }
            }
        }

        /// <summary>
        /// ユーザー情報をダウンロードし、キャッシュを更新します。
        /// </summary>
        [Obsolete("for performance reason, should use other overload.")]
        private static UserViewModel DownloadUser(string userScreenName)
        {
            ManualResetEvent mre;
            lock (semaphoreAccessLocker)
            {
                if (!strSemaphores.TryGetValue(userScreenName, out mre))
                {
                    strSemaphores.Add(userScreenName, new ManualResetEvent(false));
                }
            }
            if (mre != null)
            {
                mre.WaitOne();
                return Get(userScreenName);
            }
            try
            {
                var acc = AccountStorage.GetRandom(ai => true, true);
                if (acc != null)
                {
                    try
                    {
                        var ud = acc.GetUserByScreenName(userScreenName);
                        if (ud != null)
                        {
                            var uvm = new UserViewModel(ud);
                            using (lockWrap.GetWriterLock())
                            {
                                if (dictionary.ContainsKey(ud.NumericId))
                                    dictionary[ud.NumericId] = uvm;
                                else
                                    dictionary.Add(ud.NumericId, uvm);
                            }
                            return uvm;
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionStorage.Register(e, ExceptionCategory.TwitterError, "ユーザー情報の受信に失敗しました。(ユーザー @" + userScreenName + " を アカウント @" + acc.ScreenName + " で受信しようとしました。)");
                    }
                    return null;
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                lock (semaphoreAccessLocker)
                {
                    if (strSemaphores.ContainsKey(userScreenName))
                    {
                        strSemaphores[userScreenName].Set();
                        strSemaphores.Remove(userScreenName);
                    }
                }
            }
        }

        /// <summary>
        /// ストレージに格納されているすべてのユーザーを取得します。
        /// </summary>
        /// <returns></returns>
        public static UserViewModel[] GetAll()
        {
            return dictionary.Values.ToArray();
        }
    }
}
