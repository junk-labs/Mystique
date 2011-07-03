using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Livet.Behaviors.Messaging;
using Mystique.Views.Behaviors.Messages;

namespace Mystique.Views.Behaviors.Actions
{
    public class TextBoxSetCaretAction :InteractionMessageAction<TextBox>
    {
        protected override void InvokeAction(Livet.Messaging.InteractionMessage m)
        {
            var scm = m as TextBoxSetCaretMessage;
            if (scm == null) return;
            this.AssociatedObject.SelectionStart = scm.SelectionStart;
            this.AssociatedObject.SelectionLength = scm.SelectionLength;
        }
    }
}
