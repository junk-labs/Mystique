using System;
using System.Collections.Generic;
using System.Linq;
using Inscribe.Model;
using Inscribe.ViewModels.Common;
using Livet;
using Livet.Commands;

namespace Inscribe.ViewModels.PartBlocks.ModalParts
{
    public class UserSelectionViewModel : ViewModel
    {
        public UserSelectionViewModel()
        {
            this._userSelectorViewModel = new UserSelectorViewModel();
            this._userSelectorViewModel.LinkChanged += () =>
            {
                RaisePropertyChanged(() => SelectedUsers);
                OkCommand.RaiseCanExecuteChanged();
            };
        }

        public event Action Finished;
        private void OnFinished()
        {
            var handler = Finished;
            if (handler != null)
                handler();
        }

        private Action<IEnumerable<AccountInfo>> curInteract = null;

        private SelectionKind _kind = SelectionKind.Favorite;

        public void BeginInteraction(SelectionKind kind, IEnumerable<AccountInfo> defaultSelect, Action<IEnumerable<AccountInfo>> interact)
        {
            this._kind = kind;
            curInteract = interact;
            _userSelectorViewModel.LinkElements = defaultSelect;
            RaisePropertyChanged(() => IsFavorite);
            RaisePropertyChanged(() => IsRetweet);
            RaisePropertyChanged(() => SelectedUsers);
            OkCommand.RaiseCanExecuteChanged();
        }

        public bool IsFavorite
        {
            get { return this._kind == SelectionKind.Favorite; }
        }

        public bool IsRetweet
        {
            get { return this._kind == SelectionKind.Retweet; }
        }

        private UserSelectorViewModel _userSelectorViewModel;
        public UserSelectorViewModel UserSelectorViewModel
        {
            get { return _userSelectorViewModel; }
        }

        public string SelectedUsers
        {
            get
            {
                return this._userSelectorViewModel.LinkElements.Select(a => "@" + a.ScreenName).JoinString(", ");
            }
        }

        #region OkCommand
        DelegateCommand _OkCommand;

        public DelegateCommand OkCommand
        {
            get
            {
                if (_OkCommand == null)
                    _OkCommand = new DelegateCommand(Ok);
                return _OkCommand;
            }
        }

        private void Ok()
        {
            var invoke = curInteract;
            curInteract = null;
            invoke(_userSelectorViewModel.LinkElements.ToArray());
            OnFinished();
        }
        #endregion

        #region CancelCommand
        DelegateCommand _CancelCommand;

        public DelegateCommand CancelCommand
        {
            get
            {
                if (_CancelCommand == null)
                    _CancelCommand = new DelegateCommand(Cancel);
                return _CancelCommand;
            }
        }

        private void Cancel()
        {
            OnFinished();
        }
        #endregion
    }

    public enum SelectionKind
    {
        Favorite,
        Retweet,
    }
}
