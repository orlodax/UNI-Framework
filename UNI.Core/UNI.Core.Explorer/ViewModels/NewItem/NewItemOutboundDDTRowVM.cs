using UNI.Core.Library.GenericModels;
using UNI.Core.UI.CustomEventArgs;
using UNI.Core.UI.NewItem;

namespace UNI.Core.Explorer.ViewModels
{
    public class NewItemOutboundDDTRowVM<T> : NewItemVM<OutboundDDTRow>
    {
        public override void ViewBuilder_ObjectSelected(object sender, ObjectSelectedEventArgs e)
        {
            base.ViewBuilder_ObjectSelected(sender, e);
        }
    }
}
