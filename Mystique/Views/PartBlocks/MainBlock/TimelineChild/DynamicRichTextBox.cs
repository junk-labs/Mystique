using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Mystique.Views.PartBlocks.MainBlock.TimelineChild
{
    public class DynamicRichTextBox : RichTextBox
    {
        public IEnumerable<Inline> DynamicInline
        {
            get { return (IEnumerable<Inline>)GetValue(DynamicInlineProperty); }
            set { SetValue(DynamicInlineProperty, value); }
        }

        private static void DynamicInlineChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var rt = o as DynamicRichTextBox;
            FlowDocument fd = new FlowDocument();
            fd.PreviewMouseDown += (ob, ev) => rt.DocumentPreviewMouseDown(ob, ev);
            fd.PreviewMouseMove += (ob, ev) => rt.DocumentPreviewMouseMove(ob, ev);
            fd.PreviewMouseUp += (ob, ev) => rt.DocumentPreviewMouseUp(ob, ev);
            fd.MouseDown += (ob, ev) => rt.DocumentMouseDown(ob, ev);
            fd.MouseMove += (ob, ev) => rt.DocumentMouseMove(ob, ev);
            fd.MouseUp += (ob, ev) => rt.DocumentMouseUp(ob, ev);
            var para = new Paragraph();
            fd.Blocks.Add(para);
            if (e.NewValue != null)
                para.Inlines.AddRange(e.NewValue as IEnumerable<Inline>);
            rt.Document = fd;
        }

        public void EntryOnMouseDown(MouseButtonEventArgs e)
        {
            OnMouseDown(e);
        }

        public event MouseButtonEventHandler DocumentPreviewMouseDown = (o, e) => { };

        public event MouseEventHandler DocumentPreviewMouseMove = (o, e) => { };

        public event MouseButtonEventHandler DocumentPreviewMouseUp = (o, e) => { };

        public event MouseButtonEventHandler DocumentMouseDown = (o, e) => { };

        public event MouseEventHandler DocumentMouseMove = (o, e) => { };

        public event MouseButtonEventHandler DocumentMouseUp = (o, e) => { };

        // Using a DependencyProperty as the backing store for DynamicInline.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DynamicInlineProperty =
            DependencyProperty.Register("DynamicInline", typeof(IEnumerable<Inline>), typeof(DynamicRichTextBox), new UIPropertyMetadata(new PropertyChangedCallback(DynamicInlineChanged)));
    }
}
