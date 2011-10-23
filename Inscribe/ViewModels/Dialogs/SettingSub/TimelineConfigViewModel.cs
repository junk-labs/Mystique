using Inscribe.Configuration;
using Inscribe.Configuration.Settings;
using Livet;

namespace Inscribe.ViewModels.Dialogs.SettingSub
{
    public class TimelineConfigViewModel : ViewModel, IApplyable
    {
        public bool IsAloofUserMode
        {
            get { return Setting.Instance.ExperienceProperty.IsAloofUserMode; }
        }

        public TimelineConfigViewModel()
        {
            this._timelineScrollLockIndex = (int)Setting.Instance.TimelineExperienceProperty.ScrollLockMode;
            this._timelineStrictLock = Setting.Instance.TimelineExperienceProperty.TimelineStrictLock;
            this._isTimelineAscending = Setting.Instance.TimelineExperienceProperty.OrderByAscending;
            this._useFastScrolling = Setting.Instance.TimelineExperienceProperty.UseFastScrolling;
            this._tweetInitStrategyIndex = (int)Setting.Instance.TimelineExperienceProperty.TimelineItemInitStrategy;
            this._useIntelligentOrdering = Setting.Instance.TimelineExperienceProperty.UseIntelligentOrdering;
            this._intelligentOrderingThresholdSec = Setting.Instance.TimelineExperienceProperty.IntelligentOrderingThresholdSec;
        }

        private int _timelineScrollLockIndex;
        public int TimelineScrollLockIndex
        {
            get { return this._timelineScrollLockIndex; }
            set
            {
                this._timelineScrollLockIndex = value;
                RaisePropertyChanged(() => TimelineScrollLockIndex);
            }
        }

        private bool _timelineStrictLock;
        public bool TimelineStrictLock
        {
            get { return this._timelineStrictLock; }
            set
            {
                this._timelineStrictLock = value;
                RaisePropertyChanged(() => TimelineStrictLock);
            }
        }

        private bool _isTimelineAscending;
        public bool IsTimelineAscending
        {
            get { return _isTimelineAscending; }
            set { _isTimelineAscending = value;
            RaisePropertyChanged(() => IsTimelineAscending);
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

        private bool _useIntelligentOrdering;
        public bool UseIntelligentOrdering
        {
            get { return _useIntelligentOrdering; }
            set
            {
                _useIntelligentOrdering = value;
                RaisePropertyChanged(() => UseIntelligentOrdering);
            }
        }

        private int _intelligentOrderingThresholdSec;
        public int IntelligentOrderingThresholdSec
        {
            get { return _intelligentOrderingThresholdSec; }
            set { _intelligentOrderingThresholdSec = value;
            RaisePropertyChanged(() => IntelligentOrderingThresholdSec);
            }
        }

        public void Apply()
        {
            Setting.Instance.TimelineExperienceProperty.ScrollLockMode = (ScrollLock)this._timelineScrollLockIndex;
            Setting.Instance.TimelineExperienceProperty.TimelineStrictLock = this._timelineStrictLock;
            Setting.Instance.TimelineExperienceProperty.UseFastScrolling = this._useFastScrolling;
            Setting.Instance.TimelineExperienceProperty.OrderByAscending = this._isTimelineAscending;
            Setting.Instance.TimelineExperienceProperty.TimelineItemInitStrategy = (ItemInitStrategy)this._tweetInitStrategyIndex;
            Setting.Instance.TimelineExperienceProperty.UseIntelligentOrdering = this._useIntelligentOrdering;
            Setting.Instance.TimelineExperienceProperty.IntelligentOrderingThresholdSec = this._intelligentOrderingThresholdSec;
        }
    }
}
