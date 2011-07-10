using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace Mystique.Views.Converters
{
    public class DoublePercentStringConverter : TwoWayConverter<Double, String>
    {
        public override string ToTarget(double input, object parameter)
        {
            return ((int)(input * 100)).ToString();
        }

        public override double ToSource(string input, object parameter)
        {
            int percent;
            if (Int32.TryParse(input, out percent))
            {
                if (percent < 0)
                    return 0;
                else if (percent >= 100)
                    return 100;
                else
                    return percent / 100.0;
            }
            else
            {
                return 0;
            }
        }
    }

    public class DoublePercentValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            int percent;
            if (Int32.TryParse(value as string, out percent))
            {
                if (percent < 0 || percent > 100)
                    return new ValidationResult(false, "数値は0から100の間で指定してください。");
                else
                    return new ValidationResult(true, null);
            }
            else
            {
                return new ValidationResult(false, "数値に変換できません。");
            }
        }
    }
}
