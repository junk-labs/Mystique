using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet;
using Inscribe.Storage;
using System.Threading;
using Livet.Commands;
using Dulcet.Twitter;

namespace Inscribe.ViewModels.PartBlocks.BlockCommon
{
    public class NotifierViewModel : ViewModel
    {
        private readonly EventDescription description;

        public NotifierViewModel(EventDescription description, bool enableClose = true)
        {
            this.description = description;
            this.EnableClose = enableClose;
        }

        public EventKind Kind
        {
            get { return this.description.Kind; }
        }

        public string Source
        {
            get { return "@" + this.description.SourceUser.TwitterUser.ScreenName; }
        }

        public string Target
        {
            get
            {
                switch (this.Kind)
                {
                    case EventKind.Favorite:
                    case EventKind.Retweet:
                    case EventKind.Unfavorite:
                        // Show tweet
                        var status = description.TargetTweet.Status as TwitterStatus;
                        if(status != null && status.RetweetedOriginal != null)
                            return "@" + status.RetweetedOriginal.User.ScreenName + ": " 
                                + status.RetweetedOriginal.Text.ToString();
                        else
                            return "@" + description.TargetTweet.Status.User.ScreenName + ": "
                                + description.TargetTweet.Status.Text.ToString();
                    default:
                        // Show user
                        return "@" + description.TargetUser.TwitterUser.ScreenName;
                }
            }
        }

        public string CreatedAt
        {
            get { return this.description.CreatedAt.ToLocalTime().ToString(); }
        }

        public bool EnableClose { get; private set; }

        #region RequireCloseイベント

        public event EventHandler<NotifierViewModelEventArgs> RequireClose;
        private Notificator<NotifierViewModelEventArgs> _RequireCloseEvent;
        public Notificator<NotifierViewModelEventArgs> RequireCloseEvent
        {
            get
            {
                if (_RequireCloseEvent == null) _RequireCloseEvent = new Notificator<NotifierViewModelEventArgs>();
                return _RequireCloseEvent;
            }
            set { _RequireCloseEvent = value; }
        }

        protected void OnRequireClose(NotifierViewModelEventArgs e)
        {
            var threadSafeHandler = Interlocked.CompareExchange(ref RequireClose, null, null);
            if (threadSafeHandler != null) threadSafeHandler(this, e);
            RequireCloseEvent.Raise(e);
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
            OnRequireClose(new NotifierViewModelEventArgs(this));
        }
        #endregion
        
        #region ShowUserCommand
        
        DelegateCommand _ShowUserCommand;

        public DelegateCommand ShowUserCommand
        {
            get
            {
                if (_ShowUserCommand == null)
                    _ShowUserCommand = new DelegateCommand(ShowUser);
                return _ShowUserCommand;
            }
        }

        private void ShowUser()
        {

        }
        
        #endregion
        
        #region ShowTargetCommand
        
        DelegateCommand _ShowTargetCommand;

        public DelegateCommand ShowTargetCommand
        {
            get
            {
                if (_ShowTargetCommand == null)
                    _ShowTargetCommand = new DelegateCommand(ShowTarget);
                return _ShowTargetCommand;
            }
        }

        private void ShowTarget()
        {

        }

        #endregion
      
    }

    public class NotifierViewModelEventArgs : EventArgs
    {
        public NotifierViewModel NotifierViewModel { get; private set; }

        public NotifierViewModelEventArgs(NotifierViewModel notifier)
        {
            this.NotifierViewModel = notifier;
        }
    }
}
