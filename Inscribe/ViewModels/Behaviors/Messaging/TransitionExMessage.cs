using System.Windows;
using Livet;
using Livet.Messaging;

namespace Inscribe.ViewModels.Behaviors.Messaging
{
    public class TransitionExMessage : InteractionMessage
    {
        public TransitionExMessage(Window transitionTarget, TransitionMode mode, string messageKey)
            :base(messageKey)
        {
            this.Target = transitionTarget;
            this.Mode = mode;
        }

        public Window Target { get; set; }

        public TransitionMode Mode { get; set; }

        protected override Freezable CreateInstanceCore()
        {
            return new TransitionExMessage(this.Target, this.Mode, this.MessageKey);
        }
    }
}
