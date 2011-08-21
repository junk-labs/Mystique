using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Mystique.Views.Behaviors.Attached
{
    public class RichTextBoxText
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached(
                "Text", typeof(string),
                typeof(RichTextBoxText),
                new UIPropertyMetadata(false, TextChanged)
            );

        [AttachedPropertyBrowsableForType(typeof(RichTextBox))]
        public static bool GetText(DependencyObject obj)
        {
            return (bool)obj.GetValue(TextProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(RichTextBox))]
        public static void SetText(DependencyObject obj, bool value)
        {
            obj.SetValue(TextProperty, value);
        }

        private static void TextChanged
            (DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var rtx = sender as RichTextBox;
            if (rtx == null) return;
            var range = new TextRange(rtx.Document.ContentStart, rtx.Document.ContentEnd);
            range.Text = e.NewValue as String;
        }
    }
}
