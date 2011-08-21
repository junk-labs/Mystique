using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet;
using Inscribe.Configuration;
using Inscribe.Configuration.Settings;

namespace Inscribe.ViewModels.Dialogs.SettingSub
{
    public class TimelineConfigViewModel : ViewModel, IApplyable
    {
        public bool PowerUserMode
        {
            get { return Setting.Instance.ExperienceProperty.PowerUserMode; }
        }

        public TimelineConfigViewModel()
        {
            this._timelineScrollLockIndex = (int)Setting.Instance.TimelineExperienceProperty.ScrollLockMode;
            this._useFastScrolling = Setting.Instance.TimelineExperienceProperty.FastScrolling;
            this._tweetInitStrategyIndex = (int)Setting.Instance.TimelineExperienceProperty.TimelineItemInitStrategy;
        }

        public int _timelineScrollLockIndex;
        public int TimelineScrollLockIndex
        {
            get { return this._timelineScrollLockIndex; }
            set
            {
                this._timelineScrollLockIndex = value;
                RaisePropertyChanged(() => TimelineScrollLockIndex);
            }
        }

        private bool _useFastScrolling;
        public bool UseFastScrolling
        {
            get { return this._useFastScrolling; }
            set
            {
                this._useFastScrolling = value;
                RaisePropertyChanged(() => UseFastScrolling);
            }
        }

        private int _tweetInitStrategyIndex;
        public int TweetInitStrategyIndex
        {
            get { return this._tweetInitStrategyIndex; }
            set
            {
                this._tweetInitStrategyIndex = value;
                RaisePropertyChanged(() => TweetInitStrategyIndex);
            }
        }


        public void Apply()
        {
            Setting.Instance.TimelineExperienceProperty.ScrollLockMode = (ScrollLock)this._timelineScrollLockIndex;
            Setting.Instance.TimelineExperienceProperty.FastScrolling = this._useFastScrolling;
            Setting.Instance.TimelineExperienceProperty.TimelineItemInitStrategy = (ItemInitStrategy)this._tweetInitStrategyIndex;
        }
    }
}
