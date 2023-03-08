using System;
using System.Reflection;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace UNI.Core.UI.Converters
{
    internal class PropertyAttributeNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var resourceLoader = ResourceLoader.GetForCurrentView();
            var property = value as PropertyInfo;
            string propertyName = resourceLoader.GetString($"combobox_{property.DeclaringType.Name}_{property.Name}");
            if (string.IsNullOrEmpty(propertyName))
                propertyName = property.Name;

            return propertyName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
