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
            Inscribe.Communication.CruiseControl.AutoCruiseSchedulerManager.Begin();
        }

        private InputBlockViewModel _inputBlockViewModel = new InputBlockViewModel();
        public InputBlockViewModel InputBlockViewModel
        {
            get { return this._inputBlockViewModel; }
        }

        private ColumnOwnerViewModel _columnOwnerViewModel = new ColumnOwnerViewModel();
        public ColumnOwnerViewModel ColumnOwnerViewModel
        {
            get { return this._columnOwnerViewModel; }
        }

        private NotifyBlockViewModel _notifyBlockViewModel = new NotifyBlockViewModel();
        public NotifyBlockViewModel NotifyBlockViewModel
        {
            get { return this._notifyBlockViewModel; }
        }


        #region AuthCommand
        DelegateCommand _AuthCommand;

        public DelegateCommand AuthCommand
        {
            get
            {
                if (_AuthCommand == null)
                    _AuthCommand = new DelegateCommand(Auth);
                return _AuthCommand;
            }
        }

        private void Auth()
        {
            DispatcherHelper.BeginInvoke(() =>
                {
                    var vm = new Mystique.ViewModels.Dialogs.Account.AuthenticateViewModel();
                    this.Messenger.Raise(new TransitionMessage(vm, TransitionMode.Modal, "AccountInfo"));
                    if (vm.Success)
                    {
                        AccountStorage.RegisterAccount(vm.GetAccountInfo());
                    }
                });
        }
        #endregion
      
    }
}
