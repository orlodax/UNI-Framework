using System;
using System.Reflection;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;
using UNI.Core.UI.CustomControls.GridBox;

namespace UNI.Core.Explorer.ViewModels.GridBoxDataSet
{
    public class GridBoxDataSetProductMovementVM<T> : GridBoxDataSetVM<BaseProductMovement>
    {
        public GridBoxDataSetProductMovementVM(BaseModel parentItem, UNI.Core.Client.UniDataSet<BaseProductMovement> uniDataSet, string name, PropertyInfo propertyInfo, Type newItemType, Type editItemType) : base(parentItem, uniDataSet, name, propertyInfo, newItemType, editItemType)
        {
            CreateItemVisibility = Windows.UI.Xaml.Visibility.Collapsed;
            SaveItemVisibility = Windows.UI.Xaml.Visibility.Collapsed;
            EditItemVisibility = Windows.UI.Xaml.Visibility.Collapsed;
            DeleteItemVisibility = Windows.UI.Xaml.Visibility.Collapsed;

        }
    }
}
