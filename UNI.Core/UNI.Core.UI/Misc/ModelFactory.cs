using System;
using UNI.Core.Library;

namespace UNI.Core.UI.Misc
{
    public static class ModelFactory
    {
        public static T CreateModel<T>(BaseModel parentItem = null)
        {
            T item = (T)Activator.CreateInstance(typeof(T));
            BaseModel model = item as BaseModel;
            model?.InitModel(parentItem);
            return (T)Convert.ChangeType(model, typeof(T));
        }
    }
}
