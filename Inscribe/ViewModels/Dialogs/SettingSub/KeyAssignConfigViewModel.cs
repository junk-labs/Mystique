using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet;
using System.IO;
using Inscribe.Configuration;

namespace Inscribe.ViewModels.Dialogs.SettingSub
{
    public class KeyAssignConfigViewModel : ViewModel, IApplyable
    {
        public KeyAssignConfigViewModel()
        {
            KeyAssignIndex = KeyAssignFiles.ToList().IndexOf(Setting.Instance.KeyAssignProperty.KeyAssignFile);
        }

        public String[] KeyAssignFiles
        {
            get
            {
                return Directory.GetFiles(
                    Path.Combine(Path.GetDirectoryName(Define.ExeFilePath),
                    Define.KeyAssignDirectory), "*.xml").Select(s => Path.GetFileName(s))
                    .OrderBy(s => s)
                    .ToArray();
            }
        }

        private int _keyAssignIndex;
        public int KeyAssignIndex
        {
            get { return _keyAssignIndex; }
            set
            {
                _keyAssignIndex = value;
                RaisePropertyChanged(() => KeyAssignIndex);
            }
        }

        public void Apply()
        {
            if (KeyAssignIndex >= 0 && KeyAssignIndex < KeyAssignFiles.Length)
                Setting.Instance.KeyAssignProperty.KeyAssignFile = KeyAssignFiles[KeyAssignIndex];
            else
                Setting.Instance.KeyAssignProperty.KeyAssignFile = "default.xml";
        }
    }
}
