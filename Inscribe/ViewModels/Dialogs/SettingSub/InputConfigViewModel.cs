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
            this.ActiveFallback = Setting.Instance.InputExperienceProperty.ActiveFallback;
        }

        public bool UseInputSuggesting { get; set; }

        public bool TrimBeginSpace { get; set; }

        public bool ActiveFallback { get; set; }

        public void Apply()
        {
            Setting.Instance.InputExperienceProperty.UseInputSuggesting = this.UseInputSuggesting;
            Setting.Instance.InputExperienceProperty.TrimBeginSpace = this.TrimBeginSpace;
            Setting.Instance.InputExperienceProperty.ActiveFallback = this.ActiveFallback;
        }
    }
}
