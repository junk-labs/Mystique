using System;
using Inscribe.Configuration.KeyAssignment;
using Livet;

namespace Inscribe.ViewModels.Dialogs.Common
{
    public class AssignViewerViewModel : ViewModel
    {
        public string KeyAssignMaps
        {
            get { return KeyAssign.GetKeyAssignMaps(); }
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
