using System;

namespace UNI.Core.UI.CustomEventArgs
{
    public class PrintEventArgs : EventArgs
    {
        public object ItemVm { get; set; }
        public Type TypePage { get; set; }

        public PrintEventArgs(object itemVm, Type typePage)
        {
            ItemVm = itemVm;
            TypePage = typePage;
        }
    }
}
