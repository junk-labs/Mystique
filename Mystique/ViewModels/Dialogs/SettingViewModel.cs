using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet;
using Mystique.ViewModels.Dialogs.SettingSub;

namespace Mystique.ViewModels.Dialogs
{
    public class SettingViewModel : ViewModel
    {
        public SettingViewModel()
        {
        }

        private AccountConfigViewModel _accountConfigViewModel = new AccountConfigViewModel();

        public AccountConfigViewModel AccountConfigViewModel
        {
            get { return this._accountConfigViewModel; }
        }
    }
}
