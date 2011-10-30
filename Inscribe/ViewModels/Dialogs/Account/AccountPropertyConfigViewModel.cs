using System;
using System.Collections.Generic;
using System.Linq;
using Inscribe.Authentication;
using Inscribe.Storage;
using Inscribe.ViewModels.Common;
using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.Windows;

namespace Inscribe.ViewModels.Dialogs.Account
{
    public class AccountPropertyConfigViewModel : ViewModel
    {
        public AccountInfo AccountInfo { get; private set; }

        public AccountPropertyConfigViewModel(AccountInfo info)
        {
            this.AccountInfo = info;
            this._profileImageProvider = new ProfileImageProvider(info);
        }

        private ProfileImageProvider _profileImageProvider;
        public ProfileImageProvider ProfileImageProvider
        {
            get { return _profileImageProvider; }
        }

        public string ScreenName
        {
            get { return this.AccountInfo.ScreenName; }
        }

        public bool UseUserStreams
        {
            get { return this.AccountInfo.AccountProperty.UseUserStreams; }
            set
            {
                this.AccountInfo.AccountProperty.UseUserStreams = value;
                RaisePropertyChanged(() => UseUserStreams);
            }
        }

        public bool UserStreamsRepliesAll
        {
            get { return this.AccountInfo.AccountProperty.UserStreamsRepliesAll; }
            set
            {
                this.AccountInfo.AccountProperty.UserStreamsRepliesAll = value;
                RaisePropertyChanged(() => UserStreamsRepliesAll);
            }
        }

        public string Footer
        {
            get { return this.AccountInfo.AccountProperty.FooterString; }
            set
            {
                this.AccountInfo.AccountProperty.FooterString = value;
                RaisePropertyChanged(() => Footer);
            }
        }

        public IEnumerable<string> AccountInformations
        {
            get
            {
                return new[] { "なし" }.Concat(AccountStorage.Accounts.Except(new[] { AccountInfo })
                    .Select(a => "@" + a.ScreenName)).ToArray();
            }
        }

        public int FallbackIndex
        {
            get
            {
                if (String.IsNullOrEmpty(AccountInfo.AccountProperty.FallbackAccount) ||
                    !AccountStorage.Contains(AccountInfo.AccountProperty.FallbackAccount))
                {
                    return 0;
                }
                else
                {
                    return Array.IndexOf(AccountStorage.Accounts.Except(new[] { AccountInfo })
                        .Select(a => a.ScreenName).ToArray(),
                        AccountInfo.AccountProperty.FallbackAccount) + 1;
                }
            }
            set
            {
                if (value == 0)
                {
                    AccountInfo.AccountProperty.FallbackAccount = null;
                }
                else
                {
                    AccountInfo.AccountProperty.FallbackAccount = AccountStorage.Accounts
                        .Except(new[] { AccountInfo }).ElementAt(value - 1).ScreenName;
                }
                RaisePropertyChanged(() => FallbackIndex);
            }
        }

        public double RestApiRate
        {
            get { return this.AccountInfo.AccountProperty.AutoCruiseApiConsumeRate; }
            set
            {
                this.AccountInfo.AccountProperty.AutoCruiseApiConsumeRate = value;
                RaisePropertyChanged(() => RestApiRate);
            }
        }

        public double AutoCruiseMu
        {
            get { return this.AccountInfo.AccountProperty.AutoCruiseDefaultMu; }
            set
            {
                this.AccountInfo.AccountProperty.AutoCruiseDefaultMu = value;
                RaisePropertyChanged(() => AutoCruiseMu);
            }
        }

        #region ReauthCommand
        ViewModelCommand _ReauthCommand;

        public ViewModelCommand ReauthCommand
        {
            get
            {
                if (_ReauthCommand == null)
                    _ReauthCommand = new ViewModelCommand(Reauth);
                return _ReauthCommand;
            }
        }

        private void Reauth()
        {
            var authvm = new AuthenticateViewModel(this.AccountInfo);
            Messenger.Raise(new TransitionMessage(authvm, TransitionMode.Modal, "Reauth"));
            if (authvm.Success)
            {
                this.AccountInfo = authvm.GetAccountInfo(this.AccountInfo);
            }
        }
        #endregion

        #region CloseCommand
        ViewModelCommand _CloseCommand;

        public ViewModelCommand CloseCommand
        {
            get
            {
                if (_CloseCommand == null)
                    _CloseCommand = new ViewModelCommand(Close);
                return _CloseCommand;
            }
        }

        private void Close()
        {
            Messenger.Raise(new WindowActionMessage("Close", WindowAction.Close));
        }
        #endregion

        #region Deprecated configuration

        public string[] Queries
        {
            get { return this.AccountInfo.AccountProperty.AccountDependentQuery ?? new string[0]; }
            set
            {
                this.AccountInfo.AccountProperty.AccountDependentQuery = value;
                RaisePropertyChanged(() => Queries);
            }
        }

        private string _addQueryCandidate = null;
        public string AddQueryCandidate
        {
            get { return this._addQueryCandidate; }
            set
            {
                this._addQueryCandidate = value;
                RaisePropertyChanged(() => AddQueryCandidate);
                this.AddQueryCommand.RaiseCanExecuteChanged();
            }
        }

        #region AddQueryCommand
        ViewModelCommand _AddQueryCommand;

        public ViewModelCommand AddQueryCommand
        {
            get
            {
                if (_AddQueryCommand == null)
                    _AddQueryCommand = new ViewModelCommand(AddQuery, CanAddQuery);
                return _AddQueryCommand;
            }
        }

        private bool CanAddQuery()
        {
            return !String.IsNullOrWhiteSpace(this.AddQueryCandidate);
        }

        private void AddQuery()
        {
            this.Queries = this.Queries.Concat(new[] { this.AddQueryCandidate }).Distinct().ToArray();
            this.AddQueryCandidate = String.Empty;
            RaisePropertyChanged(() => Queries);
        }
        #endregion

        #region RemoveQueryCommand
        ListenerCommand<object> _RemoveQueryCommand;

        public ListenerCommand<object> RemoveQueryCommand
        {
            get
            {
                if (_RemoveQueryCommand == null)
                    _RemoveQueryCommand = new ListenerCommand<object>(RemoveQuery);
                return _RemoveQueryCommand;
            }
        }

        private void RemoveQuery(object parameter)
        {
            this.Queries = this.Queries.Except(new[] { parameter as string }).ToArray();
            RaisePropertyChanged(() => Queries);
        }
        #endregion

        #endregion

    }
}
