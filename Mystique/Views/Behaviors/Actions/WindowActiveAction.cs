using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet.Behaviors.Messaging;
using System.Windows;

namespace Mystique.Views.Behaviors.Actions
{
    public class WindowActiveAction : InteractionMessageAction<Window>
    {
        protected override void InvokeAction(Livet.Messaging.InteractionMessage m)
        {
            this.AssociatedObject.Activate();
        }
    }
}
