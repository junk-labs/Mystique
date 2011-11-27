using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using Dulcet.Twitter;
using Inscribe.Common;
using Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild;
using Mystique.Views.Common;

namespace Mystique.Views.Converters.Particular
{
    public class SourceToInlinesConverter : OneWayConverter<TweetViewModel, IEnumerable<Inline>>
    {
        static Regex SourceRegex = new Regex("=\"(.*?)\".*?>(.*?)<", RegexOptions.Singleline | RegexOptions.Compiled);

        public override IEnumerable<Inline> ToTarget(TweetViewModel input, object parameter)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
                return Application.Current.Dispatcher.Invoke(new Action(() => ToTarget(input, parameter)), null) as IEnumerable<Inline>;

            bool referenceOriginal = parameter as string != null ? bool.Parse(parameter as string) : false;
            var status = input.Status as TwitterStatus;
            if (referenceOriginal && status != null && status.RetweetedOriginal != null)
                status = status.RetweetedOriginal;
            if (status != null)
            {
                var src = status.Source;
                Match m = null;
                if (!String.IsNullOrEmpty(src) && (m = SourceRegex.Match(src.Replace("\\", ""))).Success)
                {
                    return new[] { TextElementGenerator.GenerateHyperlink(
                        m.Groups[2].Value,
                        ()=>Browser.Start(m.Groups[1].Value))};
                }
                else
                {
                    return new[] { TextElementGenerator.GenerateRun(status.Source) };
                }
            }
            // ELSE
            return new Run[0];
        }
    }
}
