using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;
using UNI.Core.UI.CustomControls.GridBox;

namespace UNI.Core.Explorer.ViewModels
{
    public class GridBoxMtMOutboundProductSerialVM<T> : GridBoxMtMVM<ProductSerial>
    {
        public GridBoxMtMOutboundProductSerialVM(BaseModel parentItem,
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



        public override Task LoadData(List<FilterExpression> filterExpressions = null)
        {
            filterExpressions = new List<FilterExpression>() { new FilterExpression() { PropertyName = "Quantity", PropertyValue = "1" } };
            return base.LoadData(filterExpressions);
        }
    }
}
