using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet;
using Dulcet.Twitter;

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


    }
}
