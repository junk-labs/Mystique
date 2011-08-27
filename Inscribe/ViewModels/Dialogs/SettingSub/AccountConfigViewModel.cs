using System;
using System.Collections.Generic;
using System.Linq;
using Inscribe.Communication;
using Inscribe.Model;
using Inscribe.Storage;
using Livet;
using Livet.Commands;
using Livet.Messaging;
using Inscribe.ViewModels.Dialogs.Account;
using Inscribe.ViewModels.Common;

namespace Inscribe.ViewModels.Dialogs.SettingSub
{
    public class AccountConfigViewModel : ViewModel, IApplyable
    {
        public AccountConfigViewModel()
        {
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

        public IEnumerable<AccountInfoViewModel> Accounts
        {
            get { return AccountStorage.Accounts.Select(a => new AccountInfoViewModel(a)).ToArray(); }
        }

        #region ShowAccountConfigCommand
        ListenerCommand<AccountInfoViewModel> _ShowAccountConfigCommand;

        public ListenerCommand<AccountInfoViewModel> ShowAccountConfigCommand
        {
            get
            {
                if (_ShowAccountConfigCommand == null)
                    _ShowAccountConfigCommand = new ListenerCommand<AccountInfoViewModel>(i => ShowAccountConfig(i.info));
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
        ViewModelCommand _AddAccountCommand;

        public ViewModelCommand AddAccountCommand
        {
            get
            {
                if (_AddAccountCommand == null)
                    _AddAccountCommand = new ViewModelCommand(AddAccount);
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

        public void Apply() { }
    }

    public class AccountInfoViewModel : ViewModel
    {
        public AccountInfo info;
        public AccountInfoViewModel(AccountInfo info)
        {
            this.info = info;
            this._profileImageProvider = new ProfileImageProvider(info);
        }

        public string ScreenName
        {
            get { return this.info.ScreenName; }
        }

        public bool IsSelectedDefault
        {
            get { return this.info.AccoutProperty.IsSelectedDefault; }
            set { this.info.AccoutProperty.IsSelectedDefault = value; }
        }

        private ProfileImageProvider _profileImageProvider;

        public ProfileImageProvider ProfileImageProvider
        {
            get { return _profileImageProvider; }
        }

        #region MoveUpCommand
        ViewModelCommand _MoveUpCommand;

        public ViewModelCommand MoveUpCommand
        {
            get
            {
                if (_MoveUpCommand == null)
                    _MoveUpCommand = new ViewModelCommand(MoveUp);
                return _MoveUpCommand;
            }
        }

        private void MoveUp()
        {
            AccountStorage.MoveAccount(this.ScreenName, AccountStorage.MoveDirection.Up);
        }
        #endregion

        #region MoveDownCommand
        ViewModelCommand _MoveDownCommand;

        public ViewModelCommand MoveDownCommand
        {
            get
            {
                if (_MoveDownCommand == null)
                    _MoveDownCommand = new ViewModelCommand(MoveDown);
                return _MoveDownCommand;
            }
        }

        private void MoveDown()
        {
            AccountStorage.MoveAccount(this.ScreenName, AccountStorage.MoveDirection.Down);
        }
        #endregion

        #region DeleteCommand
        ViewModelCommand _DeleteCommand;

        public ViewModelCommand DeleteCommand
        {
            get
            {
                if (_DeleteCommand == null)
                    _DeleteCommand = new ViewModelCommand(Delete);
                return _DeleteCommand;
            }
        }

        private void Delete()
        {
            AccountStorage.DeleteAccount(this.ScreenName);
        }
        #endregion


        #region DeleteConfirmCommand
        ListenerCommand<ConfirmationMessage> _DeleteConfirmCommand;

        public ListenerCommand<ConfirmationMessage> DeleteConfirmCommand
        {
            get
            {
                if (_DeleteConfirmCommand == null)
                    _DeleteConfirmCommand = new ListenerCommand<ConfirmationMessage>(DeleteConfirm);
                return _DeleteConfirmCommand;
            }
        }

        private void DeleteConfirm(ConfirmationMessage parameter)
        {
            if (parameter.Response.GetValueOrDefault())
            {
                Delete();
            }
        }
        #endregion
      
    }
}
