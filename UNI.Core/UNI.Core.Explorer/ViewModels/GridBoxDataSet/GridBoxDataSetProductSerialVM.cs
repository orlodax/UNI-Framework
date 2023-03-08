using System;
using System.Collections.Generic;
using System.Reflection;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;
using UNI.Core.UI.CustomControls.GridBox;

namespace UNI.Core.Explorer.ViewModels.GridBoxDataSet
{
    public class GridBoxDataSetProductSerialVM<T> : GridBoxDataSetVM<ProductSerial>
    {
        public GridBoxDataSetProductSerialVM(BaseModel parentItem, UNI.Core.Client.UniDataSet<ProductSerial> uniDataSet, string name, PropertyInfo propertyInfo, Type newItemType, Type editItemType) : base(parentItem, uniDataSet, name, propertyInfo, newItemType, editItemType)
        {
            EditItemVisibility = Windows.UI.Xaml.Visibility.Collapsed;
            DeleteItemVisibility = Windows.UI.Xaml.Visibility.Collapsed;
            CreateItemVisibility = Windows.UI.Xaml.Visibility.Collapsed;
            SaveItemVisibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        protected override void PopulateItemsSource(List<FilterExpression> filterExpressions = null)
        {
            filterExpressions = new List<FilterExpression>();
            filterExpressions.Add(new FilterExpression() { PropertyName = "Quantity", PropertyValue = "1" });
            base.PopulateItemsSource(filterExpressions);
        }


    }
}
