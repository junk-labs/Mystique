using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using Inscribe.Configuration;
using Inscribe.Configuration.KeyAssignment;
using Inscribe.ViewModels.Behaviors.Messaging;
using Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild;
using Livet;

namespace Inscribe.ViewModels.PartBlocks.MainBlock
{
    public class ColumnOwnerViewModel : ViewModel
    {
        #region Closed tab stack control

        private ConcurrentStack<TabViewModel> _closedTabStacks = new ConcurrentStack<TabViewModel>();

        public void PushClosedTabStack(TabViewModel viewmodel)
        {
            this._closedTabStacks.Push(viewmodel);
            this._columns.ForEach(c => c.RebirthTabCommand.RaiseCanExecuteChanged());
        }

        public TabViewModel PopClosedTab()
        {
            TabViewModel ret;
            this._closedTabStacks.TryPop(out ret);
            this._columns.ForEach(c => c.RebirthTabCommand.RaiseCanExecuteChanged());
            return ret;
        }

        public bool IsExistedClosedTab()
        {
            return this._closedTabStacks.Count > 0;
        }

        public void ClearClosedTab()
        {
            this._closedTabStacks.Clear();
            this._columns.ForEach(c => c.RebirthTabCommand.RaiseCanExecuteChanged());
        }

        #endregion

        public MainWindowViewModel Parent { get; private set; }

        public ColumnOwnerViewModel(MainWindowViewModel parent)
        {
            this.Parent = parent;
            this.CurrentFocusColumn = CreateColumn();
            Setting.Instance.StateProperty.TabPropertyProvider = () => Columns.Select(cvm => cvm.TabItems.Select(s => s.TabProperty));
            RegisterKeyAssign();
        }

        private ObservableCollection<ColumnViewModel> _columns = new ObservableCollection<ColumnViewModel>();
        public ObservableCollection<ColumnViewModel> Columns { get { return this._columns; } }

        public int ColumnIndexOf(ColumnViewModel vm)
        {
            return this._columns.IndexOf(vm);
        }

        public ColumnViewModel CreateColumn(int index = -1)
        {
            var nvm = new ColumnViewModel(this);
            if (index == -1 || index >= this._columns.Count)
                this._columns.Add(nvm);
            else
                this._columns.Insert(index, nvm);
            // ColumnViewModelのイベントを購読。
            // このイベントはリリースの必要がない
            nvm.GotFocus += (o, e) => CurrentFocusColumn = nvm;
            nvm.SelectedTabChanged += _ => RaisePropertyChanged(() => CurrentTab);
            return nvm;
        }

        /// <summary>
        /// タブが一つも含まれないカラムを削除します。
        /// </summary>
        public void GCColumn()
        {
            this._columns.Where(c => c.TabItems.Count() == 0).ToArray()
                .ForEach(v => this._columns.Remove(v));
            // 少なくとも1つのカラムは必要
            if (this._columns.Count == 0)
                CreateColumn();
        }

        private ColumnViewModel _currentFocusColumn = null;
        public ColumnViewModel CurrentFocusColumn
        {
            get { return this._currentFocusColumn; }
            protected set
            {
                this._currentFocusColumn = value;
                RaisePropertyChanged(() => CurrentFocusColumn);
                RaisePropertyChanged(() => CurrentTab);
                this._columns.SelectMany(c => c.TabItems).ForEach(vm => vm.UpdateIsCurrentFocused());
                var cfc = this.CurrentColumnChanged;
                if (cfc != null)
                    cfc(value);
                var ctab = this.CurrentTabChanged;
                if (ctab != null)
                    ctab(value.SelectedTabViewModel);
            }
        }

        public event Action<ColumnViewModel> CurrentColumnChanged;

        public TabViewModel CurrentTab
        {
            get
            {
                if (this.CurrentFocusColumn == null)
                    return null;
                return this.CurrentFocusColumn.SelectedTabViewModel;
            }
        }

        public event Action<TabViewModel> CurrentTabChanged;

        public void MoveFocusTo(ColumnViewModel column, ColumnsLocation moveTarget)
        {
            if (_columns.Count == 0)
                throw new InvalidOperationException("No columns existed.");
            int i = _columns.IndexOf(column);
            if (i == -1)
                return;
            switch (moveTarget)
            {
                case ColumnsLocation.Next:
                    if (i == _columns.Count - 1)
                        _columns[0].SetFocus();
                    else
                        _columns[i + 1].SetFocus();
                    break;
                case ColumnsLocation.Previous:
                    if (i == 0)
                        _columns[_columns.Count - 1].SetFocus();
                    else
                        _columns[i - 1].SetFocus();
                    break;
            }
        }

        public void SetFocus()
        {
            this.CurrentFocusColumn.SetFocus();
        }

        #region KeyAssign

        public void RegisterKeyAssign()
        {
            // Moving focus
            KeyAssign.RegisterOperation("FocusToTimeline", this.SetFocus);
            KeyAssign.RegisterOperation("MoveLeft", () => MoveHorizontal(false, false));
            KeyAssign.RegisterOperation("MoveLeftColumn", () => MoveHorizontal(false, true));
            KeyAssign.RegisterOperation("MoveRight", () => MoveHorizontal(true, false));
            KeyAssign.RegisterOperation("MoveRightColumn", () => MoveHorizontal(true, true));
            KeyAssign.RegisterOperation("MoveDown", () => MoveVertical(ListSelectionKind.SelectBelow));
            KeyAssign.RegisterOperation("MoveUp", () => MoveVertical(ListSelectionKind.SelectAbove));
            KeyAssign.RegisterOperation("MoveTop", () => MoveVertical(ListSelectionKind.SelectFirst));
            KeyAssign.RegisterOperation("MoveBottom", () => MoveVertical(ListSelectionKind.SelectLast));
            KeyAssign.RegisterOperation("Deselect", () => MoveVertical(ListSelectionKind.Deselect));

            // Timeline action
            KeyAssign.RegisterOperation("Mention", () => ExecTVMAction(vm => vm.MentionCommand.Execute()));
            KeyAssign.RegisterOperation("SendDirectMessage", () => ExecTVMAction(vm => vm.DirectMessageCommand.Execute()));
            KeyAssign.RegisterOperation("Favorite", ()=>ExecTVMAction(vm => vm.FavoriteCommand.Execute()));
            KeyAssign.RegisterOperation("FavoriteMulti", ()=>ExecTVMAction(vm => vm.FavoriteMultiUserCommand.Execute()));
            KeyAssign.RegisterOperation("Retweet", ()=>ExecTVMAction(vm => vm.RetweetCommand.Execute()));
            KeyAssign.RegisterOperation("RetweetMulti", ()=>ExecTVMAction(vm => vm.RetweetMultiUserCommand.Execute()));
            KeyAssign.RegisterOperation("UnofficialRetweet", () => ExecTVMAction(vm => vm.UnofficialRetweetCommand.Execute()));
            KeyAssign.RegisterOperation("QuoteTweet", () => ExecTVMAction(vm => vm.QuoteCommand.Execute()));
            KeyAssign.RegisterOperation("Delete", () => ExecTVMAction(vm => vm.DeleteCommand.Execute()));
            KeyAssign.RegisterOperation("Mute", () => ExecTVMAction(vm => vm.MuteCommand.Execute()));
            KeyAssign.RegisterOperation("ReportForSpam", () => ExecTVMAction(vm => vm.ReportForSpamCommand.Execute()));
            KeyAssign.RegisterOperation("ShowConversation", () => ExecTVMAction(vm => vm.OpenConversationCommand.Execute()));
            KeyAssign.RegisterOperation("CreateSelectedUserTab", () => ExecTVMAction(vm => vm.CreateUserTabCommand.Execute()));
            KeyAssign.RegisterOperation("OpenTweetWeb", () => ExecTVMAction(vm => vm.Tweet.ShowTweetCommand.Execute()));
            KeyAssign.RegisterOperation("ShowUserDetail", () => ExecTVMAction(vm => vm.ShowUserDetailCommand.Execute()));

            // Tab Action
            KeyAssign.RegisterOperation("Search", () => ExecTabAction(vm => vm.AddTopTimeline(null)));
            KeyAssign.RegisterOperation("RemoveViewStackTop", () => ExecTabAction(vm => vm.RemoveTopTimeline(false)));
            KeyAssign.RegisterOperation("RemoveViewStackAll", () => ExecTabAction(vm => vm.RemoveTopTimeline(true)));
            KeyAssign.RegisterOperation("CreateTab", () => ExecTabAction(vm => vm.Parent.AddNewTabCommand.Execute()));
            KeyAssign.RegisterOperation("CloseTab", () => ExecTabAction(vm => vm.Parent.CloseTab(vm)));
        }

        private void MoveHorizontal(bool directionRight, bool moveInColumn)
        {
            var cc = this.CurrentFocusColumn;
            if (cc == null) return;
            int idxofc = this.Columns.IndexOf(cc);
            int idx = cc.TabItems.IndexOf(cc.SelectedTabViewModel);
            if (directionRight)
            {
                idx++;
                // →
                if (idx >= cc.TabItems.Count || moveInColumn)
                {
                    // move column
                    idx = 0;
                    idxofc++;
                    idxofc %= this.Columns.Count;
                    this.CurrentFocusColumn = this.Columns[idxofc];
                    this.CurrentFocusColumn.SelectedTabViewModel = this.CurrentFocusColumn.TabItems[0];
                }
                else
                {
                    cc.SelectedTabViewModel = cc.TabItems[idx];
                }
            }
            else
            {
                // ←
                idx--;
                if (idx < 0 || moveInColumn)
                {
                    idx = 0;
                    idxofc--;
                    idxofc += this.Columns.Count;
                    idxofc %= this.Columns.Count;
                    this.CurrentFocusColumn = this.Columns[idxofc];
                    this.CurrentFocusColumn.SelectedTabViewModel = this.CurrentFocusColumn.TabItems[this.CurrentFocusColumn.TabItems.Count - 1];
                }
                else
                {
                    cc.SelectedTabViewModel = cc.TabItems[idx];
                }
            }
        }

        private void MoveVertical(ListSelectionKind selectKind)
        {
            var cc = this.CurrentTab == null ? null : this.CurrentTab.CurrentForegroundTimeline;
            if (selectKind == ListSelectionKind.SelectAbove && Setting.Instance.TimelineExperienceProperty.MoveAboveTopToDeselect)
                selectKind = ListSelectionKind.SelectAboveAndNull;
            if (cc != null)
                cc.SetSelect(selectKind);
        }

        private void ExecTabAction(Action<TabViewModel> action)
        {
            if (this.CurrentTab != null)
                action(this.CurrentTab);
        }

        private void ExecTVMAction(Action<TabDependentTweetViewModel> action)
        {
            var cc = this.CurrentTab != null ? this.CurrentTab.CurrentForegroundTimeline : null;
            if (cc == null) return;
            var vm = cc.SelectedTweetViewModel;
            if (vm == null) return;
            action(vm);
        }

        #endregion
    }

    public enum ColumnsLocation
    {
        Next,
        Previous
    }
}
