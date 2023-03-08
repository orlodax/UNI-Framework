using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UNI.Core.UI.Misc
{
    public static class PlatformTypeConverterHelper
    {
        public static Type GetSpecificValueConverter(Type type)
        {
            IEnumerable<Type> query = from t in Assembly.GetExecutingAssembly().GetTypes()
                                      where t.IsClass && (t?.Namespace?.Contains("Converters") ?? false)
                                      select t;

            if (query != null && query.Any())
                foreach (Type t in query)
                    if (t.IsSubclassOf(type))
                        return t;

            return type;
        }
    }
}
