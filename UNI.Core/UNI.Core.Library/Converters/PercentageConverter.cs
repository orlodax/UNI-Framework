using System;

namespace UNI.Core.Library.Converters
{
    public class PercentageConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value.GetType() == typeof(double))
            {
                if (value != null)
                {
                    double val = (double)value;
                    return string.Format("{0:N2}%", val);
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

                    val = val.Replace('%', ' ');
                    val.Trim();
                    var valdouble = double.Parse(val);
                    return valdouble;
                }
            }
            catch (Exception)
            {

            }

            return 0;
        }
    }
}