using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Inscribe.Configuration;
using Inscribe.Filter;
using Inscribe.Filter.Filters.Numeric;
using Inscribe.Subsystems;
using Inscribe.ViewModels.Behaviors.Messaging;
using Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild;
using Livet;
using Livet.Messaging;
using System.Threading.Tasks;

namespace Inscribe.ViewModels.PartBlocks.MainBlock
{
    public class ColumnOwnerViewModel : ViewModel
    {
        #region Closed tab stack control

        private object _ctsLock = new object();

        private Stack<TabViewModel> _closedTabStacks = new Stack<TabViewModel>();

        /// <summary>
        /// 閉じタブスタックにタブをプッシュします。<para />
        /// IsAliveのステート変更は行いますが、クエリやリストの購読解除は呼び出し側で行う必要があります。
        /// </summary>
        public void PushClosedTabStack(TabViewModel viewmodel)
        {
            // タブを殺す
            viewmodel.IsAlive = false;
            lock (this._ctsLock)
            {
                this._closedTabStacks.Push(viewmodel);
            }
            this._columns.ForEach(c => c.RebirthTabCommand.RaiseCanExecuteChanged());
        }

        /// <summary>
        /// 閉じタブスタックからタブをポップします。<para />
        /// IsAliveのステート変更は行いますが、クエリやリストの再購読は呼び出し側で行う必要があります。
        /// </summary>
        public TabViewModel PopClosedTab()
        {
            TabViewModel ret;
            lock (this._ctsLock)
            {
                ret = this._closedTabStacks.Pop();
            }
            this._columns.ForEach(c => c.RebirthTabCommand.RaiseCanExecuteChanged());
            // タブをブッ生き返す
            ret.IsAlive = true;
            return ret;
        }

        /// <summary>
        /// 閉じタブスタックにタブが存在するかを返します。
        /// </summary>
        public bool IsExistedClosedTab()
        {
            lock (this._ctsLock)
            {
                return this._closedTabStacks.Count > 0;
            }
        }

        /// <summary>
        /// 閉じタブスタックをクリアします。
        /// </summary>
        public void ClearClosedTab()
        {
            lock (this._ctsLock)
            {
                this._closedTabStacks.Clear();
            }
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

        #region KeyAssignCore

        public void RegisterKeyAssign()
        {
            // Moving focus
            KeyAssignCore.RegisterOperation("FocusToTimeline", this.SetFocus);
            KeyAssignCore.RegisterOperation("MoveLeft", () => MoveHorizontal(false, false));
            KeyAssignCore.RegisterOperation("MoveLeftColumn", () => MoveHorizontal(false, true));
            KeyAssignCore.RegisterOperation("MoveRight", () => MoveHorizontal(true, false));
            KeyAssignCore.RegisterOperation("MoveRightColumn", () => MoveHorizontal(true, true));
            KeyAssignCore.RegisterOperation("MoveDown", () => MoveVertical(ListSelectionKind.SelectBelow));
            KeyAssignCore.RegisterOperation("MoveUp", () => MoveVertical(ListSelectionKind.SelectAbove));
            KeyAssignCore.RegisterOperation("MoveTop", () => MoveVertical(ListSelectionKind.SelectFirst));
            KeyAssignCore.RegisterOperation("MoveBottom", () => MoveVertical(ListSelectionKind.SelectLast));
            KeyAssignCore.RegisterOperation("Deselect", () => MoveVertical(ListSelectionKind.Deselect));

            // Timeline action
            KeyAssignCore.RegisterOperation("Mention", () => ExecTVMAction(vm => vm.MentionCommand.Execute()));
            KeyAssignCore.RegisterOperation("MentionMulti", () => ExecTVMAction(vm =>
            {
                vm.MentionCommand.Execute();
                this.SetFocus();
            }));
            KeyAssignCore.RegisterOperation("SendDirectMessage", () => ExecTVMAction(vm => vm.DirectMessageCommand.Execute()));
            KeyAssignCore.RegisterOperation("FavoriteAndRetweet", () => ExecTVMAction(vm =>
            {
                vm.Favorite();
                vm.Retweet();
            }));
            KeyAssignCore.RegisterOperation("FavoriteThisTabAll", () => ExecTabAction(vm =>
            {
                var items = vm.StackingTimelines.First().CoreViewModel.TweetsSource.ToArray();
                var cms = new ConfirmationMessage(
                    "タブ " + vm.TabProperty.Name + " に存在するすべてのステータスをFavoriteに追加します。" + Environment.NewLine +
                    "(このタブには" + items.Length + "ステータスが存在します)" + Environment.NewLine +
                    "よろしいですか？", "全Favoriteの警告", System.Windows.MessageBoxImage.Warning, System.Windows.MessageBoxButton.OKCancel, "Confirm");
                this.Messenger.Raise(cms);
                if (cms.Response.GetValueOrDefault())
                    items.ForEach(tdtvm => tdtvm.Favorite());
            }));
            KeyAssignCore.RegisterOperation("Favorite", () => ExecTVMAction(vm => vm.ToggleFavorite()));
            KeyAssignCore.RegisterOperation("FavoriteCreate", () => ExecTVMAction(vm => vm.Favorite()));
            KeyAssignCore.RegisterOperation("FavoriteDelete", () => ExecTVMAction(vm => vm.Unfavorite()));
            KeyAssignCore.RegisterOperation("FavoriteMulti", () => ExecTVMAction(vm => vm.FavoriteMultiUserCommand.Execute()));
            KeyAssignCore.RegisterOperation("Retweet", () => ExecTVMAction(vm => vm.ToggleRetweet()));
            KeyAssignCore.RegisterOperation("RetweetCreate", () => ExecTVMAction(vm => vm.Retweet()));
            KeyAssignCore.RegisterOperation("RetweetDelete", () => ExecTVMAction(vm => vm.Unretweet()));
            KeyAssignCore.RegisterOperation("RetweetMulti", () => ExecTVMAction(vm => vm.RetweetMultiUserCommand.Execute()));
            KeyAssignCore.RegisterOperation("UnofficialRetweet", () => ExecTVMAction(vm => vm.UnofficialRetweetCommand.Execute()));
            KeyAssignCore.RegisterOperation("QuoteTweet", () => ExecTVMAction(vm => vm.QuoteCommand.Execute()));
            KeyAssignCore.RegisterOperation("Delete", () => ExecTVMAction(vm => vm.DeleteCommand.Execute()));
            KeyAssignCore.RegisterOperation("Mute", () => ExecTVMAction(vm => vm.MuteCommand.Execute()));
            KeyAssignCore.RegisterOperation("ReportForSpam", () => ExecTVMAction(vm => vm.ReportForSpamCommand.Execute()));
            KeyAssignCore.RegisterOperation("ShowConversation", () => ExecTVMAction(vm => vm.OpenConversationCommand.Execute()));
            KeyAssignCore.RegisterOperation("CreateSelectedUserTab", () => ExecTVMAction(vm => CreateUserTab(vm, false)));
            KeyAssignCore.RegisterOperation("CreateSelectedUserColumn", () => ExecTVMAction(vm => CreateUserTab(vm, true)));
            KeyAssignCore.RegisterOperation("OpenTweetWeb", () => ExecTVMAction(vm => vm.Tweet.ShowTweetCommand.Execute()));
            KeyAssignCore.RegisterOperation("ShowUserDetail", () => ExecTVMAction(vm => vm.ShowUserDetailCommand.Execute()));
            KeyAssignCore.RegisterOperation("CopySTOT", () => ExecTVMAction(vm => vm.Tweet.CopySTOTCommand.Execute()));
            KeyAssignCore.RegisterOperation("CopyWebUrl", () => ExecTVMAction(vm => vm.Tweet.CopyWebUrlCommand.Execute()));
            KeyAssignCore.RegisterOperation("CopyScreenName", () => ExecTVMAction(vm => vm.Tweet.CopyScreenNameCommand.Execute()));

            // Tab Action
            KeyAssignCore.RegisterOperation("Search", () => ExecTabAction(vm =>
            {
                vm.AddTopTimeline(new[] { new FilterCluster() });
                vm.Messenger.Raise(new InteractionMessage("FocusToSearch"));
            }));
            KeyAssignCore.RegisterOperation("RemoveViewStackTop", () => ExecTabAction(vm => vm.RemoveTopTimeline(false)));
            KeyAssignCore.RegisterOperation("RemoveViewStackAll", () => ExecTabAction(vm => vm.RemoveTopTimeline(true)));
            KeyAssignCore.RegisterOperation("CreateTab", () => ExecTabAction(vm => vm.Parent.AddNewTabCommand.Execute()));
            KeyAssignCore.RegisterOperation("CloseTab", () => ExecTabAction(vm => vm.Parent.CloseTab(vm)));
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

        private void CreateUserTab(TabDependentTweetViewModel tvm, bool newColumn)
        {
            var filter = new[] { new FilterUserId(tvm.Tweet.Status.User.NumericId) };
            var desc = "@" + tvm.Tweet.Status.User.ScreenName;
            if (newColumn)
            {
                var column = tvm.Parent.Parent.Parent.CreateColumn();
                column.AddTab(new Configuration.Tabs.TabProperty() { Name = desc, TweetSources = filter });
            }
            else
            {
                tvm.Parent.Parent.AddTab(new Configuration.Tabs.TabProperty() { Name = desc, TweetSources = filter });
            }
        }

        #endregion
    }

    public enum ColumnsLocation
    {
        Next,
        Previous
    }
}
