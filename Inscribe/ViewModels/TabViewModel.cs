using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet;
using Inscribe.Configuration.Tabs;
using Inscribe.Data;
using Inscribe.Storage;
using Inscribe.Filter;
using System.Threading.Tasks;

namespace Inscribe.ViewModels
{
    public class TabViewModel : ViewModel
    {
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

        public TabViewModel(TabProperty property = null)
        {
            this._tabProperty = property ?? new TabProperty();
            ViewModelHelper.BindNotification(TweetStorage.Notificator, this, TweetStorageChanged);
        }

        private void TweetStorageChanged(object o, TweetStorageChangedEventArgs e)
        {
            switch (e.ActionKind)
            {
                case TweetActionKind.Added:
                    if (CheckFilters(e.Tweet))
                        this.tweetsSource.Add(e.Tweet);
                    break;
                case TweetActionKind.Refresh:
                    Task.Factory.StartNew(InvalidateAll);
                    break;
                case TweetActionKind.Removed:
                    this.tweetsSource.Remove(e.Tweet);
                    break;
            }
        }

        public bool CheckFilters(TweetViewModel viewModel)
        {
            if (!viewModel.IsStatusInfoContains) return false;
            return (this._tabProperty.TweetSources ?? new IFilter[0]).Any(f => f.Filter(viewModel.Status));
        }

        public void InvalidateAll()
        {
            TweetStorage.GetAll(vm => CheckFilters(vm)).ForEach(this.tweetsSource.Add);
        }

        public void Commit(bool reinvalidate)
        {
            this.tweetsSource.Commit(reinvalidate);
        }

        private CachedConcurrentObservableCollection<TweetViewModel> tweetsSource = new CachedConcurrentObservableCollection<TweetViewModel>();
        public ICollection<TweetViewModel> TweetsSource { get { return this.tweetsSource; } }

        private TweetViewModel _selectedTweetViewModel = null;
        public TweetViewModel SelectedTweetViewModel
        {
            get { return _selectedTweetViewModel; }
            set { this._selectedTweetViewModel = value; }
        }
    }
}
