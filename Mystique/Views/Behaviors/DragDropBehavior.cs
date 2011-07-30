using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interactivity;
using Inscribe.Common;
using System.Windows.Input;

namespace Mystique.Views.Behaviors
{
    public class DragDropBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            this.AssociatedObject.PreviewMouseDown += AssociatedObject_MouseDown;
            this.AssociatedObject.PreviewMouseMove += AssociatedObject_MouseMove;
            this.AssociatedObject.PreviewMouseUp += AssociatedObject_MouseUp;
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.PreviewMouseDown -= AssociatedObject_MouseDown;
            this.AssociatedObject.PreviewMouseMove -= AssociatedObject_MouseMove;
            this.AssociatedObject.PreviewMouseUp -= AssociatedObject_MouseUp;
        }

        public object DragDropData
        {
            get { return (object)GetValue(DragDropDataProperty); }
            set { SetValue(DragDropDataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DragDropData.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DragDropDataProperty =
            DependencyProperty.Register("DragDropData", typeof(object), typeof(DragDropBehavior), new UIPropertyMetadata(null));

        public DragDropEffects AllowedEffects
        {
            get { return (DragDropEffects)GetValue(AllowedEffectsProperty); }
            set { SetValue(AllowedEffectsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AllowedEffects.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AllowedEffectsProperty =
            DependencyProperty.Register("AllowedEffects", typeof(DragDropEffects), typeof(DragDropBehavior), new UIPropertyMetadata(DragDropEffects.All));

        public ICommand BeforeDragDropCommand
        {
            get { return (ICommand)GetValue(BeforeDragDropCommandProperty); }
            set { SetValue(BeforeDragDropCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BeforeDragDropCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BeforeDragDropCommandProperty =
            DependencyProperty.Register("BeforeDragDropCommand", typeof(ICommand), typeof(DragDropBehavior), new UIPropertyMetadata(null));

        public ICommand AfterDragDropCommand
        {
            get { return (ICommand)GetValue(AfterDragDropCommandProperty); }
            set { SetValue(AfterDragDropCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AfterDragDropCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AfterDragDropCommandProperty =
            DependencyProperty.Register("AfterDragDropCommand", typeof(ICommand), typeof(DragDropBehavior), new UIPropertyMetadata(null));

        public ICommand OnClickCommand
        {
            get { return (ICommand)GetValue(OnClickCommandProperty); }
            set { SetValue(OnClickCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OnClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OnClickCommandProperty =
            DependencyProperty.Register("OnClickCommand", typeof(ICommand), typeof(DragDropBehavior), new UIPropertyMetadata(null));



        public bool OverrideClickEvent
        {
            get { return (bool)GetValue(OverrideClickEventProperty); }
            set { SetValue(OverrideClickEventProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OverrideClickEvent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OverrideClickEventProperty =
            DependencyProperty.Register("OverrideClickEvent", typeof(bool), typeof(DragDropBehavior), new UIPropertyMetadata(false));

        Point oPoint = new Point(double.MinValue, double.MinValue);
        bool down = false;

        private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            oPoint = e.GetPosition(this.AssociatedObject);
            down = true;
            if (OverrideClickEvent)
                e.Handled = true;
        }

        private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || !down) return;
            var point = e.GetPosition(this.AssociatedObject);
            var l = oPoint.DistanceDouble(point);
            if (l > 4)
            {
                if (this.BeforeDragDropCommand != null)
                    this.BeforeDragDropCommand.Execute(null);
                DragDrop.DoDragDrop(this.AssociatedObject, this.DragDropData, this.AllowedEffects);
                if (this.AfterDragDropCommand != null)
                    this.AfterDragDropCommand.Execute(null);
                down = false;
                e.Handled = true;
            }
            if (OverrideClickEvent)
                e.Handled = true;
        }

        private void AssociatedObject_MouseUp(object sender, MouseButtonEventArgs e)
        {
            down = false;
            var point = e.GetPosition(this.AssociatedObject);
            var l = oPoint.DistanceDouble(point);
            if (l <= 4 && OnClickCommand != null)
                OnClickCommand.Execute(null);
            if (OverrideClickEvent)
                e.Handled = true;
        }
    }
}
