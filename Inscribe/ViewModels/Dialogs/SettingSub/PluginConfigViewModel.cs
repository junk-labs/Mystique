using Acuerdo.Plugin;
using Livet;
using Inscribe.ViewModels.Behaviors.Messaging;
using System.Collections.Generic;
using System.Linq;
using Inscribe.Plugin;

namespace Inscribe.ViewModels.Dialogs.SettingSub
{
    public class PluginConfigViewModel : ViewModel
    {
        public PluginConfigViewModel()
        {
            this.PluginViewModels = PluginManager.Plugins.Select(p => new PluginViewModel(p)).ToArray();
        }

        public IEnumerable<PluginViewModel> PluginViewModels { get; set; }
    }

    public class PluginViewModel : ViewModel
    {
        IPlugin plugin;

        public PluginViewModel(IPlugin plugin)
        {
            this.plugin = plugin;
        }

        public string Name { get { return this.plugin.Name; } }

        public string Version { get { return this.plugin.Version.ToString("0.0"); } }

        #region OpenConfigurationCommand
        Livet.Commands.ViewModelCommand _OpenConfigurationCommand;

        public Livet.Commands.ViewModelCommand OpenConfigurationCommand
        {
            get
            {
                if (_OpenConfigurationCommand == null)
                    _OpenConfigurationCommand = new Livet.Commands.ViewModelCommand(OpenConfiguration, CanOpenConfiguration);
                return _OpenConfigurationCommand;
            }
        }

        private bool CanOpenConfiguration()
        {
            return plugin.ConfigurationInterface != null;
        }

        private void OpenConfiguration()
        {
            var ci = plugin.ConfigurationInterface;
            if (ci == null) return;
            this.Messenger.Raise(new TransitionExMessage(ci.GetTransitionWindow(), Livet.Messaging.TransitionMode.Modal, "OpenConfig"));
        }
        #endregion
    }
}
