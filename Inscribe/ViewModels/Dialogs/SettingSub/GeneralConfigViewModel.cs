using Inscribe.Configuration;
using Livet;

namespace Inscribe.ViewModels.Dialogs.SettingSub
{
    public class GeneralConfigViewModel : ViewModel, IApplyable
    {
        public GeneralConfigViewModel()
        {
            this._powerUserMode = Setting.Instance.ExperienceProperty.IsPowerUserMode;
            this._updateKind = Setting.Instance.ExperienceProperty.UpdateKind;
        }

        private bool _powerUserMode;
        public bool PowerUserMode
        {
            get { return _powerUserMode; }
            set
            {
                _powerUserMode = value;
                RaisePropertyChanged(() => PowerUserMode);
            }
        }

        private int _updateKind;
        public int UpdateKind
        {
            get { return _updateKind; }
            set
            {
                _updateKind = value;
                RaisePropertyChanged(() => UpdateKind);
            }
        }

        public void Apply()
        {
            Setting.Instance.ExperienceProperty.IsPowerUserMode = this._powerUserMode;
            Setting.Instance.ExperienceProperty.UpdateKind = this._updateKind;
        }
    }
}
