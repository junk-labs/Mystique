using System.Linq;
using Inscribe.Configuration;
using Inscribe.Filter;
using Inscribe.ViewModels.Common.Filter;
using Livet;
using Inscribe.Storage;

namespace Inscribe.ViewModels.Dialogs.SettingSub
{
    public class MuteConfigViewModel : ViewModel, IApplyable
    {
        public MuteConfigViewModel()
        {
            if (Setting.Instance.TimelineFilteringProperty.MuteFilterCluster.ConcatenateAnd ||
                Setting.Instance.TimelineFilteringProperty.MuteFilterCluster.Negate)
                this._filterEditorViewModel = new FilterEditorViewModel(
                    new[] { Setting.Instance.TimelineFilteringProperty.MuteFilterCluster });
            else
                this._filterEditorViewModel = new FilterEditorViewModel(
                    Setting.Instance.TimelineFilteringProperty.MuteFilterCluster.Filters.ToArray());
            this._muteBlockedUsers = Setting.Instance.TimelineFilteringProperty.MuteBlockedUsers;
        }

        private FilterEditorViewModel _filterEditorViewModel;
        public FilterEditorViewModel FilterEditorViewModel
        {
            get { return _filterEditorViewModel; }
        }

        private bool _muteBlockedUsers;
        public bool MuteBlockedUsers
        {
            get { return _muteBlockedUsers; }
            set
            {
                _muteBlockedUsers = value;
                RaisePropertyChanged(() => MuteBlockedUsers);
            }
        }

        public void Apply()
        {
            Setting.Instance.TimelineFilteringProperty.MuteFilterCluster =
                new FilterCluster() { Filters = this._filterEditorViewModel.RootFilters };
            TweetStorage.UpdateMute();
            Setting.Instance.TimelineFilteringProperty.MuteBlockedUsers = this._muteBlockedUsers;
        }
    }
}
