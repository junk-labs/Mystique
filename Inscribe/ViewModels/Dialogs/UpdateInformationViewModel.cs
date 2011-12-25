using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;
using Inscribe.Common;
using Inscribe.Configuration;

namespace Inscribe.ViewModels.Dialogs
{
    public class UpdateInformationViewModel : ViewModel
    {
        double currentVersion;
        double previousVersion;

        public UpdateInformationViewModel()
        {
            currentVersion = Define.GetNumericVersion();
            previousVersion = Setting.Instance.KernelProperty.LastWriteVersion;
        }

        public string UpdateDescription
        {
            get { return "test string."; }
        }

        #region CloseCommand
        private ViewModelCommand _CloseCommand;

        public ViewModelCommand CloseCommand
        {
            get
            {
                if (_CloseCommand == null)
                {
                    _CloseCommand = new ViewModelCommand(Close);
                }
                return _CloseCommand;
            }
        }

        public void Close()
        {
            this.Messenger.Raise(new WindowActionMessage("WindowAction", WindowAction.Close));
        }
        #endregion


        #region OpenReleaseNoteCommand
        private ViewModelCommand _OpenReleaseNoteCommand;

        public ViewModelCommand OpenReleaseNoteCommand
        {
            get
            {
                if (_OpenReleaseNoteCommand == null)
                {
                    _OpenReleaseNoteCommand = new ViewModelCommand(OpenReleaseNote);
                }
                return _OpenReleaseNoteCommand;
            }
        }

        public void OpenReleaseNote()
        {
            if (Define.IsNightly())
                Browser.Start(Define.NightlyReleaseNoteUrl);
            else
                Browser.Start(Define.ReleaseNoteUrl);
        }
        #endregion

    }
}
