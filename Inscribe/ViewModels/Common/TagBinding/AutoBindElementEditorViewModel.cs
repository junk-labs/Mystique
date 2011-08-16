using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.Configuration.Settings;
using Livet;
using Livet.Commands;
using Livet.Messaging.Windows;

namespace Inscribe.ViewModels.Common.TagBinding
{
    public class AutoBindElementEditorViewModel : ViewModel
    {
        public AutoBindElementEditorViewModel(HashtagAutoBindDescription desc = null)
        {
            this.ReturnValue = false;
            if (desc != null)
            {
                _tagText = desc.TagText;
                _argumentText = desc.ConditionText;
                _negate = desc.IsNegateCondition;
                _strategyIndex = (int)desc.Strategy;
            }
        }

        public HashtagAutoBindDescription GetDescription()
        {
            return new HashtagAutoBindDescription()
            {
                TagText = this.TagText.Trim().TrimStart('#'),
                ConditionText = this.ArgumentText,
                Strategy = (AutoBindStrategy)StrategyIndex,
                IsNegateCondition = this.Negate
            };
        }

        private string _tagText = null;
        public string TagText
        {
            get { return _tagText ?? String.Empty; }
            set
            {
                _tagText = value;
                RaisePropertyChanged(() => TagText);
            }
        }

        private string _argumentText = null;
        public string ArgumentText
        {
            get { return _argumentText ?? String.Empty; }
            set
            {
                _argumentText = value;
                RaisePropertyChanged(() => ArgumentText);
            }
        }

        private int _strategyIndex = 0;
        public int StrategyIndex
        {
            get { return _strategyIndex; }
            set
            {
                _strategyIndex = value;
                RaisePropertyChanged(() => StrategyIndex);
            }
        }

        private bool _negate = false;
        public bool Negate
        {
            get { return _negate; }
            set
            {
                _negate = value;
                RaisePropertyChanged(() => Negate);
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

        public bool ReturnValue { get; private set; }

        private void Ok()
        {
            ReturnValue = true;
            this.Messenger.Raise(new WindowActionMessage("WindowAction", WindowAction.Close));
        }
        #endregion
      
    }
}