using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild;

namespace Mystique.Views.Converters.Particular
{
    public class TimelineDateConverter : OneWayConverter<DateTime, string>
    {
        public override string ToTarget(DateTime input, object parameter)
        {
            if (input.Date == DateTime.Now.Date)
            {
                return String.Format("{0:D2}:{1:D2}:{2:D2}", input.Hour, input.Minute, input.Second);
            }
            else
            {
                return String.Format("{3:D2}/{4:D2}/{5:D2}\n{0:D2}:{1:D2}:{2:D2}", input.Hour, input.Minute, input.Second, input.Year % 100, input.Month, input.Day);
            }
        }
    }
}
