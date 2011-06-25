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
        private InputBlockViewModel _inputBlockViewModel = new InputBlockViewModel();
        public InputBlockViewModel InputBlockViewModel
        {
            get { return this._inputBlockViewModel; }
        }

        private NotifyBlockViewModel _notifyBlockViewModel = new NotifyBlockViewModel();
        public NotifyBlockViewModel NotifyBlockViewModel
        {
            get { return this._notifyBlockViewModel; }
        }
    }
}
