using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace Mystique.Views.CustomPanels
{
    public class FillPanel : Panel
    {
        public FillPanel() : base() { }

        protected override Size MeasureOverride(Size availableSize)
        {
            var panelDesired = new Size();
            foreach (UIElement elem in InternalChildren)
            {
                elem.Measure(availableSize);
                if (elem.DesiredSize.Width > panelDesired.Width)
                    panelDesired.Width = elem.DesiredSize.Width;
                if (elem.DesiredSize.Height > panelDesired.Height)
                    panelDesired.Height = elem.DesiredSize.Height;
            }
            return panelDesired;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                child.Arrange(new Rect(new Point(0, 0), finalSize));
            }
            return finalSize;
        }
    }
}
