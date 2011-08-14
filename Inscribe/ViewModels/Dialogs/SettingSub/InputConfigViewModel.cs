using Inscribe.Configuration;
using Livet;

namespace Inscribe.ViewModels.Dialogs.SettingSub
{
    class InputConfigViewModel : ViewModel, IApplyable
    {
        public InputConfigViewModel()
        {
            this.UseInputSuggesting = Setting.Instance.InputExperienceProperty.UseInputSuggesting;
            this.TrimBeginSpace = Setting.Instance.InputExperienceProperty.TrimBeginSpace;
            this.AutoBindInputtedHashtag = Setting.Instance.InputExperienceProperty.AutoBindInputtedHashtag;
        }

        public bool UseInputSuggesting { get; set; }

        public bool TrimBeginSpace { get; set; }

        public bool AutoBindInputtedHashtag { get; set; }

        public void Apply()
        {
            Setting.Instance.InputExperienceProperty.UseInputSuggesting = this.UseInputSuggesting;
            Setting.Instance.InputExperienceProperty.TrimBeginSpace = this.TrimBeginSpace;
            Setting.Instance.InputExperienceProperty.AutoBindInputtedHashtag = this.AutoBindInputtedHashtag;
        }
    }
}
