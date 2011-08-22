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
            if (rt.Document == null)
            {
                // initialize
                rt.Document = new FlowDocument();
                rt.Document.PreviewMouseDown += (ob, ev) => rt.DocumentPreviewMouseDown(ob, ev);
                rt.Document.PreviewMouseMove += (ob, ev) => rt.DocumentPreviewMouseMove(ob, ev);
                rt.Document.PreviewMouseUp += (ob, ev) => rt.DocumentPreviewMouseUp(ob, ev);
                rt.Document.MouseDown += (ob, ev) => rt.DocumentMouseDown(ob, ev);
                rt.Document.MouseMove += (ob, ev) => rt.DocumentMouseMove(ob, ev);
                rt.Document.MouseUp += (ob, ev) => rt.DocumentMouseUp(ob, ev);
            }
            var para = rt.Document.Blocks.FirstBlock as Paragraph;
            if (para == null)
            {
                para = new Paragraph();
                rt.Document.Blocks.Clear();
                rt.Document.Blocks.Add(para);
            }
            para.Inlines.Clear();
            if (e.NewValue != null)
                para.Inlines.AddRange(e.NewValue as IEnumerable<Inline>);
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
