using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Command;
using Livet.Messaging;
using Livet.Messaging.File;
using Livet.Messaging.Window;
using Inscribe.Configuration;
using Inscribe.Model;
using Inscribe.Storage;
using Mystique.ViewModels.Dialogs.Account;
using Inscribe.Communication;

namespace Mystique.ViewModels.Dialogs.SettingSub
{
    public class AccountConfigViewModel : ViewModel
    {
        public AccountConfigViewModel()
        {
            this.ReceiveTweetCount = Setting.Instance.ConnectionProperty.ApiTweetReceiveCount;
            ViewModelHelper.BindNotification(AccountStorage.AccountsChangedEvent,
                this, (o, e) => RaisePropertyChanged(() => Accounts));
        }

        private int _receiveTweetCount;

        public int ReceiveTweetCount
        {
            get { return _receiveTweetCount; }
            set
            {
                _receiveTweetCount = value;
                RaisePropertyChanged(() => ReceiveTweetCount);
            }
        }

        public IEnumerable<AccountInfo> Accounts
        {
            get { return AccountStorage.Accounts.ToArray(); }
        }

        public void WriteBack()
        {
            Setting.Instance.ConnectionProperty.ApiTweetReceiveCount = this.ReceiveTweetCount;
        }

        #region ShowAccountConfigCommand
        DelegateCommand<AccountInfo> _ShowAccountConfigCommand;

        public DelegateCommand<AccountInfo> ShowAccountConfigCommand
        {
            get
            {
                if (_ShowAccountConfigCommand == null)
                    _ShowAccountConfigCommand = new DelegateCommand<AccountInfo>(ShowAccountConfig);
                return _ShowAccountConfigCommand;
            }
        }

        private void ShowAccountConfig(AccountInfo parameter)
        {
            string prevId = parameter.ScreenName;
            var apcvm = new AccountPropertyConfigViewModel(parameter);
            var msg = new TransitionMessage(apcvm, "ShowConfig");
            this.Messenger.Raise(msg);
            if (apcvm.AccountInfo != null && apcvm.AccountInfo.ScreenName != prevId)
            {
                // User ID changed
                UserInformationManager.ReceiveInidividualInfo(apcvm.AccountInfo);
            }
        }
        #endregion

        #region AddAccountCommand
        DelegateCommand _AddAccountCommand;

        public DelegateCommand AddAccountCommand
        {
            get
            {
                if (_AddAccountCommand == null)
                    _AddAccountCommand = new DelegateCommand(AddAccount);
                return _AddAccountCommand;
            }
        }

        private void AddAccount()
        {
            var auth = new AuthenticateViewModel();
            var msg = new TransitionMessage(auth, "ShowAuth");
            this.Messenger.Raise(msg);
            var ainfo = auth.GetAccountInfo();
            if (auth.Success && ainfo != null)
            {
                ShowAccountConfig(ainfo);
                AccountStorage.RegisterAccount(ainfo);
                UserInformationManager.ReceiveInidividualInfo(ainfo);
            }
        }
        #endregion

        #region MoveUpCommand
        DelegateCommand<AccountInfo> _MoveUpCommand;

        public DelegateCommand<AccountInfo> MoveUpCommand
        {
            get
            {
                if (_MoveUpCommand == null)
                    _MoveUpCommand = new DelegateCommand<AccountInfo>(MoveUp);
                return _MoveUpCommand;
            }
        }

        private void MoveUp(AccountInfo parameter)
        {
            AccountStorage.MoveAccount(parameter.ScreenName, AccountStorage.MoveDirection.Up);
        }
        #endregion

        #region MoveDownCommand
        DelegateCommand<AccountInfo> _MoveDownCommand;

        public DelegateCommand<AccountInfo> MoveDownCommand
        {
            get
            {
                if (_MoveDownCommand == null)
                    _MoveDownCommand = new DelegateCommand<AccountInfo>(MoveDown);
                return _MoveDownCommand;
            }
        }

        private void MoveDown(AccountInfo parameter)
        {
            AccountStorage.MoveAccount(parameter.ScreenName, AccountStorage.MoveDirection.Down);
        }
        #endregion

        #region DeleteCommand
        DelegateCommand<AccountInfo> _DeleteCommand;

        public DelegateCommand<AccountInfo> DeleteCommand
        {
            get
            {
                if (_DeleteCommand == null)
                    _DeleteCommand = new DelegateCommand<AccountInfo>(Delete);
                return _DeleteCommand;
            }
        }

        private void Delete(AccountInfo parameter)
        {
            AccountStorage.DeleteAccount(parameter.ScreenName);
        }
        #endregion

    }
}
