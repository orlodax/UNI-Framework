using System;
using Windows.UI.Xaml.Data;

namespace UNI.Core.UI.Converters
{
    public class DateTimeToStringLocalTime : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset(((DateTime)value).ToLocalTime());
            string date = dateTimeOffset.ToString("d");
            return date;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return DateTime.Parse(value as string);
            //throw new NotImplementedException();
        }
    }
}
