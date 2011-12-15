using System;
using System.Windows.Documents;
using System.Windows.Media;
using Inscribe.Configuration;

namespace Mystique.Views.Common
{
    public static class TextElementGenerator
    {
        public static Hyperlink GenerateHyperlink(String surface, Action onClick)
        {
            var hlink = new Hyperlink(new Run(surface));
            hlink.Foreground = Setting.IsInitialized ?
                Setting.Instance.ColoringProperty.DefaultLinkColor.GetBrush() :
                new SolidColorBrush(Colors.DodgerBlue);
            hlink.PreviewMouseLeftButtonDown += (o, e) =>
            {
                e.Handled = true;
                onClick();
            };
            hlink.Click += (o, e) => onClick();
            return hlink;
        }

        public static Run GenerateRun(String surface)
        {
            return new Run(surface);
        }
    }
}
