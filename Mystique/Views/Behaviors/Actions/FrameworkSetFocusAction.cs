using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
