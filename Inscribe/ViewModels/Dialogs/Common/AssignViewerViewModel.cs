using System;
using Livet;
using Inscribe.Subsystems;

namespace Inscribe.ViewModels.Dialogs.Common
{
    public class AssignViewerViewModel : ViewModel
    {
        public string KeyAssignMaps
        {
            get { return KeyAssignCore.GetKeyAssignMaps(); }
        }

        private string _inputKey = String.Empty;

        public string InputKey
        {
            get { return _inputKey; }
            set
            {
                _inputKey = value;
                RaisePropertyChanged(() => InputKey);
            }
        }
    }
}
