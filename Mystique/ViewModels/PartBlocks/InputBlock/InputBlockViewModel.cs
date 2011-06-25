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
using Mystique.ViewModels.Common;
using Inscribe.ViewModels;

namespace Mystique.ViewModels.PartBlocks.InputBlock
{
    public class InputBlockViewModel : ViewModel
    {
        public InputBlockViewModel()
        {
            this._ImageStackingViewViewModel = new ImageStackingViewViewModel();
            this._UserSelectorViewModel = new UserSelectorViewModel();
            ViewModelHelper.BindNotification(this.UserSelectorViewModel.LinkChangedEvent, this, (o, e) =>
                {
                    ImageStackingViewViewModel.ImageUrls = this.UserSelectorViewModel.LinkElements.
                       Select(ai => ai.UserViewModel.TwitterUser.ProfileImage).ToArray();
                });
        }

        UserSelectorViewModel _UserSelectorViewModel;

        public UserSelectorViewModel UserSelectorViewModel
        {
            get { return _UserSelectorViewModel; }
        }

        ImageStackingViewViewModel _ImageStackingViewViewModel;

        public ImageStackingViewViewModel ImageStackingViewViewModel
        {
            get { return _ImageStackingViewViewModel; }
        }

        public void SetCurrentTab(TabViewModel tvm)
        {
            this.UserSelectorViewModel.LinkElements = tvm.TabProperty.LinkAccountInfos;
            ImageStackingViewViewModel.ImageUrls = this.UserSelectorViewModel.LinkElements.
               Select(ai => ai.UserViewModel.TwitterUser.ProfileImage).ToArray();
        }

        public void SetInReplyTo(TweetViewModel tweet)
        {

        }

        #region State control

        private bool _isOpenInput =false;
        public bool IsOpenInput
        {
            get { return this._isOpenInput; }
            private set
            {
                this._isOpenInput = value;
                RaisePropertyChanged(() => IsOpenInput);
            }
        }

        public void SetOpenText(bool isOpen, bool trasitionFocus = false)
        {
            this.IsOpenInput = isOpen;
            if (trasitionFocus && isOpen)
                this.Messenger.Raise(new InteractionMessage("FocusToText"));
        }

        #endregion

        #region Commands

        #region OpenInputCommand
        DelegateCommand _OpenInputCommand;

        public DelegateCommand OpenInputCommand
        {
            get
            {
                if (_OpenInputCommand == null)
                    _OpenInputCommand = new DelegateCommand(OpenInput);
                return _OpenInputCommand;
            }
        }

        private void OpenInput()
        {
            SetOpenText(true, true);
        }
        #endregion


        #region CloseInputCommand
        DelegateCommand _CloseInputCommand;

        public DelegateCommand CloseInputCommand
        {
            get
            {
                if (_CloseInputCommand == null)
                    _CloseInputCommand = new DelegateCommand(CloseInput);
                return _CloseInputCommand;
            }
        }

        private void CloseInput()
        {
            SetOpenText(false);
        }
        #endregion
      

        #endregion

    }
}
