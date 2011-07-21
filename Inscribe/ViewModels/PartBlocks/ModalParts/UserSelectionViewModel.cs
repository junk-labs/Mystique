using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet;
using Inscribe.ViewModels.Common;
using Inscribe.Model;

namespace Inscribe.ViewModels.PartBlocks.ModalParts
{
    public class UserSelectionViewModel : ViewModel
    {
        public UserSelectionViewModel()
        {
            this._userSelectorViewModel = new UserSelectorViewModel();
        }

        public event Action Finished;
        private void OnFinished()
        {
            var handler = Finished;
            if (handler != null)
                handler();
        }


        private Action<IEnumerable<AccountInfo>> curInteract = null;

        public void BeginInteraction(Action<IEnumerable<AccountInfo>> interact)
        {
            curInteract = interact;
        }

        private UserSelectorViewModel _userSelectorViewModel;

        public UserSelectorViewModel UserSelectorViewModel
        {
            get { return _userSelectorViewModel; }
        }
    }
}
