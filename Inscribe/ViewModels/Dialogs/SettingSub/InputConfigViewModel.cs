using Inscribe.Configuration;
using Livet;

namespace Inscribe.ViewModels.Dialogs.SettingSub
{
    public class InputConfigViewModel : ViewModel, IApplyable
    {
        public InputConfigViewModel()
        {
            this.UseInputSuggesting = Setting.Instance.InputExperienceProperty.UseInputSuggesting;
            this.TrimBeginSpace = Setting.Instance.InputExperienceProperty.TrimBeginSpace;
            this.UseActiveFallback = Setting.Instance.InputExperienceProperty.UseActiveFallback;
            this.EnableTemporarilyUserSelection = Setting.Instance.InputExperienceProperty.IsEnabledTemporarilyUserSelection;
        }

        public bool UseInputSuggesting { get; set; }

        public bool TrimBeginSpace { get; set; }

        public bool UseActiveFallback { get; set; }

        public bool EnableTemporarilyUserSelection { get; set; }


        public void Apply()
        {
            Setting.Instance.InputExperienceProperty.UseInputSuggesting = this.UseInputSuggesting;
            Setting.Instance.InputExperienceProperty.TrimBeginSpace = this.TrimBeginSpace;
            Setting.Instance.InputExperienceProperty.UseActiveFallback = this.UseActiveFallback;
            Setting.Instance.InputExperienceProperty.IsEnabledTemporarilyUserSelection = this.EnableTemporarilyUserSelection;
        }
    }
}
