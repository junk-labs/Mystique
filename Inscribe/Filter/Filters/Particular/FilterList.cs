using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.Filter.Core;
using Inscribe.Storage;
using System.Threading;
using Dulcet.Twitter;
using System.Threading.Tasks;

namespace Inscribe.Filter.Filters.Particular
{
    public class FilterList : FilterBase
    {
        private string listUser;

        [GuiVisible("ユーザーID")]
        public string ListUser
        {
            get { return listUser; }
            set
            {
                listUser = value;
                initCheckFlag = 0;
            }
        }
        private string listName;

        [GuiVisible("リスト名")]
        public string ListName
        {
            get { return listName; }
            set
            {
                listName = value;
                initCheckFlag = 0;
            }
        }
        private FilterList() { }

        public FilterList(string listUser, string listName)
        {
            this.listUser = listUser;
            this.listName = listName;
        }

        private int initCheckFlag = 0;

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            bool init = Interlocked.Exchange(ref initCheckFlag,1) == 0;
            if (ListStorage.IsListMemberCached(this.listUser, this.listName))
            {
                var ids = 
                 ListStorage.GetListMembers(this.listUser, this.listName)
                    .Select(u => u.TwitterUser.ScreenName).ToArray();
                return ids.Contains(status.User.ScreenName) &&
                    (!(status is TwitterStatus) ||
                    String.IsNullOrEmpty(((TwitterStatus)status).InReplyToUserScreenName) ||
                    ids.Contains(((TwitterStatus)status).InReplyToUserScreenName));
            }
            else
            {
                if (init)
                {
                    Task.Factory.StartNew(() =>
                    {
                        ListStorage.GetListMembers(this.listUser, this.listName).ToArray();
                        this.RaiseRequireReaccept();
                    });
                }
                return false;
            }
        }

        public override string Identifier
        {
            get { return "list"; }
        }

        public override IEnumerable<object> GetArgumentsForQueryify()
        {
            yield return listUser;
            yield return listName;
        }

        public override string Description
        {
            get { return "指定したリストのタイムラインを抽出します。"; }
        }

        public override string FilterStateString
        {
            get { return "リスト @" + this.listUser + "/" + this.listName + " のタイムラインを抽出します。"; }
        }
    }
}
