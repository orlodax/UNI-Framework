using System;
using UNI.Core.Library;

namespace UNI.Core.UI.CustomEventArgs
{
    public class ItemUpdatedEventArgs : EventArgs
    {
        public BaseModel Item { get; set; }

        public ItemUpdatedEventArgs(BaseModel item)
        {
            Item = item;
        }
    }
}
