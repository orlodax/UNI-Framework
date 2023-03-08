using System;
using System.Globalization;

namespace UNI.Core.Library.Converters
{
    public class CurrencyConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value.GetType() == typeof(double))
            {
                if (value != null)
                {
                    double val = (double)value;
                    return val.ToString("C");
                }

            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            try
            {
                string val = (string)value;
                if (!string.IsNullOrWhiteSpace(val))
                {
                    if (val.Contains("."))
                        val = val.Replace('.', ',');

                    var doubleval = double.Parse(val, NumberStyles.Currency);
                    return doubleval;
                }
            }
            catch (Exception)
            {

            }
            return 0;
        }
    }
}