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
using Inscribe.ViewModels.Behaviors.Messaging;
using Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild;
using Livet;
using Inscribe.Threading;
using Inscribe.Subsystems;

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
            Task.Factory.StartNew(() => this.InvalidateCache(true));
        }

        public TimelineListCoreViewModel(TabViewModel parent, IEnumerable<IFilter> sources)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");
            this.Parent = parent;
            this.sources = sources;
            UpdateReacceptionChain(sources, true);
            // binding nortifications
            ViewModelHelper.BindNotification(TweetStorage.TweetStorageChangedEvent, this, TweetStorageChanged);
            ViewModelHelper.BindNotification(Setting.SettingValueChangedEvent, this, SettingValueChanged);
            // Initialize binding timeline
            this._tweetsSource = new CachedConcurrentObservableCollection<TabDependentTweetViewModel>();
            this._tweetCollectionView = new CollectionViewSource();
            this._tweetCollectionView.Source = this._tweetsSource;
            // Generate timeline
            Task.Factory.StartNew(() => InvalidateCache(false))
                .ContinueWith(_ => DispatcherHelper.BeginInvoke(() => UpdateSortDescription()));
        }

        public void Commit(bool reinvalidate)
        {
            this._tweetsSource.Commit(reinvalidate);
        }

        private void TweetStorageChanged(object o, TweetStorageChangedEventArgs e)
        {
            switch (e.ActionKind)
            {
                case TweetActionKind.Added:
                    if (CheckFilters(e.Tweet))
                    {
                        this._tweetsSource.Add(new TabDependentTweetViewModel(e.Tweet, this.Parent));
                        OnNewTweetReceived();
                        if (!AccountStorage.Contains(e.Tweet.Status.User.ScreenName) && this.Parent.IsAlive)
                        {
                            this.Parent.NotifyNewTweetReceived(this);
                            if (this.Parent.TabProperty.IsNotifyEnabled)
                            {
                                if (String.IsNullOrEmpty(this.Parent.TabProperty.NotifySoundPath))
                                    NotificationCore.QueueNotify(e.Tweet);
                                else
                                    NotificationCore.QueueNotify(e.Tweet, this.Parent.TabProperty.NotifySoundPath);
                            }
                        }
                    }
                    break;
                case TweetActionKind.Refresh:
                    Task.Factory.StartNew(() => InvalidateCache(true));
                    break;
                case TweetActionKind.Changed:
                    var tdtvm = new TabDependentTweetViewModel(e.Tweet, this.Parent);
                    var contains = this._tweetsSource.Contains(tdtvm);
                    if (CheckFilters(e.Tweet) != contains)
                    {
                        if (contains)
                            this._tweetsSource.Remove(tdtvm);
                        else
                            this._tweetsSource.Add(tdtvm);
                    }
                    break;
                case TweetActionKind.Removed:
                    this._tweetsSource.Remove(new TabDependentTweetViewModel(e.Tweet, this.Parent));
                    break;
            }
        }

        private void SettingValueChanged(object o, EventArgs e)
        {
            UpdateSortDescription();
        }

        private void UpdateSortDescription()
        {
            using (var disp = this._tweetCollectionView.DeferRefresh())
            {
                this._tweetsSource.Commit();
                this._tweetCollectionView.SortDescriptions.Clear();
                if (Setting.Instance.TimelineExperienceProperty.OrderByAscending)
                    this._tweetCollectionView.SortDescriptions.Add(
                        new SortDescription("Tweet.CreatedAt", ListSortDirection.Ascending));
                else
                    this._tweetCollectionView.SortDescriptions.Add(
                        new SortDescription("Tweet.CreatedAt", ListSortDirection.Descending));
            }
        }

        public bool CheckFilters(TweetViewModel viewModel)
        {
            if (!viewModel.IsStatusInfoContains) return false;
            return (this.sources ?? this.Parent.TabProperty.TweetSources ?? new IFilter[0])
                .Any(f => f.Filter(viewModel.Status));
        }

        public void InvalidateCache(bool commit)
        {
            this._tweetsSource.Clear();
            TweetStorage.GetAll(vm => CheckFilters(vm))
                .Select(tvm => new TabDependentTweetViewModel(tvm, this.Parent))
                .Where(tvm => !this._tweetsSource.Contains(tvm))
                .ForEach(this._tweetsSource.Add);
            if (commit)
                this.Commit(true);
        }

        private CachedConcurrentObservableCollection<TabDependentTweetViewModel> _tweetsSource;
        public CachedConcurrentObservableCollection<TabDependentTweetViewModel> TweetsSource { get { return this._tweetsSource; } }

        private CollectionViewSource _tweetCollectionView;
        public CollectionViewSource TweetCollectionView { get { return this._tweetCollectionView; } }

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

        public void SetSelect(ListSelectionKind kind)
        {
            Messenger.Raise(new SetListSelectionMessage("SetListSelection", kind));
        }
    }
}
