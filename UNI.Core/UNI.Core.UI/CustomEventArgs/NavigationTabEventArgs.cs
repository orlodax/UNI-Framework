using System;
using UNI.Core.UI.Menu;

namespace UNI.Core.UI.CustomEventArgs
{
    public class NavigationTabEventArgs : EventArgs
    {
        public MenuNode Node { get; set; }
    }
}
