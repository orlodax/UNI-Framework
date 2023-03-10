using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Reflection;
using UNI.API.Contracts.Models;
using UNI.Core.Library;
using UNI.Core.UI.CustomControls.GridBox;
using UNI.Core.UI.CustomControls.ShowBox;
using UNI.Core.UI.Tabs.ListDetail;

namespace UNI.Core.UI.Menu.Settings
{
    public class UserManagerVM : ListDetailVM<User>
    {
        public UserManagerVM()
        {
            ViewBuilder.CustomizeVisibleProperties(new List<string>
            {
                nameof(User.FirstName),
                nameof(User.LastName),
                nameof(User.DateOfBirth),
                nameof(User.Email),
                nameof(User.PhoneNumber),
                nameof(User.Roles)
            });

            ViewBuilder.CustomizeViewModelByPropertyName(nameof(User.Roles), typeof(UserRolesGridBoxMTMVM));

            ViewBuilder.CustomizeShowBoxVM(UI.ViewBuilder.EnControlTypes.GridBoxMtM, typeof(ShowBoxRolesVM));
        }
    }

    public class UserRolesGridBoxMTMVM : GridBoxMtMVM<Role>
    {
        public UserRolesGridBoxMTMVM(BaseModel parentItem,
                                     List<Role> itemsSource,
                                     string name,
                                     PropertyInfo propertyInfo,
                                     string dependencyFilterPropertyName,
                                     string parentFilterPropertyName,
                                     string depenencyFilterPropertyValue,
                                     Type newItemType,
                                     Type editItemType,
                                     Type showBoxVMType) : base(parentItem, itemsSource, name, propertyInfo, dependencyFilterPropertyName, parentFilterPropertyName, depenencyFilterPropertyValue, newItemType, editItemType, showBoxVMType)
        {
            ViewBuilder.CustomizeVisibleProperties(new List<string> { nameof(Role.Name) });

            MainGrid = (DataGrid)ViewBuilder.RenderMainFrameworkElement(height: 300);
        }
    }

    public class ShowBoxRolesVM : ShowBoxVM<Role>
    {
        public ShowBoxRolesVM(Role dependency,
                              string memberName,
                              PropertyInfo propertyInfo,
                              BaseModel parent,
                              ShowBoxFilters filters,
                              Type newItemType) : base(dependency, memberName, propertyInfo, parent, filters, newItemType)
        {
            NewItemButtonVisibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
    }
}
