using System.Linq;
using System.Windows.Threading;
using Inscribe.Configuration;
using Inscribe.Core;
using Inscribe.Storage;
using Inscribe.ViewModels.PartBlocks.InputBlock;
using Inscribe.ViewModels.PartBlocks.NotifyBlock;
using Inscribe.ViewModels.Timeline;
using Livet;
using Livet.Commands;
using Inscribe.ViewModels.PartBlocks.BlockCommon;

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
