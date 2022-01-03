using System;
using System.Globalization;
using System.Windows.Controls;

namespace Chat
{
    public class PortValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int port;
            if (Int32.TryParse((string)value, out port))
            {
                if (port > 1023 && port < 65535)
                    return ValidationResult.ValidResult;
                else
                    return new ValidationResult(false, "Wprowadzona wartość jest nieprawidłowa.");
            }
            else
                return new ValidationResult(false, "Numer portu musi mieścić się w zakresie od 1024 do 65535.");
        }
    }
}
