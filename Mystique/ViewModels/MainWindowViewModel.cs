using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Command;
using Livet.Messaging;
using Livet.Messaging.File;
using Livet.Messaging.Window;
using Mystique.ViewModels.PartBlocks.NotifyBlock;
using Mystique.ViewModels.PartBlocks.InputBlock;
using Inscribe.Storage;
using Inscribe.ViewModels;
using Mystique.ViewModels.Dialogs.Common;
using System.Windows.Threading;
using Inscribe.Configuration;
using System.Threading.Tasks;

namespace Mystique.ViewModels
{
    /// <summary>
    /// メイン ウィンドウ用ViewModel
    /// </summary>
    /// <remarks>
    /// スタティッククラスのイベントリスニングを行いますが、このViewModelのライフサイクルは
    /// アプリケーションライフサイクルと一致するため、メモリリークの問題を回避できます。
    /// </remarks>
    public class MainWindowViewModel : ViewModel
    {
        public MainWindowViewModel()
        {
            this._columnOwnerViewModel = new ColumnOwnerViewModel();
            this._notifyBlockViewModel = new NotifyBlockViewModel(this);
            // Input block dependents ColumnOwnerViewModel
            this._inputBlockViewModel = new InputBlockViewModel(this);
        }

        private InputBlockViewModel _inputBlockViewModel;
        public InputBlockViewModel InputBlockViewModel
        {
            get { return this._inputBlockViewModel; }
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
            var n = NotifyStorage.NotifyManually("タイムラインを準備しています...");
            DispatcherHelper.BeginInvoke(() =>
            {
                try
                {
                    Setting.Instance.StateProperty.TabInformations.ForEach(c =>
                    {
                        var column = this.ColumnOwnerViewModel.CreateColumn();
                        c.ForEach(p => column.AddTab(p));
                    });
                    this.ColumnOwnerViewModel.GCColumn();
                }
                finally
                {
                    n.Dispose();
                    Inscribe.Communication.CruiseControl.AutoCruiseSchedulerManager.Begin();
                }
            }, DispatcherPriority.ApplicationIdle);
        }
        #endregion

    }
}
