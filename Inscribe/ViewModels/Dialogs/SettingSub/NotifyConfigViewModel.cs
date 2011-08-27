using Inscribe.Configuration;
using Inscribe.Configuration.Settings;
using Livet;

namespace Inscribe.ViewModels.Dialogs.SettingSub
{
    public class NotifyConfigViewModel : ViewModel, IApplyable
    {
        public bool IsPowerUserMode
        {
            get { return Setting.Instance.ExperienceProperty.IsPowerUserMode; }
        }

        public NotifyConfigViewModel()
        {
            this._tabNotifyEnabledAsDefault = Setting.Instance.NotificationProperty.IsTabNotifyEnabledAsDefault;
            this._isEnabledNotificationBar = Setting.Instance.NotificationProperty.IsEnabledNotificationBar;
            this._isShowMultiple = Setting.Instance.NotificationProperty.ShowMultiple;
            this._isNotifierBarBottom = Setting.Instance.NotificationProperty.ShowNotifierBarInBottom;

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

        private bool _isNotifierBarBottom;
        public bool IsNotifierBarBottom
        {
            get { return _isNotifierBarBottom; }
            set
            {
                _isNotifierBarBottom = value;
                RaisePropertyChanged(() => IsNotifierBarBottom);
            }
        }

        public void Apply()
        {
            Setting.Instance.NotificationProperty.IsTabNotifyEnabledAsDefault = this._tabNotifyEnabledAsDefault;
            Setting.Instance.NotificationProperty.IsEnabledNotificationBar = this._isEnabledNotificationBar;
            Setting.Instance.NotificationProperty.ShowMultiple = this._isShowMultiple;
            Setting.Instance.NotificationProperty.ShowNotifierBarInBottom = this._isNotifierBarBottom;

            Setting.Instance.NotificationProperty.NotifyLocation = (NotifyLocation)this._notifyLocationIndex;
            Setting.Instance.NotificationProperty.WindowNotificationStrategy = (NotificationStrategy)this._windowNotificationStrategyIndex;
            Setting.Instance.NotificationProperty.SoundNotificationStrategy = (NotificationStrategy)this._soundNotificationStrategyIndex;
        }

    }
}
