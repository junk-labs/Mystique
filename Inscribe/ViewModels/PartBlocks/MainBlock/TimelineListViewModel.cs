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
using Livet.Commands;

namespace Inscribe.ViewModels.PartBlocks.MainBlock
{
    public class TimelineListViewModel : TimelineCoreViewModelBase
    {
        public TabViewModel Parent { get; private set; }

        public override bool IsActive
        {
            get { return Parent.CurrentForegroundTimeline == this; }
        }

        public override void InvalidateIsActive()
        {
            RaisePropertyChanged(() => IsActive);
        }

        public TimelineListViewModel(TabViewModel parent, IEnumerable<IFilter> sources = null)
        {
            this.Parent = parent;
            this._timelineListCoreViewModel = new TimelineListCoreViewModel(parent, sources);
        }

        private TimelineListCoreViewModel _timelineListCoreViewModel;

        public TimelineListCoreViewModel TimelineListCoreViewModel
        {
            get { return _timelineListCoreViewModel; }
        }

        public override void InvalidateCache()
        {
            this._timelineListCoreViewModel.InvalidateCache(true);
        }

        #region GetFocusCommand
        DelegateCommand _GetFocusCommand;

        public DelegateCommand GetFocusCommand
        {
            get
            {
                if (_GetFocusCommand == null)
                    _GetFocusCommand = new DelegateCommand(GetFocus);
                return _GetFocusCommand;
            }
        }

        private void GetFocus()
        {
            this.OnGetFocus();
        }
        #endregion

        public override void SetSelect(ListSelectionKind kind)
        {
            this.TimelineListCoreViewModel.SetSelect(kind);
        }

        public override TabDependentTweetViewModel SelectedTweetViewModel
        {
            get { return this._timelineListCoreViewModel.SelectedTweetViewModel; }
            set { this._timelineListCoreViewModel.SelectedTweetViewModel = value; }
        }
    }
}
