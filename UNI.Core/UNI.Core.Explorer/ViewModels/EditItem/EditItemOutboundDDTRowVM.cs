using UNI.Core.Library.GenericModels;
using UNI.Core.UI.NewItem;

namespace UNI.Core.Explorer.ViewModels
{
    public class EditItemOutboundDDTRowVM<T> : EditItemVM<OutboundDDTRow>
    {
        public EditItemOutboundDDTRowVM(OutboundDDTRow item) : base(item)
        {
        }

        //public override void ViewBuilder_ObjectSelected(object sender, ObjectSelectedEventArgs e)
        //{
        //    Product product = e.Item as Product;
        //    if(product!= null)
        //    {
        //        SelectedItem.Description = product.Description;
        //        SelectedItem.UnitNetPrice = product.DefaultSellPrice;
        //        SelectedItem.VatPercentage = product.DefaultVat;
        //    }

        //    base.ViewBuilder_ObjectSelected(sender, e);
        //}
    }
}
