using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dulcet.Twitter.Rest;
using Inscribe.Model;
using Inscribe.Storage;
using Inscribe.Threading;

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
            Task.Factory.StartNew(() => ApiHelper.ExecApi(() => UserStorage.Register(info.GetUserByScreenName(info.ScreenName))));
            // フォロー/フォロワー/ブロックの受信
            Task.Factory.StartNew(() => ApiHelper.ExecApi(() => info.GetFriendIds(screenName: info.ScreenName).ForEach(i => info.RegisterFollowing(i))));
            Task.Factory.StartNew(() => ApiHelper.ExecApi(() => info.GetFollowerIds(screenName: info.ScreenName).ForEach(i => info.RegisterFollower(i))));
            Task.Factory.StartNew(() => ApiHelper.ExecApi(() => info.GetBlockingIds().ForEach(i => info.RegisterBlocking(i))));
        }
    }
}
