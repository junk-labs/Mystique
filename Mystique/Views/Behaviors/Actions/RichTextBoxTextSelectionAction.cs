using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Mystique.Views.Behaviors.Actions
{
    public class RichTextBoxTextSelectionAction : TriggerAction<RichTextBox>
    {
        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(string), typeof(RichTextBoxTextSelectionAction), new UIPropertyMetadata(null));

        protected override void Invoke(object parameter)
        {
            this.Source = AssociatedObject.Selection.Text;
        }
    }
}
