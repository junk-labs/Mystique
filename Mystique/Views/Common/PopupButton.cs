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

        public Popup DropDownPopup
        {
            get { return GetValue(DropDownPopupProperty) as Popup; }
            set { this.SetValue(DropDownPopupProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DropDownPopup.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DropDownPopupProperty =
            DependencyProperty.Register("DropDownPopup", typeof(Popup), typeof(PopupButton), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// コントロールがクリックされた時のイベントです。
        /// </summary>
        protected override void OnClick()
        {
            if (this.DropDownPopup == null)
                return;
            this.DropDownPopup.PlacementTarget = this;
            this.DropDownPopup.StaysOpen = false;
            this.DropDownPopup.Placement = PlacementMode.Bottom;
            this.DropDownPopup.IsOpen = !this.DropDownPopup.IsOpen;
        }
    }
}