using Inscribe.Configuration;
using Livet;

namespace Inscribe.ViewModels.Dialogs.SettingSub
{
    public class InputConfigViewModel : ViewModel, IApplyable
    {
        public InputConfigViewModel()
        {
            this.UseInputSuggesting = Setting.Instance.InputExperienceProperty.UseInputSuggesting;
            this.OfficialRetweetInReplyToRetweeter = Setting.Instance.InputExperienceProperty.OfficialRetweetInReplyToRetweeter;
            this.TrimBeginSpace = Setting.Instance.InputExperienceProperty.TrimBeginSpace;
            this.UseActiveFallback = Setting.Instance.InputExperienceProperty.UseActiveFallback;
            this.EnableTemporarilyUserSelection = Setting.Instance.InputExperienceProperty.IsEnabledTemporarilyUserSelection;
            this.UseOfficialRetweetFallback = Setting.Instance.InputExperienceProperty.OfficialRetweetFallback;
            this.TrimExceedChars = Setting.Instance.InputExperienceProperty.TrimExceedChars;
            this.AutoRetryOnTimeout = Setting.Instance.InputExperienceProperty.AutoRetryOnTimeout;
        }

        public bool OfficialRetweetInReplyToRetweeter { get; set; }

        public bool UseInputSuggesting { get; set; }

        public bool TrimBeginSpace { get; set; }

        public bool UseActiveFallback { get; set; }

        public bool EnableTemporarilyUserSelection { get; set; }

        public bool UseOfficialRetweetFallback { get; set; }

        public bool TrimExceedChars { get; set; }

        public bool AutoRetryOnTimeout { get; set; }


        public void Apply()
        {
            Setting.Instance.InputExperienceProperty.OfficialRetweetInReplyToRetweeter = this.OfficialRetweetInReplyToRetweeter;
            Setting.Instance.InputExperienceProperty.UseInputSuggesting = this.UseInputSuggesting;
            Setting.Instance.InputExperienceProperty.TrimBeginSpace = this.TrimBeginSpace;
            Setting.Instance.InputExperienceProperty.UseActiveFallback = this.UseActiveFallback;
            Setting.Instance.InputExperienceProperty.IsEnabledTemporarilyUserSelection = this.EnableTemporarilyUserSelection;
            Setting.Instance.InputExperienceProperty.OfficialRetweetFallback = this.UseOfficialRetweetFallback;
            Setting.Instance.InputExperienceProperty.TrimExceedChars = this.TrimExceedChars;
            Setting.Instance.InputExperienceProperty.AutoRetryOnTimeout = this.AutoRetryOnTimeout;
        }
    }
}
