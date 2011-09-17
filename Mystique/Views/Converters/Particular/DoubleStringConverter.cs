using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace Mystique.Views.Converters.Particular
{
    public class DoubleStringConverter : TwoWayConverter<double, String>
    {
        public override string ToTarget(double input, object parameter)
        {
            return input.ToString();
        }

        public override double ToSource(string input, object parameter)
        {
            double value;
            if (double.TryParse(input, out value))
                return value;
            else
                return 0;
        }
    }

    public class DoubleStringValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            double conv;
            if (double.TryParse(value as string, out conv))
                return new ValidationResult(true, null);
            else
                return new ValidationResult(false, "数値に変換できません。");
        }
    }
}
