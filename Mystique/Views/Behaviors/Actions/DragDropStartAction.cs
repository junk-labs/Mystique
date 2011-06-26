using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Mystique.Views.Behaviors.Actions
{
    public class DragDropStartAction : TriggerAction<FrameworkElement>
    {
        public object DragDropData
        {
            get { return (object)GetValue(DragDropDataProperty); }
            set { SetValue(DragDropDataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DragDropData.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DragDropDataProperty =
            DependencyProperty.Register("DragDropData", typeof(object), typeof(DragDropStartAction), new UIPropertyMetadata(null));

        public DragDropEffects AllowedEffects
        {
            get { return (DragDropEffects)GetValue(AllowedEffectsProperty); }
            set { SetValue(AllowedEffectsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AllowedEffects.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AllowedEffectsProperty =
            DependencyProperty.Register("AllowedEffects", typeof(DragDropEffects), typeof(DragDropStartAction), new UIPropertyMetadata(DragDropEffects.All));

        public ICommand BeforeDragDropCommand
        {
            get { return (ICommand)GetValue(BeforeDragDropCommandProperty); }
            set { SetValue(BeforeDragDropCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BeforeDragDropCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BeforeDragDropCommandProperty =
            DependencyProperty.Register("BeforeDragDropCommand", typeof(ICommand), typeof(DragDropStartAction), new UIPropertyMetadata(null));

        public ICommand AfterDragDropCommand
        {
            get { return (ICommand)GetValue(AfterDragDropCommandProperty); }
            set { SetValue(AfterDragDropCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AfterDragDropCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AfterDragDropCommandProperty =
            DependencyProperty.Register("AfterDragDropCommand", typeof(ICommand), typeof(DragDropStartAction), new UIPropertyMetadata(null));

        protected override void Invoke(object parameter)
        {
            var marg = parameter as MouseEventArgs;
            if (marg != null && marg.LeftButton != MouseButtonState.Pressed)
                return;
            if (this.BeforeDragDropCommand != null)
                this.BeforeDragDropCommand.Execute(null);
            DragDrop.DoDragDrop(AssociatedObject, this.DragDropData, this.AllowedEffects);
            if (this.AfterDragDropCommand != null)
                this.AfterDragDropCommand.Execute(null);
        }
    }
}
