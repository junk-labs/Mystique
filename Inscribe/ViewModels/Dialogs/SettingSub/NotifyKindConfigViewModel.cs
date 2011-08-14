using Inscribe.Configuration;
using Livet;

namespace Inscribe.ViewModels.Dialogs.SettingSub
{
    public class NotifyKindConfigViewModel : ViewModel, IApplyable
    {
        public NotifyKindConfigViewModel()
        {
            this._notifyMentionEvent = Setting.Instance.NotificationProperty.NotifyMentionEvent;
            this._notifyDmEvent = Setting.Instance.NotificationProperty.NotifyDmEvent;
            this._notifyRetweetEvent = Setting.Instance.NotificationProperty.NotifyRetweetEvent;
            this._notifyFavoriteEvent = Setting.Instance.NotificationProperty.NotifyFavoriteEvent;
            this._notifyFollowEvent = Setting.Instance.NotificationProperty.NotifyFollowEvent;

            this._notifyMention = Setting.Instance.NotificationProperty.NotifyMention;
            this._notifyDm = Setting.Instance.NotificationProperty.NotifyDm;
            this._notifyRetweet = Setting.Instance.NotificationProperty.NotifyRetweet;
            this._notifyFavorite = Setting.Instance.NotificationProperty.NotifyFavorite;
            this._notifyFollow = Setting.Instance.NotificationProperty.NotifyFollow;
            this._notifyReceives = Setting.Instance.NotificationProperty.NotifyReceives;
        }

        private bool _notifyMentionEvent;
        public bool NotifyMentionEvent
        {
            get { return _notifyMentionEvent; }
            set
            {
                _notifyMentionEvent = value;
                RaisePropertyChanged(() => NotifyMentionEvent);
            }
        }

        private bool _notifyDmEvent;
        public bool NotifyDmEvent
        {
            get { return _notifyDmEvent; }
            set
            {
                _notifyDmEvent = value;
                RaisePropertyChanged(() => NotifyDmEvent);
            }
        }

        private bool _notifyRetweetEvent;
        public bool NotifyRetweetEvent
        {
            get { return _notifyRetweetEvent; }
            set
            {
                _notifyRetweetEvent = value;
                RaisePropertyChanged(() => NotifyRetweetEvent);
            }
        }

        private bool _notifyFavoriteEvent;
        public bool NotifyFavoriteEvent
        {
            get { return _notifyFavoriteEvent; }
            set
            {
                _notifyFavoriteEvent = value;
                RaisePropertyChanged(() => NotifyFavoriteEvent);
            }
        }

        private bool _notifyFollowEvent;
        public bool NotifyFollowEvent
        {
            get { return _notifyFollowEvent; }
            set
            {
                _notifyFollowEvent = value;
                RaisePropertyChanged(() => NotifyFollowEvent);
            }
        }

        private bool _notifyMention;
        public bool NotifyMention
        {
            get { return _notifyMention; }
            set
            {
                _notifyMention = value;
                RaisePropertyChanged(() => NotifyMention);
            }
        }

        private bool _notifyDm;
        public bool NotifyDm
        {
            get { return _notifyDm; }
            set
            {
                _notifyDm = value;
                RaisePropertyChanged(() => NotifyDm);
            }
        }

        private bool _notifyRetweet;
        public bool NotifyRetweet
        {
            get { return _notifyRetweet; }
            set
            {
                _notifyRetweet = value;
                RaisePropertyChanged(() => NotifyRetweet);
            }
        }

        private bool _notifyFavorite;
        public bool NotifyFavorite
        {
            get { return _notifyFavorite; }
            set
            {
                _notifyFavorite = value;
                RaisePropertyChanged(() => NotifyFavorite);
            }
        }

        private bool _notifyFollow;
        public bool NotifyFollow
        {
            get { return _notifyFollow; }
            set
            {
                _notifyFollow = value;
                RaisePropertyChanged(() => NotifyFollow);
            }
        }

        private bool _notifyReceives;
        public bool NotifyReceives
        {
            get { return _notifyReceives; }
            set
            {
                _notifyReceives = value;
                RaisePropertyChanged(() => NotifyReceives);
            }
        }

        public void Apply()
        {
            Setting.Instance.NotificationProperty.NotifyMentionEvent = this._notifyMentionEvent;
            Setting.Instance.NotificationProperty.NotifyDmEvent = this._notifyDmEvent;
            Setting.Instance.NotificationProperty.NotifyRetweetEvent = this._notifyRetweetEvent;
            Setting.Instance.NotificationProperty.NotifyFavoriteEvent = this._notifyFavoriteEvent;
            Setting.Instance.NotificationProperty.NotifyFollowEvent = this._notifyFollowEvent;

            Setting.Instance.NotificationProperty.NotifyMention = this._notifyMention;
            Setting.Instance.NotificationProperty.NotifyDm = this._notifyDm;
            Setting.Instance.NotificationProperty.NotifyRetweet = this._notifyRetweet;
            Setting.Instance.NotificationProperty.NotifyFavorite = this._notifyFavorite;
            Setting.Instance.NotificationProperty.NotifyFollow = this._notifyFollow;
            Setting.Instance.NotificationProperty.NotifyReceives = this._notifyReceives;
        }
    }
}
