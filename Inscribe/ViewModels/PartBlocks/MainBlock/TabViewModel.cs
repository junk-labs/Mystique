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
using Inscribe.Filter.Filters.Text;
using Inscribe.ViewModels.MainBlock;
using Livet;
using Livet.Commands;
using Livet.Messaging;
using Inscribe.Filter.Filters.ScreenName;

namespace Inscribe.ViewModels.PartBlocks.MainBlock
{
    public class TabViewModel : ViewModel
    {
        public ColumnViewModel Parent { get; private set; }

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
            }
        }

        private bool _isMouseOver = false;
        public bool IsMouseOver
        {
            get { return this._isMouseOver; }
            set
            {
                this._isMouseOver = value;
                RaisePropertyChanged(() => IsMouseOver);
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

        public TabViewModel(ColumnViewModel parent, TabProperty property = null)
        {
            this.Parent = parent;

            this._tabProperty = property ?? new TabProperty();
            ViewModelHelper.BindNotification(this._tabProperty.LinkAccountInfoChangedEvent, this, (o, e) =>
                this._timelines.SelectMany(vm => vm.TweetsSource).ForEach(t => t.PendingColorChanged(true)));

            this.AddTopTimeline(null);
        }

        public void SetTabOwner(ColumnViewModel newParent)
        {
            this.Parent = newParent;
        }

        public void Commit(bool reinvalidate)
        {
            this._timelines.ForEach(f => f.Commit(reinvalidate));
        }

        private ObservableCollection<TimelineListViewModel> _timelines = new ObservableCollection<TimelineListViewModel>();
        public IEnumerable<TimelineListViewModel> Timelines
        {
            get { return this._timelines; }
        }

        public TimelineListViewModel CurrentForegroundTimeline
        {
            get { return this._timelines.Last(); }
        }

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

        private void AnalyzeCurrentQuery()
        {
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
                    filter = uqsplitted.Select(s => new FilterCluster(){
                        ConcatenateAnd = true,
                        Filters = s.Select(un => new FilterUser(un)).ToArray()}).ToArray();
                    break;
                case 't':
                case 'T':
                    // search text
                    var tsplitted = this._queryTextBuffer.Substring(2)
                        .Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Split(new[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries));
                    filter = tsplitted.Select(s => new FilterCluster(){
                        ConcatenateAnd = true,
                        Filters = s.Select(un => new Filter.Filters.Text.FilterText(un)).ToArray()}).ToArray();
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
                    filter = tosplitted.Select(s => new FilterCluster(){
                        ConcatenateAnd = true,
                        Filters = s.Select(un => new Filter.Filters.Text.FilterText(un)).ToArray()}).ToArray();
                    break;
            }
            this.IsQueryValid = true;
            this.CurrentForegroundTimeline.Sources = filter;
            Task.Factory.StartNew(() => this.CurrentForegroundTimeline.RefreshCache())
                .ContinueWith(_ => DispatcherHelper.BeginInvoke(() => this.CurrentForegroundTimeline.Commit(true)));
        }

        private void WritebackQuery()
        {
            if (this.TimelinesCount == 1)
                this._queryTextBuffer = String.Empty;
            if (this.CurrentForegroundTimeline.Sources == null)
                this._queryTextBuffer = String.Empty;
            else if(this.CurrentForegroundTimeline.Sources.All(
                c => (c is FilterCluster) && ((FilterCluster)c).Filters
                    .All(f => f is Filter.Filters.Text.FilterText)))
            {
                // t:
                this._queryTextBuffer =
                    this.CurrentForegroundTimeline.Sources.Select(
                    c => ((FilterCluster)c).Filters.Select(f =>
                        ((FilterText)f).Needle).JoinString(" ")).JoinString("|");
            }
            else if (this.CurrentForegroundTimeline.Sources.All(
                c => (c is FilterCluster) && ((FilterCluster)c).Filters
                    .All(f => f is FilterUser)))
            {
                // u:
                this._queryTextBuffer = "u:" +
                    this.CurrentForegroundTimeline.Sources.Select(
                    c => ((FilterCluster)c).Filters.Select(f =>
                        ((FilterUser)f).Needle).JoinString(" ")).JoinString("|");
            }
            else
            {
                this._queryTextBuffer = "q:(" +
                    this.CurrentForegroundTimeline.Sources.Select(
                    s => s.ToQuery()).JoinString("|") + ")";
            }
            RaisePropertyChanged(() => QueryText);
        }

        public int TimelinesCount
        {
            get { return this._timelines.Count; }
        }

        public bool IsContainsSingle
        {
            get { return this._timelines.Count == 1; }
        }

        /// <summary>
        /// 現在のスタックトップタイムラインを削除します。
        /// </summary>
        /// <param name="removeAllStackings">スタックされているすべてのタイムラインを削除します。</param>
        public void RemoveTopTimeline(bool removeAllStackings)
        {
            do
            {
                this._timelines.RemoveAt(this._timelines.Count - 1);
            } while (removeAllStackings && this._timelines.Count > 2);
            this._timelines.ForEach(t => t.InvalidateIsActive());
            RaisePropertyChanged(() => TimelinesCount);
            RaisePropertyChanged(() => IsContainsSingle);
            WritebackQuery();
        }

        /// <summary>
        /// スタックトップタイムラインを追加します。
        /// </summary>
        /// <param name="newFilter"></param>
        /// <param name="description"></param>
        public void AddTopTimeline(IEnumerable<IFilter> newFilter)
        {
            this._timelines.Add(new TimelineListViewModel(this, newFilter));
            this._timelines.ForEach(t => t.InvalidateIsActive());
            RaisePropertyChanged(() => TimelinesCount);
            RaisePropertyChanged(() => IsContainsSingle);
            WritebackQuery();
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
            this.AddTopTimeline(null);
            this.Messenger.Raise(new InteractionMessage("FocusToSearch"));
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

    }
}
