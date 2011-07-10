using System;
using System.Linq;
using System.Threading.Tasks;
using Dulcet.Twitter.Rest;
using Inscribe.Communication;
using Inscribe.Model;
using Inscribe.Storage;
using Livet;
using Livet.Command;
using Livet.Messaging;
using Livet.Messaging.Window;

namespace Mystique.ViewModels.Dialogs.Account
{
    public class AccountPropertyConfigViewModel : ViewModel
    {
        public AccountInfo AccountInfo { get; private set; }

        public AccountPropertyConfigViewModel(AccountInfo info)
        {
            this.AccountInfo = info;
        }

        public Uri ProfileImage
        {
            get
            {
                var url = this.AccountInfo.ProfileImage;
                if (url == null)
                {
                    Task.Factory.StartNew(() =>
                        {
                            return UserStorage.Get(ApiHelper.ExecApi(() => this.AccountInfo.GetUserByScreenName(this.AccountInfo.ScreenName)));
                        }).ContinueWith(t => RaisePropertyChanged(() => ProfileImage));
                }
                return url;
            }
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

        public string[] Queries
        {
            get { return this.AccountInfo.AccoutProperty.AccountDependentQuery ?? new string[0]; }
            set
            {
                this.AccountInfo.AccoutProperty.AccountDependentQuery = value;
                RaisePropertyChanged(() => Queries);
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
        DelegateCommand _ReauthCommand;

        public DelegateCommand ReauthCommand
        {
            get
            {
                if (_ReauthCommand == null)
                    _ReauthCommand = new DelegateCommand(Reauth);
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
        DelegateCommand _AddQueryCommand;

        public DelegateCommand AddQueryCommand
        {
            get
            {
                if (_AddQueryCommand == null)
                    _AddQueryCommand = new DelegateCommand(AddQuery, CanAddQuery);
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
        DelegateCommand<object> _RemoveQueryCommand;

        public DelegateCommand<object> RemoveQueryCommand
        {
            get
            {
                if (_RemoveQueryCommand == null)
                    _RemoveQueryCommand = new DelegateCommand<object>(RemoveQuery);
                return _RemoveQueryCommand;
            }
        }

        private void RemoveQuery(object parameter)
        {
            this.Queries = this.Queries.Except(new[] { parameter as string }).ToArray();
            RaisePropertyChanged(() => Queries);
        }
        #endregion
      
      
    }
}
