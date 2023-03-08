using System;
using System.Reflection;
using UNI.Core.Library;

namespace UNI.Core.UI.CustomEventArgs
{
    public class ObjectSelectedEventArgs : EventArgs
    {
        public BaseModel Item { get; set; }
        public PropertyInfo PropertyInfo { get; set; }

        public ObjectSelectedEventArgs(BaseModel item, PropertyInfo propertyInfo)
        {
            Item = item;
            PropertyInfo = propertyInfo;
        }
    }
}
