using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.Model;
using Inscribe.Configuration;
using Inscribe.Storage;
using Dulcet.Twitter.Rest;
using System.Threading;
using Inscribe.Threading;

namespace Inscribe.Communication.MainTimeline
{
    public static class ReceiverManager
    {
        static Dictionary<AccountInfo, Receiver> receivers = new Dictionary<AccountInfo, Receiver>();

        static Timer userInfoUpdateTimer = null;

        public static void RunUserInfoTimer()
        {
            userInfoUpdateTimer = new Timer(_ => receivers.Keys.ForEach(ReceiveInidividualInfo)
            , null, TwitterDefine.UserInformationRefreshPeriod, TwitterDefine.UserInformationRefreshPeriod);
            ThreadHelper.Halt += () => userInfoUpdateTimer.Dispose();
        }

        /// <summary>
        /// 明示的な変更があった場合に接続を更新します。
        /// </summary>
        public static void RefreshReceivers()
        {
            var exists = receivers.Keys;
            var infos = AccountStorage.Accounts;
            var addeds = infos.Except(exists);
            var removes = exists.Except(infos);
            var keeps = exists.Except(removes);
            foreach (var i in addeds)
            {
                // 新規接続
                var recv = new Receiver(i);
                ReceiveInidividualInfo(i);
                recv.UpdateConnection();
            }
        }

        /// <summary>
        /// 指定アカウントの依存情報を受信します。
        /// </summary>
        public static void ReceiveInidividualInfo(AccountInfo info)
        {
            // アカウント情報の受信
            UserStorage.Register(info.GetUserByScreenName(info.ScreenName));
            // フォロー/フォロワー/ブロックの受信
            info.GetFriendsAll().Select(u => UserStorage.Get(u)).ForEach(u => info.RegisterFollowing(u));
            info.GetFollowersAll().Select(u => UserStorage.Get(u)).ForEach(u => info.RegisterFollower(u));
            info.GetBlockingUsersAll().Select(u => UserStorage.Get(u)).ForEach(u => info.RegisterBlocking(u));
        }

        /// <summary>
        /// 特定のアカウント情報の接続を更新します。<para />
        /// 接続が無い場合は失敗します。
        /// </summary>
        public static void RefreshReceiver(AccountInfo accountInfo)
        {
            if (!receivers.ContainsKey(accountInfo))
                throw new ArgumentException("アカウント @" + accountInfo.ScreenName + " は接続登録されていません。");

        }
    }
}
