using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Inscribe.Data;
using Inscribe.Model;
using Inscribe.Storage;

namespace Inscribe.Communication.CruiseControl.Lists
{
    /// <summary>
    /// リスト受信のマネージメントクラス
    /// </summary>
    public static class ListReceiverManager
    {
        private static ReaderWriterLockWrap rvLocker = new ReaderWriterLockWrap();

        private static Dictionary<string, ListReceiveTask> receivers = new Dictionary<string, ListReceiveTask>();

        private static ReaderWriterLockWrap rcLocker = new ReaderWriterLockWrap();

        private static Dictionary<string, int> referenceCount = new Dictionary<string, int>();

        static ListReceiverManager()
        {
            // ?
            // AutoCruiseSchedulerManager.SchedulerUpdated += new EventHandler<EventArgs>(AutoCruiseSchedulerManager_SchedulerUpdated);
        }


        public static void RegisterReceive(string listUser, string listName)
        {
            listName = NormalizeListName(listName);
            var fullname = BuildListName(listUser, listName);
            using (rcLocker.GetWriterLock())
            {
                if (referenceCount.ContainsKey(fullname))
                {
                    referenceCount[fullname]++;
                }
                else
                {
                    using (rvLocker.GetWriterLock())
                    {
                        referenceCount.Add(fullname, 1);
                        AccountInfo target;
                        if (AccountStorage.Contains(listUser))
                            target = AccountStorage.Get(listUser);
                        else
                            target = AccountStorage.GetRandom();
                        receivers.Add(fullname, new ListReceiveTask(target, listUser, listName));
                        Task.Factory.StartNew(() => ListStorage.Get(listUser, listName));
                    }
                }
            }
        }

        public static void RemoveReceive(string listUser, string listName)
        {
            var list = BuildListName(listUser, listName);
            using (rcLocker.GetWriterLock())
            {
                if (referenceCount.ContainsKey(list))
                {
                    referenceCount[list]--;
                    if (referenceCount[list] == 0)
                    {
                        referenceCount.Remove(list);
                        using (rvLocker.GetWriterLock())
                        {
                            receivers[list].EndReceive();
                            receivers.Remove(list);
                        }
                    }
                }
            }
        }

        private static string NormalizeListName(string listName)
        {
            return listName.ToLower().Replace("_", "-").Replace(" ", "_");
        }

        private static string BuildListName(string user, string list)
        {
            if (String.IsNullOrEmpty(user))
                throw new ArgumentNullException("user");
            if (String.IsNullOrEmpty("list"))
                throw new ArgumentNullException("list");
            if (user[0] == '@')
                return user.ToLower() + "/" + NormalizeListName(list);
            else
                return "@" + user.ToLower() + "/" + NormalizeListName(list);
        }
    }
}