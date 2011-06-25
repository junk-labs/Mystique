using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Mystique.Views.Common
{
    public class DropDownButton : ToggleButton
    {
        public DropDownButton()
        {
            var binding = new Binding("DropDownMenu.IsOpen") { Source = this };
            this.SetBinding(PopupButton.IsCheckedProperty, binding);
        }

        public ContextMenu DropDownMenu
        {
            get { return (ContextMenu)GetValue(DropDownMenuProperty); }
            set { SetValue(DropDownMenuProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DropDownMenu.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DropDownMenuProperty =
            DependencyProperty.Register("DropDownMenu", typeof(ContextMenu), typeof(DropDownButton), new UIPropertyMetadata(null, DropDownMenuChanged));

        private static void DropDownMenuChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var ddb = o as DropDownButton;
            var cm = e.NewValue as ContextMenu;
            if (ddb == null || cm == null) return;
            cm.PlacementTarget = ddb;
            cm.Placement = PlacementMode.Bottom;
            cm.StaysOpen = false;
        }
    }
}
