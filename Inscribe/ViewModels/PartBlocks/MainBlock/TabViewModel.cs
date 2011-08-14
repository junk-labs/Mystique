using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Inscribe.Configuration;
using Inscribe.Configuration.Tabs;
using Inscribe.Filter;
using Inscribe.Filter.Core;
using Inscribe.Filter.Filters.ScreenName;
using Inscribe.Filter.Filters.Text;
using Inscribe.Storage;
using Inscribe.Subsystems;
using Livet;
using Livet.Commands;
using Livet.Messaging;

namespace Inscribe.ViewModels.PartBlocks.MainBlock
{
    public class TabViewModel : ViewModel
    {
        public ColumnViewModel Parent { get; private set; }

        /// <summary>
        /// このタブがまだ生存しているか
        /// </summary>
        public bool IsAlive { get; set; }

        private TabProperty _tabProperty;
        public TabProperty TabProperty
        {
            get { return this._tabProperty; }
            set
            {
                this._tabProperty = value;
                RaisePropertyChanged(() => TabProperty);
            }
        }

        /// <summary>
        /// このタブがフォーカスを得た
        /// </summary>
        public event EventHandler GotFocus;
        protected void OnGetFocus()
        {
            var fchandler = GotFocus;
            if (fchandler != null)
                fchandler(this, EventArgs.Empty);
        }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return this._isSelected; }
            set
            {
                this._isSelected = value;
                if (value)
                    this.NewTweetsCount = 0;
                RaisePropertyChanged(() => IsSelected);
                RaisePropertyChanged(() => IsCurrentFocused);
            }
        }

        private bool _isTimelineMouseOver = false;
        public bool IsTimelineMouseOver
        {
            get { return this._isTimelineMouseOver; }
            set
            {
                this._isTimelineMouseOver = value;
                RaisePropertyChanged(() => IsTimelineMouseOver);
            }
        }

        private int _newTweetsCount = 0;
        public int NewTweetsCount
        {
            get { return this._newTweetsCount; }
            protected set
            {
                if (IsSelected)
                    this._newTweetsCount = 0;
                else
                    this._newTweetsCount = value;
                RaisePropertyChanged(() => NewTweetsCount);
            }
        }

        public bool IsFullLineView
        {
            get { return Setting.Instance.TimelineExperienceProperty.FullLineView; }
        }

        public TabViewModel(ColumnViewModel parent, TabProperty property = null)
        {
            this.Parent = parent;

            this.IsAlive = true;
            this._tabProperty = property ?? new TabProperty();
            ViewModelHelper.BindNotification(this._tabProperty.LinkAccountInfoChangedEvent, this, (o, e) =>
                this.StackingTimelines.OfType<TimelineListViewModel>()
                .SelectMany(vm => vm.TimelineListCoreViewModel.TweetsSource)
                .ForEach(t => t.PendingColorChanged(true)));

            this.AddTopTimeline(null);
            ViewModelHelper.BindNotification(Setting.SettingValueChangedEvent, this, (o, e) => UpdateSettingValue());
        }

        private void UpdateSettingValue()
        {
            RaisePropertyChanged(() => IsFullLineView);
        }

        public void SetTabOwner(ColumnViewModel newParent)
        {
            this.Parent = newParent;
        }

        public void InvalidateCache()
        {
            this.StackingTimelines.ForEach(f => f.InvalidateCache());
        }

        public bool IsCurrentFocused
        {
            get
            {
                return this.Parent.Parent.CurrentFocusColumn == this.Parent &&
                    IsSelected;
            }
        }

        public void UpdateIsCurrentFocused()
        {
            RaisePropertyChanged(() => IsCurrentFocused);
        }

        private ObservableCollection<TimelineCoreViewModelBase> _stackings = new ObservableCollection<TimelineCoreViewModelBase>();
        public IEnumerable<TimelineCoreViewModelBase> StackingTimelines
        {
            get { return _stackings; }
        }

        public TimelineCoreViewModelBase BaseTimeline
        {
            get { return this._stackings.First(); }
        }

        public TimelineCoreViewModelBase CurrentForegroundTimeline
        {
            get { return this._stackings.Last(); }
        }

        #region Binding Property

        private bool _isQueryValid = true;
        public bool IsQueryValid
        {
            get { return this._isQueryValid; }
            set
            {
                this._isQueryValid = value;
                RaisePropertyChanged(() => IsQueryValid);
            }
        }

        private string _queryError = String.Empty;
        public string QueryError
        {
            get { return _queryError; }
            set
            {
                this._queryError = value;
                RaisePropertyChanged(() => QueryError);
            }
        }

        private volatile string _queryTextBuffer = String.Empty;
        public string QueryText
        {
            get { return this._queryTextBuffer; }
            set
            {
                this._queryTextBuffer = value;
                RaisePropertyChanged(() => QueryText);
                Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(Setting.Instance.TimelineExperienceProperty.QueryApplyWait);
                        if (this._queryTextBuffer == value)
                        {
                            this.AnalyzeCurrentQuery();
                        }
                    });
            }
        }

        private bool _isMouseOver = false;

        public bool IsMouseOver
        {
            get { return _isMouseOver; }
            set
            {
                _isMouseOver = value;
                RaisePropertyChanged(() => IsMouseOver);
            }
        }

        private void AnalyzeCurrentQuery()
        {
            var cf = this.CurrentForegroundTimeline as TimelineListViewModel;
            if (cf == null) return;
            if (this.TimelinesCount == 1)
                this._queryTextBuffer = String.Empty;
            IEnumerable<IFilter> filter = null;
            var prefix = (this._queryTextBuffer.Length > 2 && this._queryTextBuffer[1] == ':') ?
                this._queryTextBuffer[0] : '\0';
            switch (prefix)
            {
                case 'u':
                case 'U':
                    // search with user
                    // tokenizing
                    var uqsplitted = this._queryTextBuffer.Substring(2)
                        .Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Split(new[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries));
                    filter = uqsplitted.Select(s => new FilterCluster()
                    {
                        ConcatenateAnd = true,
                        Filters = s.Select(un => new FilterUser(un)).ToArray()
                    }).ToArray();
                    break;
                case 't':
                case 'T':
                    // search text
                    var tsplitted = this._queryTextBuffer.Substring(2)
                        .Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Split(new[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries));
                    filter = tsplitted.Select(s => new FilterCluster()
                    {
                        ConcatenateAnd = true,
                        Filters = s.Select(un => new Filter.Filters.Text.FilterText(un)).ToArray()
                    }).ToArray();
                    break;
                case 'q':
                case 'Q':
                    // search with query
                    try
                    {
                        filter = new[] { QueryCompiler.ToFilter(this._queryTextBuffer.Substring(2)) };
                    }
                    catch (Exception e)
                    {
                        this.IsQueryValid = false;
                        this.QueryError = e.Message;
                        return;
                    }
                    break;
                default:
                    var tosplitted = this._queryTextBuffer
                        .Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Split(new[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries));
                    filter = tosplitted.Select(s => new FilterCluster()
                    {
                        ConcatenateAnd = true,
                        Filters = s.Select(un => new Filter.Filters.Text.FilterText(un)).ToArray()
                    }).ToArray();
                    break;
            }
            this.IsQueryValid = true;
            CreateTabFromTopTimelineCommand.RaiseCanExecuteChanged();
            cf.TimelineListCoreViewModel.Sources = filter;
            Task.Factory.StartNew(() => cf.TimelineListCoreViewModel.InvalidateCache(true));
        }

        private void WritebackQuery()
        {
            var cf = this.CurrentForegroundTimeline as TimelineListViewModel;
            if (cf == null) return;
            if (this.TimelinesCount == 1)
                this._queryTextBuffer = String.Empty;
            if (cf.TimelineListCoreViewModel.Sources == null)
                this._queryTextBuffer = String.Empty;
            else if (cf.TimelineListCoreViewModel.Sources.All(
                c => (c is FilterCluster) && ((FilterCluster)c).Filters
                    .All(f => f is Filter.Filters.Text.FilterText)))
            {
                // t:
                this._queryTextBuffer =
                    cf.TimelineListCoreViewModel.Sources.Select(
                    c => ((FilterCluster)c).Filters.Select(f =>
                        ((FilterText)f).Needle).JoinString(" ")).JoinString("|");
            }
            else if (cf.TimelineListCoreViewModel.Sources.All(
                c => (c is FilterCluster) && ((FilterCluster)c).Filters
                    .All(f => f is FilterUser)))
            {
                // u:
                this._queryTextBuffer = "u:" +
                    cf.TimelineListCoreViewModel.Sources.Select(
                    c => ((FilterCluster)c).Filters.Select(f =>
                        ((FilterUser)f).Needle).JoinString(" ")).JoinString("|");
            }
            else
            {
                this._queryTextBuffer = "q:(" +
                    cf.TimelineListCoreViewModel.Sources.Select(
                    s => s.ToQuery()).JoinString("|") + ")";
            }
            RaisePropertyChanged(() => QueryText);
        }

        public int TimelinesCount
        {
            get { return this._stackings.Count; }
        }

        public bool IsContainsSingle
        {
            get { return this._stackings.Count == 1; }
        }

        public bool IsStackTopUserPage
        {
            get { return this.CurrentForegroundTimeline is UserPageViewModel; }
        }

        #endregion

        /// <summary>
        /// 現在のスタックトップタイムラインを削除します。
        /// </summary>
        /// <param name="removeAllStackings">スタックされているすべてのタイムラインを削除します。</param>
        public void RemoveTopTimeline(bool removeAllStackings)
        {
            if (this._stackings.Count == 1) return;
            do
            {
                this._stackings.RemoveAt(this._stackings.Count - 1);
            } while (removeAllStackings && this._stackings.Count > 2);
            this._stackings.ForEach(t => t.InvalidateIsActive());
            RaisePropertyChanged(() => TimelinesCount);
            RaisePropertyChanged(() => IsContainsSingle);
            RaisePropertyChanged(() => IsStackTopUserPage);
            WritebackQuery();
            this.IsQueryValid = true;
        }

        /// <summary>
        /// スタックトップタイムラインを追加します。
        /// </summary>
        /// <param name="newFilter"></param>
        /// <param name="description"></param>
        public void AddTopTimeline(IEnumerable<IFilter> newFilter)
        {
            if (this._stackings.Count > 0 && newFilter == null)
                throw new ArgumentNullException("スタックベースとなる時以外、デフォルトフィルタがNULLのタイムラインを追加できません。");
            this._stackings.Add(new TimelineListViewModel(this, newFilter));
            this._stackings.ForEach(t => t.InvalidateIsActive());
            RaisePropertyChanged(() => TimelinesCount);
            RaisePropertyChanged(() => IsContainsSingle);
            RaisePropertyChanged(() => IsStackTopUserPage);
            WritebackQuery();
        }

        public void AddTopUser(string userId)
        {
            this._stackings.Add(new UserPageViewModel(this, userId));
            this._stackings.ForEach(t => t.InvalidateIsActive());
            RaisePropertyChanged(() => TimelinesCount);
            RaisePropertyChanged(() => IsContainsSingle);
            RaisePropertyChanged(() => IsStackTopUserPage);
        }

        #region ClearNewTweetsCountCommand
        DelegateCommand _ClearNewTweetsCountCommand;

        public DelegateCommand ClearNewTweetsCountCommand
        {
            get
            {
                if (_ClearNewTweetsCountCommand == null)
                    _ClearNewTweetsCountCommand = new DelegateCommand(ClearNewTweetsCount);
                return _ClearNewTweetsCountCommand;
            }
        }

        private void ClearNewTweetsCount()
        {
            this.NewTweetsCount = 0;
        }
        #endregion

        #region CreateTopTimelineCommand
        DelegateCommand _CreateTopTimelineCommand;

        public DelegateCommand CreateTopTimelineCommand
        {
            get
            {
                if (_CreateTopTimelineCommand == null)
                    _CreateTopTimelineCommand = new DelegateCommand(CreateTopView);
                return _CreateTopTimelineCommand;
            }
        }

        private void CreateTopView()
        {
            this.AddTopTimeline(new[] { new FilterCluster() });
            this.Messenger.Raise(new InteractionMessage("FocusToSearch"));
        }
        #endregion

        #region CreateTabFromTopTimelineCommand
        DelegateCommand _CreateTabFromTopTimelineCommand;

        public DelegateCommand CreateTabFromTopTimelineCommand
        {
            get
            {
                if (_CreateTabFromTopTimelineCommand == null)
                    _CreateTabFromTopTimelineCommand = new DelegateCommand(CreateTabFromTopTimeline, CanCreateTabFromTopTimeline);
                return _CreateTabFromTopTimelineCommand;
            }
        }

        private bool CanCreateTabFromTopTimeline()
        {
            return IsQueryValid;
        }

        private void CreateTabFromTopTimeline()
        {
            this.Parent.AddTab(
                new TabProperty()
                {
                    Name = this.QueryText,
                    TweetSources = this.CurrentForegroundTimeline.CoreViewModel.Sources
                });
            this.RemoveTopTimeline(false);
        }
        #endregion

        #region RemoveTopTimelineCommand
        DelegateCommand _RemoveTopTimelineCommand;

        public DelegateCommand RemoveTopTimelineCommand
        {
            get
            {
                if (_RemoveTopTimelineCommand == null)
                    _RemoveTopTimelineCommand = new DelegateCommand(RemoveTopTimeline);
                return _RemoveTopTimelineCommand;
            }
        }

        private void RemoveTopTimeline()
        {
            this.RemoveTopTimeline(false);
        }
        #endregion

        internal void NotifyNewTweetReceived(TimelineListCoreViewModel timelineListCoreViewModel, TimelineChild.TweetViewModel tweetViewModel)
        {
            if (AccountStorage.Contains(tweetViewModel.Status.User.ScreenName) || !this.IsAlive)
                return;

            if (Setting.Instance.NotificationProperty.TabNotifyStackTopTimeline ?
                this.CurrentForegroundTimeline.CoreViewModel == timelineListCoreViewModel :
                this.BaseTimeline.CoreViewModel == timelineListCoreViewModel)
            {
                this.NewTweetsCount++;
                if (this.TabProperty.IsNotifyEnabled)
                {
                    if (String.IsNullOrEmpty(this.TabProperty.NotifySoundPath))
                        NotificationCore.QueueNotify(tweetViewModel);
                    else
                        NotificationCore.QueueNotify(tweetViewModel, this.TabProperty.NotifySoundPath);
                }
            }
        }
    }
}
