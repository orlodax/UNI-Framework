using System;
using System.Collections.Generic;
using UNI.Core.Library.GenericModels;
using UNI.Core.UI.NewItem;

namespace UNI.Core.Explorer.ViewModels
{
    public class EditItemInboundDDTRowVM<T> : EditItemVM<InboundDDTRow>
    {
        public EditItemInboundDDTRowVM(InboundDDTRow item) : base(item)
        {
            //F1ButtonVisibility = Windows.UI.Xaml.Visibility.Visible;
            //F1ButtonContent = "Scansione seriali";
        }

        //public override void F1Command(object parameter)
        //{
        //    var newItemProductSerialVm = new NewItemMultipleProductSerialVM<ProductSerial>
        //    {
        //        ParentItem = SelectedItem
        //    };

        //    var newitem = new MultipleProductSerialNewItem
        //    {
        //        DataContext = newItemProductSerialVm
        //    };
        //    newItemProductSerialVm.SerialsAdded += NewItemProductSerialVm_SerialsAdded;
        //    TabViewVM.ShowContentDialog(newitem);

        //    base.F1Command(parameter);
        //}

        private async void NewItemProductSerialVm_SerialsAdded(object sender, EventArgs e)
        {
            if (sender is NewItemMultipleProductSerialVM<ProductSerial> vm)
            {
                if (SelectedItem.InboundProductSerials == null) SelectedItem.InboundProductSerials = new List<ProductSerial>();
                foreach (var serial in vm.ProductSerials)
                {
                    ProductSerial inboundProductSerial = new ProductSerial()
                    {
                        ID = serial.ID,
                        IdProduct = serial.IdProduct,
                        SerialCode = serial.SerialCode

                    };
                    SelectedItem.InboundProductSerials.Add(inboundProductSerial);
                }
                await BaseClient.UpdateItem(SelectedItem);
            }
        }

        //public override void ViewBuilder_ObjectSelected(object sender, ObjectSelectedEventArgs e)
        //{

        //    base.ViewBuilder_ObjectSelected(sender, e);

        //    Product product = e.Item as Product;
        //    if (product != null)
        //    {
        //        SelectedItem.Description = product.Description;
        //        SelectedItem.UnitNetPrice = product.DefaultSellPrice;
        //        SelectedItem.VatPercentage = product.DefaultVat;
        //    }
        //}
    }
}
