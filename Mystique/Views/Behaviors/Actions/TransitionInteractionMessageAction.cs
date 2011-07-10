using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet.Messaging;
using System.Windows;
using System.Windows.Interactivity;
using Livet.Behaviors.Messaging;

namespace Mystique.Views.Behaviors.Actions
{
    /// <summary>
    /// 画面遷移(Window)を行うアクションです。<see cref="TransitionMessage"/>に対応します。
    /// </summary>
    public class TransitionInteractionMessageAction : InteractionMessageAction<FrameworkElement>
    {
        /// <summary>
        /// 遷移するウインドウの型を指定、または取得します。
        /// </summary>
        public Type WindowType
        {
            get { return (Type)GetValue(WindowTypeProperty); }
            set { SetValue(WindowTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WindowType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WindowTypeProperty =
            DependencyProperty.Register("WindowType", typeof(Type), typeof(TransitionInteractionMessageAction), new PropertyMetadata());

        private static bool IsValidWindowType(Type value)
        {
            if (value != null)
            {
                if (value.IsSubclassOf(typeof(System.Windows.Window)))
                {
                    return value.GetConstructor(Type.EmptyTypes) != null;
                }
            }

            return false;
        }

        /// <summary>
        /// 画面遷移の種類を指定するTransitionMode列挙体を指定、または取得します。<br/>
        /// TransitionMessageでModeがUnKnown以外に指定されていた場合、そちらが優先されます。
        /// </summary>
        public TransitionMode Mode
        {
            get { return (TransitionMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Mode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(TransitionMode), typeof(TransitionInteractionMessageAction), new UIPropertyMetadata(TransitionMode.Normal));

        /// <summary>
        /// 遷移先ウィンドウがこのウィンドウに所有されるかを設定します。
        /// </summary>
        public bool IsOwned
        {
            get { return (bool)GetValue(OwnedFromThisProperty); }
            set { SetValue(OwnedFromThisProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OwnedFromThis.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OwnedFromThisProperty =
            DependencyProperty.Register("IsOwned", typeof(bool), typeof(TransitionInteractionMessageAction), new UIPropertyMetadata(true));

        protected override void InvokeAction(InteractionMessage m)
        {
            var transitionMessage = m as TransitionMessage;

            if (transitionMessage == null)
            {
                return;
            }

            if (!IsValidWindowType(WindowType))
            {
                return;
            }

            var defaultConstructor = WindowType.GetConstructor(Type.EmptyTypes);

            TransitionMode mode = TransitionMode.UnKnown;

            if (Mode == TransitionMode.UnKnown && transitionMessage.Mode == TransitionMode.UnKnown)
            {
                return;
            }

            if (transitionMessage.Mode == TransitionMode.UnKnown)
            {
                mode = Mode;
            }
            else
            {
                mode = transitionMessage.Mode;
            }

            switch (mode)
            {
                case TransitionMode.Normal:
                case TransitionMode.Modal:
                    var targetWindow = (System.Windows.Window)defaultConstructor.Invoke(null);
                    if (transitionMessage.TransitionViewModel != null)
                    {
                        targetWindow.DataContext = transitionMessage.TransitionViewModel;
                    }

                    if (this.IsOwned)
                    {
                        targetWindow.Owner = Window.GetWindow(this.AssociatedObject);
                    }

                    if (mode == TransitionMode.Normal)
                    {
                        targetWindow.Show();
                    }
                    else
                    {
                        targetWindow.ShowDialog();
                    }

                    break;
                case TransitionMode.NewOrActive:
                    var window = Application.Current.Windows
                        .OfType<System.Windows.Window>()
                        .FirstOrDefault(w => w.GetType() == WindowType);

                    if (window == null)
                    {
                        window = (System.Windows.Window)defaultConstructor.Invoke(null);

                        if (transitionMessage.TransitionViewModel != null)
                        {
                            window.DataContext = transitionMessage.TransitionViewModel;
                        }
                        if (this.IsOwned)
                        {
                            window.Owner = System.Windows.Window.GetWindow(this.AssociatedObject);
                        }
                        window.Show();
                    }
                    else
                    {
                        if (transitionMessage.TransitionViewModel != null)
                        {
                            window.DataContext = transitionMessage.TransitionViewModel;
                        }
                        if (this.IsOwned)
                        {
                            window.Owner = System.Windows.Window.GetWindow(this.AssociatedObject);
                        }
                        window.Activate();
                    }

                    break;
            }

        }

    }
}
