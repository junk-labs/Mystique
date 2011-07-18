using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.Filter.Core;
using System.Windows.Data;
using System.Windows.Controls;

namespace Mystique.Views.Converters
{
    public class LongRangeStringConverter : TwoWayConverter<LongRange, String>
    {
        public override string ToTarget(LongRange input, object parameter)
        {
            return input.ToString();
        }

        public override LongRange ToSource(string input, object parameter)
        {
            LongRange value;
            if (LongRange.TryParse(input, out value))
                return value;
            else
                return LongRange.FromPivotValue(0);
        }
    }

    public class LongRangeStringValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            LongRange lr;
            if (LongRange.TryParse(value as string, out lr))
            {
                return new ValidationResult(true, null);
            }
            else
            {
                return new ValidationResult(false, "数値範囲に変換できません。");
            }
        }
    }

}
