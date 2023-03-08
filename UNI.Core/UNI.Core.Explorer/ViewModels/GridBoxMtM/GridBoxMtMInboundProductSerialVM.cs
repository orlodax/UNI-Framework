using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;
using UNI.Core.UI.CustomControls.GridBox;

namespace UNI.Core.Explorer.ViewModels
{
    public class GridBoxMtMInboundProductSerialVM<T> : GridBoxMtMVM<ProductSerial>
    {
        public GridBoxMtMInboundProductSerialVM(BaseModel parentItem,
                                                List<ProductSerial> itemsSource,
                                                string name,
                                                PropertyInfo propertyInfo,
                                                string dependencyFilterPropertyName,
                                                string parentFilterPropertyName,
                                                string depenencyFilterPropertyValue,
                                                Type newItemType,
                                                Type editItemType,
                                                Type showBoxVMType) : base(parentItem, itemsSource, name, propertyInfo, dependencyFilterPropertyName, parentFilterPropertyName, depenencyFilterPropertyValue, newItemType, editItemType, showBoxVMType)
        {
            CreateItemVisibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        public override void EditItemCommand(object parameter)
        {
            CreateItem.Execute(parameter);
        }

        public override Task LoadData(List<FilterExpression> filterExpressions = null)
        {
            filterExpressions = new List<FilterExpression>() { new FilterExpression() { PropertyName = "Quantity", PropertyValue = "1" } };
            return base.LoadData(filterExpressions);
        }
    }
}
