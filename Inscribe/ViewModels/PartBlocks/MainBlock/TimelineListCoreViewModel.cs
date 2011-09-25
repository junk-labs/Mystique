using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inscribe.Common;
using Inscribe.Configuration;
using Inscribe.Data;
using Inscribe.Filter;
using Inscribe.Storage;
using Inscribe.ViewModels.Behaviors.Messaging;
using Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild;
using Livet;
using Dulcet.Twitter;

namespace Inscribe.ViewModels.PartBlocks.MainBlock
{
    public class TimelineListCoreViewModel : ViewModel
    {
        public TabViewModel Parent { get; private set; }

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
                    _referenced = true;
                    this._tweetsSource.Commit(true);
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
                if (this._selectedTweetViewModel == value) return;
                this._selectedTweetViewModel = value;
                RaisePropertyChanged(() => SelectedTweetViewModel);
                Task.Factory.StartNew(() => this._tweetsSource.ToArrayVolatile()
                    .ForEach(vm => vm.PendingColorChanged()));
            }
        }

        private void UpdateReacceptionChain(IEnumerable<IFilter> filter, bool join)
        {
            if (filter != null)
            {
                if (join)
                {
                    filter.ForEach(f => f.RequireReaccept += f_RequireReaccept);
                    filter.ForEach(f => f.RequirePartialReaccept += f_RequirePartialReaccept);
                }
                else
                {
                    filter.ForEach(f => f.RequireReaccept -= f_RequireReaccept);
                    filter.ForEach(f => f.RequirePartialReaccept -= f_RequirePartialReaccept);
                }
            }
        }

        void f_RequireReaccept()
        {
            Task.Factory.StartNew(() => this.InvalidateCache());
        }

        void f_RequirePartialReaccept(TwitterStatusBase tsb)
        {
            var tvm = TweetStorage.Register(tsb);
            if (tvm == null || !TweetStorage.ValidateTweet(tvm)) return;
            if (CheckFilters(tvm))
                AddTweet(tvm);
            else
                RemoveTweet(tvm);
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
                        AddTweet(e.Tweet);
                        OnNewTweetReceived();
                        this.Parent.NotifyNewTweetReceived(this, e.Tweet);
                    }
                    break;
                case TweetActionKind.Refresh:
                    Task.Factory.StartNew(() => InvalidateCache());
                    break;
                case TweetActionKind.Changed:
                    if (CheckFilters(e.Tweet))
                        AddTweet(e.Tweet);
                    else
                        RemoveTweet(e.Tweet);
                        break;
                case TweetActionKind.Removed:
                    RemoveTweet(e.Tweet);
                    break;
            }
        }

        private void AddTweet(TweetViewModel tvm)
        {
            var atdtvm = new TabDependentTweetViewModel(tvm, this.Parent);
            if (this._tweetsSource.Contains(atdtvm)) return;
            if (Setting.Instance.TimelineExperienceProperty.UseIntelligentOrdering &&
                DateTime.Now.Subtract(tvm.CreatedAt).TotalSeconds < Setting.Instance.TimelineExperienceProperty.IntelligentOrderingThresholdSec)
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
        }

        private void RemoveTweet(TweetViewModel tvm)
        {
            this._tweetsSource.Remove(new TabDependentTweetViewModel(tvm, this.Parent));
        }

        private void SettingValueChanged(object o, EventArgs e)
        {
            this.Commit();
            // UpdateSortDescription();
        }

        public bool CheckFilters(TweetViewModel viewModel)
        {
            try
            {
                if (!viewModel.IsStatusInfoContains) return false;
                return (this.sources ?? this.Parent.TabProperty.TweetSources ?? new IFilter[0])
                    .Any(f => f.Filter(viewModel.Status));
            }
            catch (Exception ex)
            {
                ExceptionStorage.Register(ex, ExceptionCategory.InternalError, "フィルタ処理中に内部エラーが発生しました。");
                return false;
            }
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
                this._tweetsSource.AddVolatile(tvm);
            }
            this.Commit();
        }

        public void SetSelect(ListSelectionKind kind)
        {
            Messenger.Raise(new SetListSelectionMessage("SetListSelection", kind, this.SelectedTweetViewModel));
        }
    }
}
