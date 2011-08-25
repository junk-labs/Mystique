using System;
using Dulcet.Twitter;
using Inscribe.Configuration;
using Inscribe.Filter;
using Inscribe.Filter.Filters.ScreenName;
using Inscribe.Filter.Filters.Text;
using Inscribe.Storage;
using Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild;
using Livet;
using Livet.Commands;
using Livet.Messaging.Windows;

namespace Inscribe.ViewModels.Dialogs
{
    public class MuteViewModel : ViewModel
    {
        private TweetViewModel _status;

        public MuteViewModel(TweetViewModel tvm)
        {
            this._status = tvm;
            this.MuteText = tvm.Status.Text;
        }

        private string _muteText;

        public string MuteText
        {
            get { return _muteText; }
            set
            {
                _muteText = value;
                RaisePropertyChanged(() => MuteText);
                this.OkCommand.RaiseCanExecuteChanged();
            }
        }

        public string ClientNameText
        {
            get
            {
                var st = this._status.Status as TwitterStatus;
                if (st != null)
                    return st.Source;
                else
                    return String.Empty;
            }
        }

        public string UserScreenName
        {
            get { return _status.Status.User.ScreenName; }
        }

        public enum NGKindTypes
        {
            Keyword,
            ClientName,
            UserName
        }

        private NGKindTypes _ngKind = NGKindTypes.Keyword;
        public NGKindTypes NGKind
        {
            get { return this._ngKind; }
            set
            {
                this._ngKind = value;
                RaisePropertyChanged(() => NGKind);
                this.OkCommand.RaiseCanExecuteChanged();
            }
        }


        #region OkCommand
        ViewModelCommand _OkCommand;

        public ViewModelCommand OkCommand
        {
            get
            {
                if (_OkCommand == null)
                    _OkCommand = new ViewModelCommand(Ok, CanOk);
                return _OkCommand;
            }
        }

        private bool CanOk()
        {
            switch (this.NGKind)
            {
                case NGKindTypes.ClientName:
                    return true;
                case NGKindTypes.Keyword:
                    return !String.IsNullOrEmpty(this.MuteText);
                case NGKindTypes.UserName:
                    return true;
                default:
                    return false;
            }
        }

        private void Ok()
        {
            // remove from krile
            Setting.Instance.TimelineFilteringProperty.MuteFilterCluster =
                Setting.Instance.TimelineFilteringProperty.MuteFilterCluster.Join(this.GenerateConfiguredFilter());
            TweetStorage.UpdateMute();
            this.Messenger.Raise(new WindowActionMessage("WindowAction", WindowAction.Close));
        }
        #endregion

        private FilterBase GenerateConfiguredFilter()
        {
            switch (this.NGKind)
            {
                case NGKindTypes.ClientName:
                    return new FilterVia("^" + this.ClientNameText + "$");
                case NGKindTypes.Keyword:
                    return new FilterText(this.MuteText);
                case NGKindTypes.UserName:
                    return new FilterUser("^" + this.UserScreenName + "$");
                default:
                    throw new InvalidOperationException("Invalid kind:" + this.NGKind.ToString());
            }
        }
    }
}
