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

        public void Apply()
        {
            Setting.Instance.NotificationProperty.ShowNotifierBarInBottom = this._showNotifierBarInBottom;
        }
    }
}
