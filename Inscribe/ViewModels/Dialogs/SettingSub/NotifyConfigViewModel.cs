using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet;
using Inscribe.Configuration;
using Inscribe.Configuration.Settings;

namespace Inscribe.ViewModels.Dialogs.SettingSub
{
    public class NotifyConfigViewModel : ViewModel, IApplyable
    {
        public NotifyConfigViewModel()
        {
            this._tabNotifyEnabledAsDefault = Setting.Instance.NotificationProperty.TabNotifyEnabledAsDefault;
            this._tabNotifyStackTopTimeline = Setting.Instance.NotificationProperty.TabNotifyStackTopTimeline;
            this._isEnabledNotificationBar = Setting.Instance.NotificationProperty.IsEnabledNotificationBar;
            this._notifyMentionEvent = Setting.Instance.NotificationProperty.NotifyDmEvent;
            this._notifyDmEvent = Setting.Instance.NotificationProperty.NotifyDmEvent;
            this._notifyRetweetEvent = Setting.Instance.NotificationProperty.NotifyRetweetEvent;
            this._notifyFavoriteEvent = Setting.Instance.NotificationProperty.NotifyFavoriteEvent;
            this._notifyMention = Setting.Instance.NotificationProperty.NotifyDm;
            this._notifyDm = Setting.Instance.NotificationProperty.NotifyDm;
            this._notifyRetweet = Setting.Instance.NotificationProperty.NotifyRetweet;
            this._notifyFavorite = Setting.Instance.NotificationProperty.NotifyFavorite;
            this._notifyReceives = Setting.Instance.NotificationProperty.NotifyReceives;
            this._isShowMultiple = Setting.Instance.NotificationProperty.IsShowMultiple;
            this._notifyInMainWindowDisplay = Setting.Instance.NotificationProperty.NotifyInMainWindowDisplay;
            this._notifyLocationIndex = (int)Setting.Instance.NotificationProperty.NotifyLocation;
            this._windowNotificationStrategyIndex = (int)Setting.Instance.NotificationProperty.WindowNotificationStrategy;
            this._soundNotificationStrategyIndex = (int)Setting.Instance.NotificationProperty.SoundNotificationStrategy;
        }

        private bool _tabNotifyEnabledAsDefault;
        public bool TabNotifyEnabledAsDefault
        {
            get { return _tabNotifyEnabledAsDefault; }
            set
            {
                _tabNotifyEnabledAsDefault = value;
                RaisePropertyChanged(() => TabNotifyEnabledAsDefault);
            }
        }

        private bool _tabNotifyStackTopTimeline;
        public bool TabNotifyStackTopTimeline
        {
            get { return _tabNotifyStackTopTimeline; }
            set
            {
                _tabNotifyStackTopTimeline = value;
                RaisePropertyChanged(() => TabNotifyStackTopTimeline);
            }
        }

        private bool _isEnabledNotificationBar;
        public bool IsEnabledNotificationBar
        {
            get { return _isEnabledNotificationBar; }
            set
            {
                _isEnabledNotificationBar = value;
                RaisePropertyChanged(() => IsEnabledNotificationBar);
            }
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

        private bool _isShowMultiple;
        public bool IsShowMultiple
        {
            get { return _isShowMultiple; }
            set
            {
                _isShowMultiple = value;
                RaisePropertyChanged(() => IsShowMultiple);
            }
        }

        private bool _notifyInMainWindowDisplay;
        public bool NotifyInMainWindowDisplay
        {
            get { return _notifyInMainWindowDisplay; }
            set
            {
                _notifyInMainWindowDisplay = value;
                RaisePropertyChanged(() => NotifyInMainWindowDisplay);
            }
        }

        private int _notifyLocationIndex;
        public int NotifyLocationIndex
        {
            get { return _notifyLocationIndex; }
            set
            {
                _notifyLocationIndex = value;
                RaisePropertyChanged(() => NotifyLocationIndex);
            }
        }

        private int _windowNotificationStrategyIndex;
        public int WindowNotificationStrategyIndex
        {
            get { return _windowNotificationStrategyIndex; }
            set
            {
                _windowNotificationStrategyIndex = value;
                RaisePropertyChanged(() => WindowNotificationStrategyIndex);
            }
        }

        private int _soundNotificationStrategyIndex;
        public int SoundNotificationStrategyIndex
        {
            get { return _soundNotificationStrategyIndex; }
            set
            {
                _soundNotificationStrategyIndex = value;
                RaisePropertyChanged(() => SoundNotificationStrategyIndex);
            }
        }

        public void Apply()
        {
            Setting.Instance.NotificationProperty.TabNotifyEnabledAsDefault = this._tabNotifyEnabledAsDefault;
            Setting.Instance.NotificationProperty.TabNotifyStackTopTimeline = this._tabNotifyStackTopTimeline;
            Setting.Instance.NotificationProperty.IsEnabledNotificationBar = this._isEnabledNotificationBar;
            Setting.Instance.NotificationProperty.NotifyDmEvent = this._notifyMentionEvent;
            Setting.Instance.NotificationProperty.NotifyDmEvent = this._notifyDmEvent;
            Setting.Instance.NotificationProperty.NotifyRetweetEvent = this._notifyRetweetEvent;
            Setting.Instance.NotificationProperty.NotifyFavoriteEvent = this._notifyFavoriteEvent;
            Setting.Instance.NotificationProperty.NotifyDm = this._notifyMention;
            Setting.Instance.NotificationProperty.NotifyDm = this._notifyDm;
            Setting.Instance.NotificationProperty.NotifyRetweet = this._notifyRetweet;
            Setting.Instance.NotificationProperty.NotifyFavorite = this._notifyFavorite;
            Setting.Instance.NotificationProperty.NotifyReceives = this._notifyReceives;
            Setting.Instance.NotificationProperty.IsShowMultiple = this._isShowMultiple;
            Setting.Instance.NotificationProperty.NotifyInMainWindowDisplay = this._notifyInMainWindowDisplay;
            Setting.Instance.NotificationProperty.NotifyLocation = (NotifyLocation) this._notifyLocationIndex;
            Setting.Instance.NotificationProperty.WindowNotificationStrategy = (NotificationStrategy)this._windowNotificationStrategyIndex;
            Setting.Instance.NotificationProperty.SoundNotificationStrategy = (NotificationStrategy)this._soundNotificationStrategyIndex;
        }
    }
}
