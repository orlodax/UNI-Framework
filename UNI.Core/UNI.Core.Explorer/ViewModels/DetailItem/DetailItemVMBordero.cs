using Gioiaspa.Warehouse.Library;
using System.Collections.Generic;
using UNI.Core.Explorer.CustomControls;
using UNI.Core.UI.Tabs.DetailItem;

namespace UNI.Core.Explorer.ViewModels.DetailItem
{
    internal class DetailItemVMBordero : DetailItemVM<Bordero>
    {
        public DetailItemVMBordero(Bordero selectedItem) : base(selectedItem)
        {
            SetCustomProperties();
            DrawDetailsPane();
        }

        public void SetCustomProperties()
        {
            var visibleProps = new List<string>
            {
               nameof(Bordero.Date),
               nameof(Bordero.DeliveryDate),
               nameof(Bordero.Name),
               nameof(Bordero.Carrier),
               nameof(Bordero.State),
               nameof(Bordero.Area),
               nameof(Bordero.SalesOrder),
            };

            ViewBuilder.CustomizeVisibleProperties(visibleProps);

            //ViewBuilder.CustomizeViewModel(typeof(UNI.Core.UI.CustomControls.GridBox.GridBox), typeof(GridBoxVMSalesOrder));
            ViewBuilder.CustomizeViewModelByPropertyName(nameof(Bordero.SalesOrder), typeof(GridBoxVMSalesOrder));

        }

    }
}
