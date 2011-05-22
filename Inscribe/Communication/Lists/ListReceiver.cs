using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.Storage;
using Dulcet.Twitter.Rest;
namespace Inscribe.Communication.Lists
{
    public class ListReceiver
    {
        public ListReceiver BeginReceive(string list)
        {
            var delimiter = list.IndexOf('/');
            try
            {
                // @user/list から user と list を切り出す
                return new ListReceiver(list.Substring(1, delimiter - 1), list.Substring(delimiter + 1));
            }
            catch
            {
                throw new ArgumentException("リストの指定が不正です:" + list);
            }
        }

        private readonly string listUser;

        private readonly string listName;

        public ListReceiver(string listFullName)
        {
            var delimiter = listFullName.IndexOf('/');
            try
            {
                this.listUser = listFullName.Substring(1, delimiter - 1);
                this.listName = listFullName.Substring(delimiter + 1);
            }
            catch
            {
                throw new ArgumentException("リストの指定が不正です:" + listFullName);
            }
        }

        public ListReceiver(string listUser, string listName)
        {
            this.listUser = listUser;
            this.listName = listName;
        }

        /// <summary>
        /// リストの新着ステータスを確認します。
        /// </summary>
        public void ReceiveList()
        {
            var acc = AccountStorage.GetRandom(a => a.IsFollowingList(listUser, listName), true);
            if (acc == null) return;
            var statuses = APIHelper.ExecApi(() => acc.GetListStatuses(listUser, listName));
            if (statuses == null) return;
            statuses.ForEach(s => TweetStorage.Register(s));
        }
    }
}
