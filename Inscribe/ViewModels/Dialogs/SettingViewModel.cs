using Livet;
using Livet.Commands;
using Livet.Messaging.Windows;
using Inscribe.ViewModels.Dialogs.SettingSub;
using Inscribe.Configuration;

namespace Inscribe.ViewModels.Dialogs
{
    public class SettingViewModel : ViewModel
    {
        public SettingViewModel() { }

        private AccountConfigViewModel _accountConfigViewModel = new AccountConfigViewModel();
        public AccountConfigViewModel AccountConfigViewModel
        {
            get { return this._accountConfigViewModel; }
        }

        private GeneralConfigViewModel _generalConfigViewModel = new GeneralConfigViewModel();
        public GeneralConfigViewModel GeneralConfigViewModel
        {
            get { return _generalConfigViewModel; }
        }

        private TimelineConfigViewModel _timelineConfigViewModel = new TimelineConfigViewModel();
        public TimelineConfigViewModel TimelineConfigViewModel
        {
            get { return _timelineConfigViewModel; }
        }

        private TweetViewConfigViewModel _tweetViewConfigViewModel = new TweetViewConfigViewModel();
        public TweetViewConfigViewModel TweetViewConfigViewModel
        {
            get { return _tweetViewConfigViewModel; }
        }

        private ColoringConfigViewModel _coloringConfigViewModel = new ColoringConfigViewModel();
        public ColoringConfigViewModel ColoringConfigViewModel
        {
            get { return _coloringConfigViewModel; }
        }

        private NotifyConfigViewModel _notifyConfigViewModel = new NotifyConfigViewModel();
        public NotifyConfigViewModel NotifyConfigViewModel
        {
            get { return _notifyConfigViewModel; }
        }

        private NotifyKindConfigViewModel _notifyKindConfigViewModel = new NotifyKindConfigViewModel();
        public NotifyKindConfigViewModel NotifyKindConfigViewModel
        {
            get { return _notifyKindConfigViewModel; }
        }

        private InputConfigViewModel _inputConfigViewModel = new InputConfigViewModel();
        public InputConfigViewModel InputConfigViewModel
        {
            get { return _inputConfigViewModel; }
        }

        private MuteConfigViewModel _muteConfigViewModel = new MuteConfigViewModel();
        public MuteConfigViewModel MuteConfigViewModel
        {
            get { return _muteConfigViewModel; }
        }

        private KeyAssignConfigViewModel _keyAssignConfigViewModel = new KeyAssignConfigViewModel();
        public KeyAssignConfigViewModel KeyAssignConfigViewModel
        {
            get { return _keyAssignConfigViewModel; }
        }

        private ExtServiceConfigViewModel _extServiceConfigViewModel = new ExtServiceConfigViewModel();
        public ExtServiceConfigViewModel ExtServiceConfigViewModel
        {
            get { return _extServiceConfigViewModel; }
        }


        #region ApplyCommand
        DelegateCommand _ApplyCommand;

        public DelegateCommand ApplyCommand
        {
            get
            {
                if (_ApplyCommand == null)
                    _ApplyCommand = new DelegateCommand(Apply);
                return _ApplyCommand;
            }
        }

        private void Apply()
        {
            // Apply
            this.AccountConfigViewModel.Apply();
            this.GeneralConfigViewModel.Apply();
            this.TimelineConfigViewModel.Apply();
            this.TweetViewConfigViewModel.Apply();
            this.ColoringConfigViewModel.Apply();
            this.NotifyConfigViewModel.Apply();
            this.NotifyKindConfigViewModel.Apply();
            this.InputConfigViewModel.Apply();
            this.MuteConfigViewModel.Apply();
            this.KeyAssignConfigViewModel.Apply();
            this.ExtServiceConfigViewModel.Apply();

            Setting.RaiseSettingValueChanged();
            Close();
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
      
    }
}
