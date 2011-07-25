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

        #region OpenUserCommand
        DelegateCommand _OpenUserCommand;

        public DelegateCommand OpenUserCommand
        {
            get
            {
                if (_OpenUserCommand == null)
                    _OpenUserCommand = new DelegateCommand(OpenUser);
                return _OpenUserCommand;
            }
        }

        private void OpenUser()
        {

        }
        #endregion
      
    }
}
