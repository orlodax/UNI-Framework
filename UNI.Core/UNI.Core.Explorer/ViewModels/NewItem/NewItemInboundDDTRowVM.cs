using UNI.Core.Library.GenericModels;
using UNI.Core.UI.CustomEventArgs;
using UNI.Core.UI.NewItem;

namespace UNI.Core.Explorer.ViewModels
{
    public class NewItemInboundDDTRowVM<T> : NewItemVM<InboundDDTRow>
    {
        public override void ViewBuilder_ObjectSelected(object sender, ObjectSelectedEventArgs e)
        {
            base.ViewBuilder_ObjectSelected(sender, e);
            if (e.Item is BaseProduct product)
            {
                SelectedItem.Description = product.Description;
                SelectedItem.UnitNetPrice = product.DefaultSellPrice;
                SelectedItem.VatPercentage = product.DefaultVat;
            }
        }
    }
}
