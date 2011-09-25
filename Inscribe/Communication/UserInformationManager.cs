using System;
using System.Linq;
using System.Threading;
using Dulcet.Twitter.Rest;
using Inscribe.Authentication;
using Inscribe.Common;
using Inscribe.Storage;

namespace Inscribe.Communication
{
    public static class UserInformationManager
    {
        static Timer userInfoUpdateTimer = null;

        public static void RunUserInfoTimer()
        {
            userInfoUpdateTimer = new Timer(_ => AccountStorage.Accounts.ForEach(ReceiveInidividualInfo)
                , null, TwitterDefine.UserInformationRefreshPeriod, TwitterDefine.UserInformationRefreshPeriod);
            ThreadHelper.Halt += () => userInfoUpdateTimer.Dispose();
        }

        /// <summary>
        /// 指定アカウントの依存情報を受信します。
        /// </summary>
        public static void ReceiveInidividualInfo(AccountInfo info)
        {
            // アカウント情報の受信
            SafeExec(() => UserStorage.Register(info.GetUserByScreenName(info.ScreenName)));
            // フォロー/フォロワー/ブロックの受信
            SafeExec(() => info.GetFriendIds(screenName: info.ScreenName).ForEach(i => info.RegisterFollowing(i)));
            SafeExec(() => info.GetFollowerIds(screenName: info.ScreenName).ForEach(i => info.RegisterFollower(i)));
            SafeExec(() => info.GetBlockingIds().ForEach(i => info.RegisterBlocking(i)));
        }

        private static void SafeExec(Action action)
        {
            try
            {
                ApiHelper.ExecApi(action);
            }
            catch (Exception e)
            {
                ExceptionStorage.Register(e, ExceptionCategory.TwitterError, "アカウント情報の受信中にエラーが発生しました", () => SafeExec(action));
            }
        }
    }
}
