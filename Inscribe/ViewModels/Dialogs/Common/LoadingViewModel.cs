using System;
using System.Threading.Tasks;
using Livet;
using Livet.Commands;
using Livet.Messaging.Windows;

namespace Inscribe.ViewModels.Dialogs.Common
{
    public class LoadingViewModel : ViewModel
    {
        private Action _action;
        private bool _sync;

        public LoadingViewModel(string title, string description, Action action, bool runSynchronous = false)
        {
            this._title = title;
            this._description = description;
            this._action = action;
            this._sync = runSynchronous;
        }

        private string _title;

        public string Title
        {
            get { return _title; }
        }

        private string _description;

        public string Description
        {
            get { return _description; }
        }

        #region LoadedCommand
        DelegateCommand _LoadedCommand;

        public DelegateCommand LoadedCommand
        {
            get
            {
                if (_LoadedCommand == null)
                    _LoadedCommand = new DelegateCommand(Loaded);
                Loaded();
                return _LoadedCommand;
            }
        }

        private void Loaded()
        {
            if (this._sync)
            {
                this._action();
                Messenger.Raise(new WindowActionMessage("Close", WindowAction.Close));
            }
            else
            {
                Task.Factory.StartNew(() => this._action())
                    .ContinueWith(t => Messenger.Raise(new WindowActionMessage("Close", WindowAction.Close)));
            }
        }
        #endregion

    }
}
