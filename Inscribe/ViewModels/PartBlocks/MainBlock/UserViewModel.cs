using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet;
using Dulcet.Twitter;
using Livet.Commands;

namespace Inscribe.ViewModels.PartBlocks.MainBlock
{
    public class UserViewModel : ViewModel
    {
        public TwitterUser TwitterUser { get; private set; }

        public UserViewModel(TwitterUser user)
        {
            if (user == null)
                throw new NullReferenceException("user");
            this.TwitterUser = user;
        }

        public override bool Equals(object obj)
        {
            var tvm = obj as UserViewModel;
            return tvm != null && tvm.TwitterUser.NumericId == this.TwitterUser.NumericId;
        }

        public override int GetHashCode()
        {
            return (int)TwitterUser.NumericId;
        }

    }
}
