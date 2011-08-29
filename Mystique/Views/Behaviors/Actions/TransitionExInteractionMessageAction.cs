using System.Windows;
using Inscribe.ViewModels.Behaviors.Messaging;
using Livet.Behaviors.Messaging;
using Livet.Messaging;

namespace Mystique.Views.Behaviors.Actions
{
    /// <summary>
    /// 画面遷移を行います。Viewもコードから指定します。
    /// </summary>
    public class TransitionExInteractionMessageAction : InteractionMessageAction<FrameworkElement>
    {
        protected override void InvokeAction(Livet.Messaging.InteractionMessage m)
        {
            var txm = m as TransitionExMessage;
            if (txm == null) return;
            var targetWindow = txm.Target;
            var mode = txm.Mode;
            switch (txm.Mode)
            {
                case TransitionMode.Normal:
                case TransitionMode.Modal:
                    targetWindow.Owner = Window.GetWindow(this.AssociatedObject);

                    if (mode == TransitionMode.Normal)
                    {
                        targetWindow.Show();
                    }
                    else
                    {
                        targetWindow.ShowDialog();
                    }

                    break;
            }
        }
    }
}
