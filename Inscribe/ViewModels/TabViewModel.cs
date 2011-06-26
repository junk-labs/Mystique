using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Inscribe.Configuration;
using Inscribe.Configuration.Tabs;
using Inscribe.Data;
using Inscribe.Filter;
using Inscribe.Storage;
using Livet;
using Livet.Command;
using System.ComponentModel;

namespace Inscribe.ViewModels
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
            this._tweetsSource = new CachedConcurrentObservableCollection<TabDependentTweetViewModel>();
            this._tweetCollectionView = new CollectionViewSource();
            this._tweetCollectionView.Source = this._tweetsSource;
            this._tabProperty = property ?? new TabProperty();
            ViewModelHelper.BindNotification(TweetStorage.Notificator, this, TweetStorageChanged);
            ViewModelHelper.BindNotification(Setting.SettingValueChangedEvent, this, SettingValueChanged);
            Task.Factory.StartNew(() => InvalidateAll())
                .ContinueWith(_ => DispatcherHelper.BeginInvoke(() => UpdateSortDescription()));
        }

        public void SetTabOwner(ColumnViewModel newParent)
        {
            this.Parent = newParent;
        }

        private void TweetStorageChanged(object o, TweetStorageChangedEventArgs e)
        {
            switch (e.ActionKind)
            {
                case TweetActionKind.Added:
                    if (CheckFilters(e.Tweet))
                    {
                        this._tweetsSource.Add(new TabDependentTweetViewModel(e.Tweet, this));
                        this.NewTweetsCount++;
                    }
                    break;
                case TweetActionKind.Refresh:
                    Task.Factory.StartNew(InvalidateAll);
                    break;
                case TweetActionKind.Changed:
                    var tdtvm = new TabDependentTweetViewModel(e.Tweet, this);
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
                    this._tweetsSource.Remove(new TabDependentTweetViewModel(e.Tweet, this));
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
                if (Setting.Instance.TimelineExperienceProperty.AscendingSort)
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
            return (this._tabProperty.TweetSources ?? new IFilter[0]).Any(f => f.Filter(viewModel.Status));
        }

        public void InvalidateAll()
        {
            TweetStorage.GetAll(vm => CheckFilters(vm))
                .Select(tvm => new TabDependentTweetViewModel(tvm, this))
                .Where(tvm => !this._tweetsSource.Contains(tvm))
                .ForEach(this._tweetsSource.Add);
        }

        public void Commit(bool reinvalidate)
        {
            this._tweetsSource.Commit(reinvalidate);
        }

        private CachedConcurrentObservableCollection<TabDependentTweetViewModel> _tweetsSource;
        public ICollection<TabDependentTweetViewModel> TweetsSource { get { return this._tweetsSource; } }

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
            }
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
    }
}
