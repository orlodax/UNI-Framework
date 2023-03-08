using System;
using Windows.UI.Xaml.Data;

namespace UNI.Core.UI.Converters
{
    /// <summary>
    /// Implements a databind converter from DateTime to DateTimeOffset.
    /// </summary>
    public class DateTimeToDateTimeOffsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            if (DateTime.Compare((DateTime)value, new DateTime()) == 0)
            {
                return null;
            }
            var dof = new DateTimeOffset(((DateTime)value).ToLocalTime());
            return dof;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
                return ((DateTimeOffset)value).DateTime;
            else return new DateTimeOffset();
        }
    }
}
