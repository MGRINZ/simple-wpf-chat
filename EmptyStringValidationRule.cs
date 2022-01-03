using System.Globalization;
using System.Windows.Controls;

namespace Chat
{
    public class EmptyStringValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string str = (string)value;
            if (str.Trim().Length > 0)
                return ValidationResult.ValidResult;
            return new ValidationResult(false, "Pole nie może być puste");
        }
    }
}
