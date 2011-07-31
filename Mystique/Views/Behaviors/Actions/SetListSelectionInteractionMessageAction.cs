using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Livet;
using Livet.Behaviors.Messaging;
using Inscribe.ViewModels.Behaviors.Messaging;
using Inscribe.Configuration;


namespace Mystique.Views.Behaviors.Actions
{
    public class SetListSelectionInteractionMessageAction : InteractionMessageAction<ListBox>
    {
        protected override void InvokeAction(Livet.Messaging.InteractionMessage m)
        {
            var listmsg = m as SetListSelectionMessage;
            if (listmsg == null) return;
            switch (listmsg.ListSelectionKind)
            {
                case ListSelectionKind.Deselect:
                    this.AssociatedObject.SelectedIndex = -1;
                    break;
                case ListSelectionKind.SelectAbove:
                    if (this.AssociatedObject.SelectedIndex > 0)
                        this.AssociatedObject.SelectedIndex--;
                    break;
                case ListSelectionKind.SelectAboveAndNull:
                    if (this.AssociatedObject.SelectedIndex >= 0)
                        this.AssociatedObject.SelectedIndex--;
                    break;
                case ListSelectionKind.SelectBelow:
                    if (this.AssociatedObject.SelectedIndex < this.AssociatedObject.Items.Count - 1)
                        this.AssociatedObject.SelectedIndex++;
                    break;
                case ListSelectionKind.SelectFirst:
                    if (this.AssociatedObject.Items.Count > 0)
                        this.AssociatedObject.SelectedIndex = 0;
                    break;
                case ListSelectionKind.SelectLast:
                    this.AssociatedObject.SelectedIndex = this.AssociatedObject.Items.Count - 1;
                    break;
            }
            this.AssociatedObject.ScrollIntoView(this.AssociatedObject.SelectedItem);
        }
    }
}
