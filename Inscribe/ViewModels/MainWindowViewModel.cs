using System.Linq;
using System.Windows.Threading;
using Inscribe.Configuration;
using Inscribe.Core;
using Inscribe.Storage;
using Inscribe.ViewModels.PartBlocks.InputBlock;
using Inscribe.ViewModels.PartBlocks.NotifyBlock;
using Inscribe.ViewModels.PartBlocks.MainBlock;
using Livet;
using Livet.Commands;
using Inscribe.ViewModels.PartBlocks.BlockCommon;
using Inscribe.ViewModels.PartBlocks.ModalParts;
using System;
using System.Collections.Generic;
using Inscribe.Model;

namespace Inscribe.ViewModels
{
    /// <summary>
    /// メイン ウィンドウ用ViewModel
    /// </summary>
    /// <remarks>
    /// このViewModelのライフサイクルはアプリケーションライフサイクルと一致するため、
    /// Modelのイベントを購読しても、メモリリークの問題を回避できます。
    /// </remarks>
    public class MainWindowViewModel : ViewModel
    {
        public MainWindowViewModel()
        {
            this._columnOwnerViewModel = new ColumnOwnerViewModel(this);
            this._notifierViewModel = new NotifierViewModel();
            this._notifyBlockViewModel = new NotifyBlockViewModel(this);
            // Input block dependents ColumnOwnerViewModel
            this._inputBlockViewModel = new InputBlockViewModel(this);

            this._userSelectionViewModel = new UserSelectionViewModel();
            this._userSelectionViewModel.Finished += () =>
            {
                this._isVisibleUserSelection = false;
                RaisePropertyChanged(() => IsVisibleUserSelection);
                RaisePropertyChanged(() => IsActivateMain);
            };
        }

        public string Title
        {
            get { return Define.ApplicationName + " " + Define.GetFormattedVersion(); }
        }

        private InputBlockViewModel _inputBlockViewModel;
        public InputBlockViewModel InputBlockViewModel
        {
            get { return this._inputBlockViewModel; }
        }

        private NotifierViewModel _notifierViewModel;
        public NotifierViewModel NotifierViewModel
        {
            get { return _notifierViewModel; }
        }

        private ColumnOwnerViewModel _columnOwnerViewModel;
        public ColumnOwnerViewModel ColumnOwnerViewModel
        {
            get { return this._columnOwnerViewModel; }
        }

        private NotifyBlockViewModel _notifyBlockViewModel;
        public NotifyBlockViewModel NotifyBlockViewModel
        {
            get { return this._notifyBlockViewModel; }
        }

        private UserSelectionViewModel _userSelectionViewModel;

        public UserSelectionViewModel UserSelectionViewModel
        {
            get { return _userSelectionViewModel; }
        }

        public bool IsActivateMain
        {
            get { return !this._isVisibleUserSelection; }
        }

        private bool _isVisibleUserSelection = false;
        public bool IsVisibleUserSelection
        {
            get { return _isVisibleUserSelection; }
        }

        public void SelectUser(Action<IEnumerable<AccountInfo>> returning)
        {
            this.UserSelectionViewModel.BeginInteraction(returning);
            _isVisibleUserSelection = true;
            RaisePropertyChanged(() => IsVisibleUserSelection);
            RaisePropertyChanged(() => IsActivateMain);
        }

        #region LoadedCommand
        DelegateCommand _LoadedCommand;

        public DelegateCommand LoadedCommand
        {
            get
            {
                if (_LoadedCommand == null)
                    _LoadedCommand = new DelegateCommand(Loaded);
                return _LoadedCommand;
            }
        }

        private void Loaded()
        {
            if (AccountStorage.Accounts.Count() == 0)
            {
                this.InputBlockViewModel.ShowConfig();
            }
            var n = NotifyStorage.NotifyManually("タブとカラムを読み込んでいます。しばらくお待ちください...");
            DispatcherHelper.BeginInvoke(() =>
            {
                try
                {
                    if (Setting.Instance.StateProperty.TabInformations != null)
                    {
                        Setting.Instance.StateProperty.TabInformations.ForEach(c =>
                        {
                            var column = this.ColumnOwnerViewModel.CreateColumn();
                            c.ForEach(p => column.AddTab(p));
                        });
                        this.ColumnOwnerViewModel.GCColumn();
                    }
                }
                finally
                {
                    n.Dispose();
                    Initializer.StandbyApp();
                }
            }, DispatcherPriority.ApplicationIdle);
        }
        #endregion
    }
}
