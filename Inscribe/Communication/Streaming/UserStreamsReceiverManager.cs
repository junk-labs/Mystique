using System;
using System.Collections.Generic;
using System.Linq;
using Inscribe.Model;
using Inscribe.Storage;

namespace Inscribe.Communication.Streaming
{
    public static class UserStreamsReceiverManager
    {
        static Dictionary<AccountInfo, UserStreamsReceiver> receivers = new Dictionary<AccountInfo, UserStreamsReceiver>();

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
                var recv = new UserStreamsReceiver(i);
                UserInformationManager.ReceiveInidividualInfo(i);
                recv.UpdateConnection();
            }
        }

        /// <summary>
        /// 特定のアカウント情報の接続を更新します。<para />
        /// 接続が無い場合は失敗します。
        /// </summary>
        public static void RefreshReceiver(AccountInfo accountInfo)
        {
            if (!receivers.ContainsKey(accountInfo))
                throw new ArgumentException("アカウント @" + accountInfo.ScreenName + " は接続登録されていません。");
            receivers[accountInfo].UpdateConnection();
        }
    }
}
