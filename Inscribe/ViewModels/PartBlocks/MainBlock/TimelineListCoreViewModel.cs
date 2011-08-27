using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Inscribe.Configuration;
using Inscribe.Data;
using Inscribe.Filter;
using Inscribe.Storage;
using Inscribe.Threading;
using Inscribe.ViewModels.Behaviors.Messaging;
using Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild;
using Livet;

namespace Inscribe.ViewModels.PartBlocks.MainBlock
{
    public class TimelineListCoreViewModel : ViewModel
    {
        public TabViewModel Parent { get; private set; }

        private static QueueTaskDispatcher _updateDispatcher;
        static TimelineListCoreViewModel()
        {
            _updateDispatcher = new QueueTaskDispatcher(6);
            ThreadHelper.Halt += _updateDispatcher.Dispose;
        }

        public event EventHandler NewTweetReceived;
        private void OnNewTweetReceived()
        {
            var handler = NewTweetReceived;
            if (handler != null)
                NewTweetReceived(this, EventArgs.Empty);
        }

        private IEnumerable<IFilter> sources;
        public IEnumerable<IFilter> Sources
        {
            get { return this.sources; }
            set
            {
                if (this.sources == value) return;
                var prev = this.sources;
                this.sources = (value ?? new IFilter[0]).ToArray();
                UpdateReacceptionChain(prev, false);
                UpdateReacceptionChain(value, true);
                RaisePropertyChanged(() => Sources);
            }
        }

        private bool _referenced = false;

        private CachedConcurrentObservableCollection<TabDependentTweetViewModel> _tweetsSource;
        public CachedConcurrentObservableCollection<TabDependentTweetViewModel> TweetsSource
        {
            get
            {
                if (!_referenced)
                {
                    this._tweetsSource.Commit(true);
                    _referenced = true;
                }
                return this._tweetsSource;
            }
        }

        /*
        private CollectionViewSource _tweetCollectionView;
        public CollectionViewSource TweetCollectionView { get { return this._tweetCollectionView; } }
        */

        private TabDependentTweetViewModel _selectedTweetViewModel = null;
        public TabDependentTweetViewModel SelectedTweetViewModel
        {
            get { return _selectedTweetViewModel; }
            set
            {
                this._selectedTweetViewModel = value;
                RaisePropertyChanged(() => SelectedTweetViewModel);
                Task.Factory.StartNew(() => this._tweetsSource.ToArrayVolatile()
                    .ForEach(vm => _updateDispatcher.Enqueue(() => vm.PendingColorChanged())));
            }
        }

        private void UpdateReacceptionChain(IEnumerable<IFilter> filter, bool join)
        {
            if (filter != null)
            {
                if (join)
                    filter.ForEach(f => f.RequireReaccept += f_RequireReaccept);
                else
                    filter.ForEach(f => f.RequireReaccept -= f_RequireReaccept);
            }
        }

        void f_RequireReaccept()
        {
            System.Diagnostics.Debug.WriteLine("received reacception");
            Task.Factory.StartNew(() => this.InvalidateCache());
        }

        public TimelineListCoreViewModel(TabViewModel parent, IEnumerable<IFilter> sources)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");
            this._tweetsSource = new CachedConcurrentObservableCollection<TabDependentTweetViewModel>();
            this.Parent = parent;
            this.sources = sources;
            UpdateReacceptionChain(sources, true);
            // binding nortifications
            ViewModelHelper.BindNotification(TweetStorage.TweetStorageChangedEvent, this, TweetStorageChanged);
            ViewModelHelper.BindNotification(Setting.SettingValueChangedEvent, this, SettingValueChanged);
            // Generate timeline
            Task.Factory.StartNew(() => this.InvalidateCache());
        }

        public void Commit()
        {
            this._tweetsSource.Sort(Setting.Instance.TimelineExperienceProperty.OrderByAscending,
                t => t.Tweet.CreatedAt);
        }

        private void TweetStorageChanged(object o, TweetStorageChangedEventArgs e)
        {
            switch (e.ActionKind)
            {
                case TweetActionKind.Added:
                    if (CheckFilters(e.Tweet))
                    {
                        var atdtvm = new TabDependentTweetViewModel(e.Tweet, this.Parent);
                        if (Setting.Instance.TimelineExperienceProperty.UseIntelligentOrdering &&
                            DateTime.Now.Subtract(e.Tweet.CreatedAt).TotalSeconds < Setting.Instance.TimelineExperienceProperty.IntelligentOrderingThresholdSec)
                        {
                            if (Setting.Instance.TimelineExperienceProperty.OrderByAscending)
                                this._tweetsSource.AddLastSingle(atdtvm);
                            else
                                this._tweetsSource.AddTopSingle(atdtvm);
                        }
                        else
                        {
                            this._tweetsSource.AddOrderedSingle(
                                atdtvm, Setting.Instance.TimelineExperienceProperty.OrderByAscending,
                                t => t.Tweet.CreatedAt);
                        }
                        OnNewTweetReceived();
                        this.Parent.NotifyNewTweetReceived(this, e.Tweet);
                    }
                    break;
                case TweetActionKind.Refresh:
                    Task.Factory.StartNew(() => InvalidateCache());
                    break;
                case TweetActionKind.Changed:
                    var ctdtvm = new TabDependentTweetViewModel(e.Tweet, this.Parent);
                    var contains = this._tweetsSource.Contains(ctdtvm);
                    if (CheckFilters(e.Tweet) != contains)
                    {
                        if (contains)
                            this._tweetsSource.Remove(ctdtvm);
                        else
                            this._tweetsSource.Add(ctdtvm);
                    }
                    break;
                case TweetActionKind.Removed:
                    this._tweetsSource.Remove(new TabDependentTweetViewModel(e.Tweet, this.Parent));
                    break;
            }
        }

        private void SettingValueChanged(object o, EventArgs e)
        {
            this.Commit();
            // UpdateSortDescription();
        }

        public bool CheckFilters(TweetViewModel viewModel)
        {
            if (!viewModel.IsStatusInfoContains) return false;
            return (this.sources ?? this.Parent.TabProperty.TweetSources ?? new IFilter[0])
                .Any(f => f.Filter(viewModel.Status));
        }

        public void InvalidateCache()
        {
            if (DispatcherHelper.UIDispatcher.CheckAccess())
            {
                // ディスパッチャ スレッドではInvalidateCacheを行わない
                throw new InvalidOperationException("Can't invalidate cache on Dispatcher thread.");
            }

            this._tweetsSource.Clear();
            var collection = TweetStorage.GetAll(vm => CheckFilters(vm))
                .Select(tvm => new TabDependentTweetViewModel(tvm, this.Parent)).ToArray();
            foreach (var tvm in collection)
            {
                this._tweetsSource.AddTopSingle(tvm);
            }
            this.Commit();
        }

        public void SetSelect(ListSelectionKind kind)
        {
            Messenger.Raise(new SetListSelectionMessage("SetListSelection", kind, this.SelectedTweetViewModel));
        }
    }
}
