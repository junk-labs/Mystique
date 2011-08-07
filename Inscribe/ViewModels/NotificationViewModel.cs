using Inscribe.Storage;
using Inscribe.ViewModels.PartBlocks.MainBlock;
using Livet;
using Livet.Commands;
using Livet.Messaging.Windows;
using System;
using System.Media;
using System.IO;
using System.Threading.Tasks;

namespace Inscribe.ViewModels
{
    public class NotificationViewModel : ViewModel
    {
        public MainWindowViewModel Parent { get; private set; }
        public NotificationViewModel(MainWindowViewModel parent, UserViewModel source, UserViewModel target, string text, EventKind eventKind = EventKind.Undefined)
        {
            this.Parent = parent;
            this.SourceUser = source;
            this.TargetUser = target;
            this.Text = text;
            this.NotifyEventKind = eventKind;
            this.RaisePropertyChanged(() => SourceUser);
            this.RaisePropertyChanged(() => TargetUser);
            this.RaisePropertyChanged(() => Text);
            this.RaisePropertyChanged(() => NotifyEventKind);
        }

        public Uri UserImage
        {
            get { return SourceUser.TwitterUser.ProfileImage; }
        }

        public UserViewModel SourceUser
        {
            get;
            private set;
        }

        public UserViewModel TargetUser
        {
            get;
            private set;
        }

        public string Text
        {
            get;
            private set;
        }

        public EventKind NotifyEventKind
        {
            get;
            private set;
        }

        #region ClickCommand
        DelegateCommand _ClickCommand;

        public DelegateCommand ClickCommand
        {
            get
            {
                if (_ClickCommand == null)
                    _ClickCommand = new DelegateCommand(Click);
                return _ClickCommand;
            }
        }

        private void Click()
        {
            Parent.Messenger.Raise(new WindowActionMessage("WindowAction", WindowAction.Normal));
            Parent.Messenger.Raise(new WindowActionMessage("WindowAction", WindowAction.Active));
            this.Messenger.Raise(new WindowActionMessage("WindowAction", WindowAction.Close));
        }
        #endregion
    }
}
