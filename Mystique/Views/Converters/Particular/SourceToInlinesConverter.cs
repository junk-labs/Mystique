using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.ViewModels;
using System.Windows.Documents;
using System.Windows.Data;
using Dulcet.Twitter;
using System.Text.RegularExpressions;
using Inscribe.Text;
using System.Windows.Media;
using Inscribe.Common;

namespace Mystique.Views.Converters.Particular
{
    public class SourceToInlinesConverter : OneWayConverter<TweetViewModel, IEnumerable<Inline>>
    {
        public override IEnumerable<Inline> ToTarget(TweetViewModel input, object parameter)
        {
            var status = input.Status as TwitterStatus;
            if (status != null)
            {
                var src = status.Source;
                Match m = null;
                if (!String.IsNullOrEmpty(src) && (m = RegularExpressions.URLRegex.Match(src.Replace("\\", ""))).Success)
                {
                    var hlink = new Hyperlink(new Run(m.Groups[2].Value));
                    hlink.Click += (o, e) => Browser.Start(m.Groups[1].Value);
                    hlink.Foreground = Brushes.DodgerBlue;
                    return new[] { hlink };
                }
            }
            // ELSE
            return new Run[0];
        }
    }
}
