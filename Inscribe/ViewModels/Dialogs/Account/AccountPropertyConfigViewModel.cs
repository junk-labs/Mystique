using System;
using System.Collections.Generic;
using System.Linq;
using Inscribe.Model;
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
            get { return this.AccountInfo.AccoutProperty.UseUserStreams; }
            set
            {
                this.AccountInfo.AccoutProperty.UseUserStreams = value;
                RaisePropertyChanged(() => UseUserStreams);
            }
        }

        public bool UserStreamsRepliesAll
        {
            get { return this.AccountInfo.AccoutProperty.UserStreamsRepliesAll; }
            set
            {
                this.AccountInfo.AccoutProperty.UserStreamsRepliesAll = value;
                RaisePropertyChanged(() => UserStreamsRepliesAll);
            }
        }

        public string Footer
        {
            get { return this.AccountInfo.AccoutProperty.FooterString; }
            set
            {
                this.AccountInfo.AccoutProperty.FooterString = value;
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
                if (String.IsNullOrEmpty(AccountInfo.AccoutProperty.FallbackAccount) ||
                    !AccountStorage.Contains(AccountInfo.AccoutProperty.FallbackAccount))
                {
                    return 0;
                }
                else
                {
                    return Array.IndexOf(AccountStorage.Accounts.Except(new[] { AccountInfo })
                        .Select(a => a.ScreenName).ToArray(),
                        AccountInfo.AccoutProperty.FallbackAccount) + 1;
                }
            }
            set
            {
                if (value == 0)
                {
                    AccountInfo.AccoutProperty.FallbackAccount = null;
                }
                else
                {
                    AccountInfo.AccoutProperty.FallbackAccount = AccountStorage.Accounts
                        .Except(new[] { AccountInfo }).ElementAt(value - 1).ScreenName;
                }
                RaisePropertyChanged(() => FallbackIndex);
            }
        }

        public double RestApiRate
        {
            get { return this.AccountInfo.AccoutProperty.AutoCruiseApiConsumeRate; }
            set
            {
                this.AccountInfo.AccoutProperty.AutoCruiseApiConsumeRate = value;
                RaisePropertyChanged(() => RestApiRate);
            }
        }

        public double AutoCruiseMu
        {
            get { return this.AccountInfo.AccoutProperty.AutoCruiseDefaultMu; }
            set
            {
                this.AccountInfo.AccoutProperty.AutoCruiseDefaultMu = value;
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
            get { return this.AccountInfo.AccoutProperty.AccountDependentQuery ?? new string[0]; }
            set
            {
                this.AccountInfo.AccoutProperty.AccountDependentQuery = value;
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
