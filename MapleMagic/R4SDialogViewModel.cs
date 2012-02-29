using System;
using System.Linq;
using Dulcet.Twitter.Rest;
using Inscribe.Common;
using Inscribe.Storage;
using Inscribe.ViewModels.PartBlocks.MainBlock;
using Livet;
using Livet.Messaging;
using Livet.Messaging.Windows;
using System.Threading.Tasks;
using Inscribe.Authentication;
using Dulcet.Twitter;

namespace MapleMagic
{
    public class R4SDialogViewModel : ViewModel
    {
        public UserViewModel User { get; private set; }

        public string Receiver { get; private set; }

        public R4SDialogViewModel(UserViewModel user, string receiver, string reason)

        {
            this.User = user;
            this.Receiver = receiver;
            this.Reason = reason;
        }

        public Uri ProfileImage
        {
            get { return User.TwitterUser.ProfileImage; }
        }

        public string ScreenName
        {
            get { return User.TwitterUser.ScreenName; }
        }

        public string Name
        {
            get { return User.TwitterUser.UserName; }
        }

        #region OpenLinkCommand
        private Livet.Commands.ViewModelCommand _OpenLinkCommand;

        public Livet.Commands.ViewModelCommand OpenLinkCommand
        {
            get
            {
                if (_OpenLinkCommand == null)
                {
                    _OpenLinkCommand = new Livet.Commands.ViewModelCommand(OpenLink);
                }
                return _OpenLinkCommand;
            }
        }

        public void OpenLink()
        {
            Browser.Start("http://twitter.com/" + this.ScreenName);
        }
        #endregion

        public string Bio
        {
            get { return User.TwitterUser.Bio; }
        }

        public long TweetsCount
        {
            get { return User.TwitterUser.Tweets; }
        }

        public long FavoritesCount
        {
            get { return User.TwitterUser.Favorites; }
        }

        public long FollowsCount
        {
            get { return User.TwitterUser.Followings; }
        }

        public long FollowersCount
        {
            get { return User.TwitterUser.Followers; }
        }

        public long ListedCount
        {
            get { return User.TwitterUser.Listed; }
        }

        public string Reason { get; private set; }

        #region R4SCommand
        private Livet.Commands.ViewModelCommand _R4SCommand;

        public Livet.Commands.ViewModelCommand R4SCommand
        {
            get
            {
                if (_R4SCommand == null)
                {
                    _R4SCommand = new Livet.Commands.ViewModelCommand(R4S);
                }
                return _R4SCommand;
            }
        }

        public void R4S()
        {
            var cm = new ConfirmationMessage(
                "ユーザー @" + User.TwitterUser.ScreenName + " をスパム報告します。",
                "スパム報告", System.Windows.MessageBoxImage.Warning, System.Windows.MessageBoxButton.OKCancel,
                "Confirm");
            if (this.Messenger.GetResponse(cm).Response.GetValueOrDefault())
            {
                this.Messenger.Raise(new WindowActionMessage("WindowAction", WindowAction.Close));
                Task.Factory.StartNew(() =>
                {
                    AccountStorage.Accounts.ForEach(i => ReportForSpam(i, User.TwitterUser));
                    NotifyStorage.Notify("@" + User.TwitterUser.ScreenName + " をスパム報告しました。");
                });
            }
        }
        #endregion

        private void ReportForSpam(AccountInfo info, TwitterUser user)
        {
            try
            {
                info.ReportSpam(user.NumericId);
            }
            catch (Exception ex)
            {
                ExceptionStorage.Register(ex, ExceptionCategory.TwitterError,
                    "@" + user.ScreenName + " のR4Sに失敗しました。", () => ReportForSpam(info, user));
            }
        }

        #region CloseCommand
        private Livet.Commands.ViewModelCommand _CloseCommand;

        public Livet.Commands.ViewModelCommand CloseCommand
        {
            get
            {
                if (_CloseCommand == null)
                {
                    _CloseCommand = new Livet.Commands.ViewModelCommand(Close);
                }
                return _CloseCommand;
            }
        }

        public void Close()
        {
            this.Messenger.Raise(new WindowActionMessage("WindowAction", WindowAction.Close));
        }
        #endregion

    }
}
