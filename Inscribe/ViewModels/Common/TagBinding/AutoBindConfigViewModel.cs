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
using Inscribe.Configuration;
using Inscribe.Configuration.Settings;
using System.Collections.ObjectModel;

namespace Inscribe.ViewModels.Common.TagBinding
{
    public class AutoBindConfigViewModel : ViewModel
    {
        public AutoBindConfigViewModel()
        {
            Setting.Instance.InputExperienceProperty.HashtagAutoBindDescriptions
                .OrderBy(d => d.TagText)
                .Select(d => new HashtagAutoBindViewModel(d, this))
                .ForEach(vm => _autoBindDatas.Add(vm));
        }

        public ObservableCollection<HashtagAutoBindViewModel> _autoBindDatas = new ObservableCollection<HashtagAutoBindViewModel>();
        public IEnumerable<HashtagAutoBindViewModel> AutoBindDatas
        {
            get { return _autoBindDatas; }
        }

        public void RemoveAutoBind(HashtagAutoBindViewModel vm)
        {
            _autoBindDatas.Remove(vm);
            WritebackToSetting();
        }


        #region AddAutoBindCommand
        DelegateCommand _AddAutoBindCommand;

        public DelegateCommand AddAutoBindCommand
        {
            get
            {
                if (_AddAutoBindCommand == null)
                    _AddAutoBindCommand = new DelegateCommand(AddAutoBind);
                return _AddAutoBindCommand;
            }
        }

        private void AddAutoBind()
        {
            var edit = new AutoBindElementEditorViewModel();
            this.Messenger.Raise(new TransitionMessage(edit, "EditBindElement"));
            if (edit.ReturnValue)
            {
                this._autoBindDatas.Add(new HashtagAutoBindViewModel(edit.GetDescription(), this));
                WritebackToSetting();
            }
        }
        #endregion

        public void WritebackToSetting()
        {
            Setting.Instance.InputExperienceProperty.HashtagAutoBindDescriptions =
                this._autoBindDatas.Select(vm => vm.Description).ToArray();
        }
    }

    public class HashtagAutoBindViewModel : ViewModel
    {
        public HashtagAutoBindViewModel(HashtagAutoBindDescription description, AutoBindConfigViewModel parent)
        {
            this.Parent = parent;
            this.Description = description;
        }

        public AutoBindConfigViewModel Parent { get; private set; }

        public HashtagAutoBindDescription Description { get; private set; }

        #region EditCommand
        DelegateCommand _EditCommand;

        public DelegateCommand EditCommand
        {
            get
            {
                if (_EditCommand == null)
                    _EditCommand = new DelegateCommand(Edit);
                return _EditCommand;
            }
        }

        private void Edit()
        {
            var edit = new AutoBindElementEditorViewModel(this.Description);
            this.Parent.Messenger.Raise(new TransitionMessage(edit, "EditBindElement"));
            if (edit.ReturnValue)
            {
                this.Description = edit.GetDescription();
                RaisePropertyChanged(() => Description);
                this.Parent.WritebackToSetting();
            }
        }
        #endregion

        #region DeleteCommand
        DelegateCommand _DeleteCommand;

        public DelegateCommand DeleteCommand
        {
            get
            {
                if (_DeleteCommand == null)
                    _DeleteCommand = new DelegateCommand(Delete);
                return _DeleteCommand;
            }
        }

        private void Delete()
        {
            this.Parent.RemoveAutoBind(this);
        }
        #endregion
    }
}
