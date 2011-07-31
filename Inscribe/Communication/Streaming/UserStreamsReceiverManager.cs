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
                receivers.Add(i, recv);
                recv.UpdateConnection();
            }
            foreach (var i in removes)
            {
                // 登録削除
                receivers.Remove(i);
            }
            foreach (var i in keeps)
            {
                RefreshReceiver(i);
            }
            RefreshQuery();
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

        #region Streaming-Query Control

        static Dictionary<string, AccountInfo> lookupDictionary = new Dictionary<string, AccountInfo>();

        static Dictionary<string, int> referenceCount = new Dictionary<string, int>();

        public static bool AddQuery(string query)
        {
            if (referenceCount.ContainsKey(query))
            {
                referenceCount[query]++;
                return true;
            }
            else
            {
                if (!StartListenQuery(query))
                    return false;
                referenceCount.Add(query, 1);
                return true;
            }
        }

        private static bool StartListenQuery(string query)
        {
            var info = AccountStorage.Accounts
                .Where(a => a.AccoutProperty.UseUserStreams && receivers.ContainsKey(a))
                .OrderByDescending(a => receivers[a].Queries.Count())
                .FirstOrDefault();
            if (info == null)
                return false;
            lookupDictionary.Add(query, info);
            var recv = receivers[info];
            recv.Queries = recv.Queries.Concat(new[] { query }).ToArray();
            recv.UpdateConnection();
            return true;
        }

        public static void RemoveQuery(string query)
        {
            if (!referenceCount.ContainsKey(query))
                return;
            referenceCount[query]--;
            if (referenceCount[query] == 0)
            {
                referenceCount.Remove(query);
                var recv = receivers[lookupDictionary[query]];
                lookupDictionary.Remove(query);
                recv.Queries = recv.Queries.Except(new[] { query }).ToArray();
                recv.UpdateConnection();
            }
        }

        private static void RefreshQuery()
        {
            lookupDictionary.Keys
                .Where(k => !lookupDictionary[k].AccoutProperty.UseUserStreams)
                .ForEach(k =>
                {
                    lookupDictionary.Remove(k);
                    StartListenQuery(k);
                });
        }

        #endregion
    }
}
