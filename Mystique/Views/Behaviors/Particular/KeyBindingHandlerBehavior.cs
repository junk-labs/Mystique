using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interactivity;

namespace Mystique.Views.Behaviors.Particular
{
    public class KeyBindingHandlerBehavior : Behavior<FrameworkElement>
    {


        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.KeyDown += new System.Windows.Input.KeyEventHandler(AssociatedObject_KeyDown);
            this.AssociatedObject.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(AssociatedObject_PreviewKeyDown);
        }

        void AssociatedObject_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            throw new NotImplementedException();
        }

        void AssociatedObject_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }
    }
}
