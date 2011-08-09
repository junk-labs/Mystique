using System.Windows;
using Livet.Behaviors.Messaging;

namespace Mystique.Views.Behaviors.Actions
{
    public class FrameworkSetFocusAction : InteractionMessageAction<FrameworkElement>
    {
        protected override void InvokeAction(Livet.Messaging.InteractionMessage m)
        {
            this.AssociatedObject.Focus();
        }
    }
}
