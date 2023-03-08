using Gioiaspa.Warehouse.Library;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Reflection;
using UNI.Core.Library;
using UNI.Core.UI.CustomControls.GridBox;

namespace UNI.Core.Explorer.CustomControls
{
    internal class GridBoxVMSalesOrder : GridBoxVM<SalesOrder>
    {
        public GridBoxVMSalesOrder(BaseModel parentItem, List<SalesOrder> itemsSource, string name, PropertyInfo propertyInfo, Type newItemType, Type editItemType) : base(parentItem, itemsSource, name, propertyInfo, newItemType, editItemType)
        {
            SetCustomProperties();
            MainGrid = (DataGrid)ViewBuilder.RenderMainFrameworkElement(height: 300);
        }

        public void SetCustomProperties()
        {
            var visibleProps = new List<string>
            {
                 nameof(SalesOrder.RegistrationNumber),
                 nameof(SalesOrder.Date),
                 nameof(SalesOrder.Causal),
                 nameof(SalesOrder.Sender),
                 nameof(SalesOrder.Protocol),
                 nameof(SalesOrder.Series),
                 nameof(SalesOrder.IdBordero),
                 nameof(SalesOrder.IdDDT),

            };
            ViewBuilder.CustomizeVisibleProperties(visibleProps);
        }
    }
}
