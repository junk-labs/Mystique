using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Mystique.Views.PartBlocks.MainBlock.TimelineChild
{
    public class DynamicRichTextBox : RichTextBox
    {
        public DynamicRichTextBox()
        {
            if (this.Document == null)
                this.Document = new FlowDocument();
            this.Document.PreviewMouseDown += (ob, ev) => this.DocumentPreviewMouseDown(ob, ev);
            this.Document.PreviewMouseMove += (ob, ev) => this.DocumentPreviewMouseMove(ob, ev);
            this.Document.PreviewMouseUp += (ob, ev) => this.DocumentPreviewMouseUp(ob, ev);
            this.Document.MouseDown += (ob, ev) => this.DocumentMouseDown(ob, ev);
            this.Document.MouseMove += (ob, ev) => this.DocumentMouseMove(ob, ev);
            this.Document.MouseUp += (ob, ev) => this.DocumentMouseUp(ob, ev);
        }


        public IEnumerable<Inline> DynamicInline
        {
            get { return (IEnumerable<Inline>)GetValue(DynamicInlineProperty); }
            set { SetValue(DynamicInlineProperty, value); }
        }

        private static void DynamicInlineChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var rt = o as DynamicRichTextBox;
            if (rt.Document == null)
                return;
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
