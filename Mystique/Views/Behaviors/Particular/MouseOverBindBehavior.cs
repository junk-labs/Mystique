using System.Windows;
using System.Windows.Interactivity;

namespace Mystique.Views.Behaviors.Particular
{
    public class MouseOverBindBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.MouseEnter += new System.Windows.Input.MouseEventHandler(MouseChanged);
            this.AssociatedObject.MouseLeave += new System.Windows.Input.MouseEventHandler(MouseChanged);
            this.AssociatedObject.MouseMove += new System.Windows.Input.MouseEventHandler(MouseChanged);
            this.AssociatedObject.IsMouseDirectlyOverChanged += new DependencyPropertyChangedEventHandler(AssociatedObject_IsMouseDirectlyOverChanged);
            this.IsMouseOver = this.AssociatedObject.IsMouseOver;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.MouseEnter -= new System.Windows.Input.MouseEventHandler(MouseChanged);
            this.AssociatedObject.MouseLeave -= new System.Windows.Input.MouseEventHandler(MouseChanged);
            this.AssociatedObject.MouseMove -= new System.Windows.Input.MouseEventHandler(MouseChanged);
            this.AssociatedObject.IsMouseDirectlyOverChanged -= new DependencyPropertyChangedEventHandler(AssociatedObject_IsMouseDirectlyOverChanged);
        }

        void AssociatedObject_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.IsMouseOver = this.AssociatedObject.IsMouseOver;
        }

        void MouseChanged(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.IsMouseOver = this.AssociatedObject.IsMouseOver;
        }

        /// <summary>
        /// マウスがこの要素の上にあるかどうかを示します。
        /// </summary>
        public bool IsMouseOver
        {
            get { return (bool)this.GetValue(IsMouseOverProperty); }
            set { this.SetValue(IsMouseOverProperty, value); }
        }

        public static DependencyProperty IsMouseOverProperty =
            DependencyProperty.Register("IsMouseOver", typeof(bool), typeof(MouseOverBindBehavior), new FrameworkPropertyMetadata(false));
    }
}
