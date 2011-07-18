using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace Mystique.Views.Converters
{
    public class LongStringConverter : TwoWayConverter<long, String>
    {
        public override string ToTarget(long input, object parameter)
        {
            return input.ToString();
        }

        public override long ToSource(string input, object parameter)
        {
            long value;
            if (long.TryParse(input, out value))
                return value;
            else
                return 0;
        }
    }

    public class LongStringValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            long conv;
            if (long.TryParse(value as string, out conv))
            {
                return new ValidationResult(true, null);
            }
            else
            {
                return new ValidationResult(false, "数値に変換できません。");
            }
        }
    }

}
