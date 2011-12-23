using System;
using Livet;

namespace Inscribe.ViewModels.PartBlocks.InputBlock
{
    public class PluginMenuItem : ViewModel
    {
        private string _name = String.Empty;
        public string Name
        {
            get { return _name ?? String.Empty; }
            set
            {
                _name = value;
                RaisePropertyChanged(() => Name);
            }
        }
        
        public Action ExecuteAction { get; set; }

        public PluginMenuItem(string header, Action command)
        {
            this.Name = header;
            this.ExecuteAction = command;
        }

        #region ExecuteCommand
        private Livet.Commands.ViewModelCommand _ExecuteCommand;

        public Livet.Commands.ViewModelCommand ExecuteCommand
        {
            get
            {
                if (_ExecuteCommand == null)
                {
                    _ExecuteCommand = new Livet.Commands.ViewModelCommand(Execute);
                }
                return _ExecuteCommand;
            }
        }

        public void Execute()
        {
            if (ExecuteAction != null)
                ExecuteAction();
        }
        #endregion

    }
}
