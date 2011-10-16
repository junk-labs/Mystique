using Inscribe.Configuration;
using Livet;

namespace Inscribe.ViewModels.Dialogs.SettingSub
{
    public class LayoutConfigViewModel : ViewModel, IApplyable
    {
        public LayoutConfigViewModel()
        {
            this._showNotifierBarInBottom = Setting.Instance.NotificationProperty.ShowNotifierBarInBottom;
            this._showInputBlockInBottom = Setting.Instance.InputExperienceProperty.ShowInputBlockInBottom;
            this._showTabInBottom = Setting.Instance.TimelineExperienceProperty.ShowTabInBottom;
            this._showSearchBarInBottom = Setting.Instance.TimelineExperienceProperty.ShowSearchBarInBottom;
        }


        private bool _showNotifierBarInBottom;
        public bool ShowNotifierBarInBottom
        {
            get { return _showNotifierBarInBottom; }
            set
            {
                _showNotifierBarInBottom = value;
                RaisePropertyChanged(() => ShowNotifierBarInBottom);
            }
        }

        private bool _showInputBlockInBottom;
        public bool ShowInputBlockInBottom
        {
            get { return _showInputBlockInBottom; }
            set
            {
                _showInputBlockInBottom = value;
                RaisePropertyChanged(() => ShowInputBlockInBottom);
            }
        }

        private bool _showTabInBottom;
        public bool ShowTabInBottom
        {
            get { return _showTabInBottom; }
            set { _showTabInBottom = value;
            RaisePropertyChanged(() => ShowTabInBottom);
            }
        }

        private bool _showSearchBarInBottom;
        public bool ShowSearchBarInBottom
        {
            get { return _showSearchBarInBottom; }
            set { _showSearchBarInBottom = value;
            RaisePropertyChanged(() => ShowSearchBarInBottom);
            }
        }

        public void Apply()
        {
            Setting.Instance.NotificationProperty.ShowNotifierBarInBottom = this._showNotifierBarInBottom;
            Setting.Instance.InputExperienceProperty.ShowInputBlockInBottom = this._showInputBlockInBottom;
            Setting.Instance.TimelineExperienceProperty.ShowTabInBottom = this._showTabInBottom;
            Setting.Instance.TimelineExperienceProperty.ShowSearchBarInBottom = this._showSearchBarInBottom;
        }
    }
}
