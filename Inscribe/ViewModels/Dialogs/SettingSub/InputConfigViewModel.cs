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
            this.FallbackBackTracking = Setting.Instance.InputExperienceProperty.FallbackBackTracking;
            this.UseOfficialRetweetFallback = Setting.Instance.InputExperienceProperty.OfficialRetweetFallback;
            this.TrimExceedChars = Setting.Instance.InputExperienceProperty.TrimExceedChars;
            this.AutoRetryOnError = Setting.Instance.InputExperienceProperty.AutoRetryOnError;
        }

        public bool OfficialRetweetInReplyToRetweeter { get; set; }

        public bool UseInputSuggesting { get; set; }

        public bool TrimBeginSpace { get; set; }

        public bool UseActiveFallback { get; set; }

        public bool EnableTemporarilyUserSelection { get; set; }

        public bool FallbackBackTracking { get; set; }

        public bool UseOfficialRetweetFallback { get; set; }

        public bool TrimExceedChars { get; set; }

        public bool AutoRetryOnError { get; set; }


        public void Apply()
        {
            Setting.Instance.InputExperienceProperty.OfficialRetweetInReplyToRetweeter = this.OfficialRetweetInReplyToRetweeter;
            Setting.Instance.InputExperienceProperty.UseInputSuggesting = this.UseInputSuggesting;
            Setting.Instance.InputExperienceProperty.TrimBeginSpace = this.TrimBeginSpace;
            Setting.Instance.InputExperienceProperty.UseActiveFallback = this.UseActiveFallback;
            Setting.Instance.InputExperienceProperty.IsEnabledTemporarilyUserSelection = this.EnableTemporarilyUserSelection;
            Setting.Instance.InputExperienceProperty.FallbackBackTracking = this.FallbackBackTracking;
            Setting.Instance.InputExperienceProperty.OfficialRetweetFallback = this.UseOfficialRetweetFallback;
            Setting.Instance.InputExperienceProperty.TrimExceedChars = this.TrimExceedChars;
            Setting.Instance.InputExperienceProperty.AutoRetryOnError = this.AutoRetryOnError;
        }
    }
}
