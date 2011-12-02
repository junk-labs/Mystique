using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Livet.Behaviors.Messaging;
using Inscribe.ViewModels.Behaviors.Messaging;

namespace Mystique.Views.Behaviors.Actions
{
    public class RichTextBoxAction : InteractionMessageAction<RichTextBox>
    {
        protected override void InvokeAction(Livet.Messaging.InteractionMessage m)
        {
            var rtbm = m as RichTextBoxMessage;
            if (rtbm == null) return;
            switch (rtbm.ActionType)
            {
                case RichTextActionType.Copy:
                    this.AssociatedObject.Copy();
                    break;
                case RichTextActionType.SelectAll:
                    this.AssociatedObject.SelectAll();
                    break;
            }
        }
    }
}
