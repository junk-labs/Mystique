using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet;
using Inscribe.Configuration.Tabs;

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
        }

        private TweetViewModel _selectedTweetViewModel = null;
        public TweetViewModel SelectedTweetViewModel
        {
            get { return _selectedTweetViewModel; }
            set { this._selectedTweetViewModel = value; }
        }

    }
}
