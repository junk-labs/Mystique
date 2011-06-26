using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Livet;
using Livet.Command;
using System.Linq;
using Inscribe.Configuration.Tabs;

namespace Inscribe.ViewModels
{
    public class ColumnViewModel : ViewModel
    {
        public ColumnOwnerViewModel Parent { get; private set; }

        public ColumnViewModel(ColumnOwnerViewModel parent)
        {
            this.Parent = parent;
        }

        private bool _isFocusContains = false;
        public bool IsFocusContains
        {
            get { return this._isFocusContains; }
            set
            {
                this._isFocusContains = value;
                RaisePropertyChanged(() => IsFocusContains);
            }
        }

        private bool _isDragOver = false;
        public bool IsDragOver
        {
            get { return this._isDragOver; }
            set
            {
                this._isDragOver = value;
                RaisePropertyChanged(() => IsDragOver);
            }
        }

        private ObservableCollection<TabViewModel> _tabItems = new ObservableCollection<TabViewModel>();
        public IEnumerable<TabViewModel> TabItems { get { return this._tabItems; } }

        private TabViewModel _selectedTabViewModel = null;
        public TabViewModel SelectedTabViewModel
        {
            get { return this._selectedTabViewModel; }
            set
            {
                if (this._selectedTabViewModel != null)
                    this._selectedTabViewModel.IsSelected = false;
                this._selectedTabViewModel = value;
                if (this._selectedTabViewModel != null)
                    this._selectedTabViewModel.IsSelected = true;
                RaisePropertyChanged(() => SelectedTabViewModel);
            }
        }

        public TabViewModel AddTab(TabProperty tabProperty = null)
        {
            var nvm = new TabViewModel(this, tabProperty);
            this.AddTab(nvm);
            return nvm;
        }

        public void AddTab(TabViewModel tabViewModel)
        {
            tabViewModel.SetTabOwner(this);
            this._tabItems.Add(tabViewModel);
            if (this._tabItems.Count == 1)
                SelectedTabViewModel = tabViewModel;
        }

        public void InsertBefore(TabViewModel insert, TabViewModel beforeThis)
        {
            var idx = this._tabItems.IndexOf(beforeThis);
            if (idx == -1)
                this._tabItems.Add(insert);
            else
                this._tabItems.Insert(idx, insert);
            insert.Commit(true);
        }

        public void CloseTab(TabViewModel tabViewModel)
        {
            RemoveTab(tabViewModel);
        }

        public void RemoveTab(TabViewModel tabViewModel)
        {
            if (this._tabItems.Remove(tabViewModel))
            {
                if (this._tabItems.Count > 0)
                    SelectedTabViewModel = this._tabItems[0];
                else
                    SelectedTabViewModel = null;
            }
        }

        public bool Contains(TabViewModel tabViewModel)
        {
            return this._tabItems.Contains(tabViewModel);
        }

        internal void SetFocus()
        {
        }

        #region DragDropStartCommand
        DelegateCommand _DragDropStartCommand;

        public DelegateCommand DragDropStartCommand
        {
            get
            {
                if (_DragDropStartCommand == null)
                    _DragDropStartCommand = new DelegateCommand(DragDropStart);
                return _DragDropStartCommand;
            }
        }

        private void DragDropStart()
        {
            this.Parent.Columns.ForEach(c => c.IsDragOver = true);
        }
        #endregion

        #region OnDropCommand
        DelegateCommand<DragEventArgs> _OnDropCommand;

        public DelegateCommand<DragEventArgs> OnDropCommand
        {
            get
            {
                if (_OnDropCommand == null)
                    _OnDropCommand = new DelegateCommand<DragEventArgs>(OnDrop);
                return _OnDropCommand;
            }
        }

        private void OnDrop(DragEventArgs parameter)
        {
            var data = parameter.Data.GetData(typeof(TabViewModel)) as TabViewModel;
            if (data == null) return;
            var odo = parameter.OriginalSource as FrameworkElement;
            var tvm = odo != null ? odo.DataContext as TabViewModel : null;
            if (tvm == data) return;
            Parent.Columns.ForEach(cvm => cvm.RemoveTab(data));
            this.InsertBefore(data, tvm);
            this.SelectedTabViewModel = data;
        }
        #endregion

        #region OnDropLeftColumnCommand
        DelegateCommand<DragEventArgs> _OnDropLeftColumnCommand;

        public DelegateCommand<DragEventArgs> OnDropLeftColumnCommand
        {
            get
            {
                if (_OnDropLeftColumnCommand == null)
                    _OnDropLeftColumnCommand = new DelegateCommand<DragEventArgs>(OnDropLeftColumn);
                return _OnDropLeftColumnCommand;
            }
        }

        private void OnDropLeftColumn(DragEventArgs parameter)
        {
            var data = parameter.Data.GetData(typeof(TabViewModel)) as TabViewModel;
            if (data == null) return;
            var idx = this.Parent.ColumnIndexOf(this);
            var target = this.Parent.CreateColumn(idx);
            this.Parent.Columns.ForEach(cvm => cvm.RemoveTab(data));
            target.AddTab(data);
        }
        #endregion

        #region OnDropRightColumnCommand
        DelegateCommand<DragEventArgs> _OnDropRightColumnCommand;

        public DelegateCommand<DragEventArgs> OnDropRightColumnCommand
        {
            get
            {
                if (_OnDropRightColumnCommand == null)
                    _OnDropRightColumnCommand = new DelegateCommand<DragEventArgs>(OnDropRightColumn);
                return _OnDropRightColumnCommand;
            }
        }

        private void OnDropRightColumn(DragEventArgs parameter)
        {
            var data = parameter.Data.GetData(typeof(TabViewModel)) as TabViewModel;
            if (data == null) return;
            this.Parent.Columns.ForEach(c => c.IsDragOver = false);
            var idx = this.Parent.ColumnIndexOf(this);
            var target = this.Parent.CreateColumn(idx + 1);
            this.Parent.Columns.ForEach(cvm => cvm.RemoveTab(data));
            target.AddTab(data);
        }
        #endregion

        #region DragDropFinishCommand
        DelegateCommand _DragDropFinishCommand;

        public DelegateCommand DragDropFinishCommand
        {
            get
            {
                if (_DragDropFinishCommand == null)
                    _DragDropFinishCommand = new DelegateCommand(DragDropFinish);
                return _DragDropFinishCommand;
            }
        }

        private void DragDropFinish()
        {
            this.Parent.Columns.ForEach(c => c.IsDragOver = false);
            this.Parent.GCColumn();
        }
        #endregion

    }
}
