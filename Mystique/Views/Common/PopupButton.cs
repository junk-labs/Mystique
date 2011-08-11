    using System.Windows;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;

namespace Mystique.Views.Common
{
    /// <summary>
    /// ドロップダウンポップアップを表示可能なボタンです。
    /// </summary>
    public class PopupButton : ToggleButton
    {
        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        public PopupButton()
        {
            var binding = new Binding("DropDownPopup.IsOpen") { Source = this };
            this.SetBinding(PopupButton.IsCheckedProperty, binding);
        }

        public UIElement DropDownPopup
        {
            get { return GetValue(DropDownPopupProperty) as UIElement; }
            set { this.SetValue(DropDownPopupProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DropDownPopup.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DropDownPopupProperty =
            DependencyProperty.Register("DropDownPopup", typeof(UIElement), typeof(PopupButton), new FrameworkPropertyMetadata(null, DropDownPopupChanged));

        private static void DropDownPopupChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var pb = o as PopupButton;
            var popobj = e.NewValue as UIElement;
            if (pb == null || popobj == null) return;
            Popup popup = popobj as Popup;
            if(popup == null)
            {
                popup = new Popup();
                popup.Child = popobj;
            }
            popup.PlacementTarget = pb;
            popup.StaysOpen = false;
        }
    }
}