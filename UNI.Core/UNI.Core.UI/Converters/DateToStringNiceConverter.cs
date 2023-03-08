using System;
using Windows.UI.Xaml.Data;

namespace UNI.Core.UI.Converters
{
    public class DateToStringNiceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var date = (DateTime)value;

            return date.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
