using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet;
using Mystique.ViewModels.Dialogs.SettingSub;
using Livet.Command;
using Livet.Messaging.Window;

namespace Mystique.ViewModels.Dialogs
{
    public class SettingViewModel : ViewModel
    {
        public SettingViewModel() { }

        private AccountConfigViewModel _accountConfigViewModel = new AccountConfigViewModel();

        public AccountConfigViewModel AccountConfigViewModel
        {
            get { return this._accountConfigViewModel; }
        }

        #region ApplyCommand
        DelegateCommand _ApplyCommand;

        public DelegateCommand ApplyCommand
        {
            get
            {
                if (_ApplyCommand == null)
                    _ApplyCommand = new DelegateCommand(Apply);
                return _ApplyCommand;
            }
        }

        private void Apply()
        {
            this.AccountConfigViewModel.WriteBack();
            Close();
        }
        #endregion

        #region CloseCommand
        DelegateCommand _CloseCommand;

        public DelegateCommand CloseCommand
        {
            get
            {
                if (_CloseCommand == null)
                    _CloseCommand = new DelegateCommand(Close);
                return _CloseCommand;
            }
        }

        private void Close()
        {
            Messenger.Raise(new WindowActionMessage("Close", WindowAction.Close));
        }
        #endregion
      
    }
}
