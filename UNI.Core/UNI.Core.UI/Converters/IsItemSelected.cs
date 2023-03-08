using System;
using Windows.UI.Xaml.Data;

namespace UNI.Core.UI.Converters
{
    public class IsItemSelected : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}
