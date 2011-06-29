using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Mystique.Views.PartBlocks.MainBlock.TimelineChild
{
    public class DynamicTextBlock : TextBlock
    {
        public DynamicTextBlock()
            : base()
        {
        }

        public IEnumerable<Inline> DynamicInline
        {
            get { return (IEnumerable<Inline>)GetValue(DynamicInlineProperty); }
            set { SetValue(DynamicInlineProperty, value); }
        }

        private static void DynamicInlineChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var dtb = o as DynamicTextBlock;
            dtb.Inlines.Clear();
            if (e.NewValue != null)
                dtb.Inlines.AddRange(e.NewValue as IEnumerable<Inline>);
        }

        // Using a DependencyProperty as the backing store for DynamicInline.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DynamicInlineProperty =
            DependencyProperty.Register("DynamicInline", typeof(IEnumerable<Inline>), typeof(DynamicTextBlock), new FrameworkPropertyMetadata(new PropertyChangedCallback(DynamicInlineChanged)));
    }
}