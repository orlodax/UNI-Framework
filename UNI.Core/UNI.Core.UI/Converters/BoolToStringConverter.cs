using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace UNI.Core.UI.Converters
{
    /// <summary>
    /// Implements a databind converter from DateTime to DateTimeOffset.
    /// </summary>
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var resourceLoader = ResourceLoader.GetForCurrentView();

            if ((bool)value)
                return resourceLoader.GetString("yes");

            return resourceLoader.GetString("no");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
