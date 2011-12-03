using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Inscribe.Util;
using Inscribe.Storage;
using System.Linq;
using Inscribe.Authentication;

namespace Inscribe.Communication.CruiseControl.Lists
{
    /// <summary>
    /// リスト受信のマネージメントクラス
    /// </summary>
    public static class ListReceiverManager
    {
        private static ReaderWriterLockWrap rvLocker = new ReaderWriterLockWrap();

        private static Dictionary<string, ListReceiveTask> receivers = new Dictionary<string, ListReceiveTask>();

        private static object rcLocker = new object();

        private static Dictionary<string, int> referenceCount = new Dictionary<string, int>();

        static ListReceiverManager()
        {
            AutoCruiseSchedulerManager.SchedulerUpdated += new EventHandler<EventArgs>(AutoCruiseSchedulerManager_SchedulerUpdated);
        }

        private static List<Tuple<string, string>> waitings = new List<Tuple<string, string>>();

        static void AutoCruiseSchedulerManager_SchedulerUpdated(object sender, EventArgs e)
        {
            lock (rcLocker)
            {
                receivers.Select(l => l.Value)
                    .Where(l => AutoCruiseSchedulerManager.GetScheduler(l.AccountInfo) == null)
                    .ToArray()
                    .ForEach(l =>
                    {
                        RemoveReceive(l.ListUserScreenName, l.ListName);
                        RegisterReceive(l.ListUserScreenName, l.ListName);
                    });
            }
            if (waitings.Count > 0)
            {
                var wa = waitings.ToArray();
                waitings.Clear();
                wa.ForEach(t => RegisterReceive(t.Item1, t.Item2));
            }
        }


        public static void RegisterReceive(string listUser, string listName)
        {
            System.Diagnostics.Debug.WriteLine("** LIST LISTEN START:@" + listUser + "/" + listName);
            listName = NormalizeListName(listName);
            var fullname = BuildListName(listUser, listName);
            lock (rcLocker)
            {
                if (referenceCount.ContainsKey(fullname))
                {
                    referenceCount[fullname]++;
                }
                else
                {
                    var target = AccountStorage.Get(listUser);
                    if (target == null)
                        target = AccountStorage.GetRandom(ai => ai.IsFollowingList(listUser, listName), true);
                    var tscheduler = target != null ? AutoCruiseSchedulerManager.GetScheduler(target) : null;
                    if (tscheduler == null)
                    {
                        // スケジューラがまだない
                        // スケジューラが更新されるまで待つ
                        waitings.Add(new Tuple<string, string>(listUser, listName));
                        return;
                    }
                    var task = new ListReceiveTask(target, listUser, listName);
                    receivers.Add(fullname, task);
                    tscheduler.AddSchedule(task);
                    Task.Factory.StartNew(() => ListStorage.Get(listUser, listName));
                    referenceCount.Add(fullname, 1);
                }
            }
        }

        public static void RemoveReceive(string listUser, string listName)
        {
            System.Diagnostics.Debug.WriteLine("** LIST LISTEN END:@" + listUser + "/" + listName);
            var list = BuildListName(listUser, listName);
            lock(rcLocker)
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