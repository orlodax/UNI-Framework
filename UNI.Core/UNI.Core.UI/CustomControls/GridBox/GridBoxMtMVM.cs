using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UNI.API.Contracts.RequestsDTO;
using UNI.Core.Library;
using UNI.Core.UI.CustomControls.ShowBox;
using UNI.Core.UI.CustomEventArgs;
using UNI.Core.UI.MainPage;
using UNI.Core.UI.Misc;
using UNI.Core.UI.NewItem;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UNI.Core.UI.CustomControls.GridBox
{
    public class GridBoxMtMVM<T> : GridBoxVM<T> where T : BaseModel
    {
        public ShowBoxVM<T> ShowBoxVM { get; set; }
        private Type showBoxVMType;

        private readonly string depenencyFilterPropertyValue;
        private readonly string dependencyFilterPropertyName; //used by selectorbox for advanced filter
        private readonly string parentFilterPropertyName; //used by selectorbox for advanced filter

        public GridBoxMtMVM(BaseModel parentItem,
                            List<T> itemsSource,
                            string name,
                            PropertyInfo propertyInfo,
                            string dependencyFilterPropertyName,
                            string parentFilterPropertyName,
                            string depenencyFilterPropertyValue,
                            Type newItemType,
                            Type editItemType,
                            Type showBoxVMType) : base(parentItem, itemsSource, name, propertyInfo, newItemType, editItemType)
        {
            this.dependencyFilterPropertyName = dependencyFilterPropertyName;
            this.parentFilterPropertyName = parentFilterPropertyName;
            this.depenencyFilterPropertyValue = depenencyFilterPropertyValue;
            this.showBoxVMType = showBoxVMType;

            CreateItemVisibility = Visibility.Collapsed;
            SaveItemVisibility = Visibility.Collapsed;
            NavButtonsVisibility = Visibility.Visible;

            BuildShowBox();

            BindCommands();
        }

        #region Commands

        private void BindCommands()
        {
            CreateItem = new RelayCommand((parameter) =>
            {
                if (NewItemVM == null)
                {
                    NewItemVM = (NewItemVM<T>)Activator.CreateInstance(newItemVMType.MakeGenericType(typeof(T)), new object[] { ParentItem });
                    NewItemVM.ItemUpdated += ItemVM_ItemUpdated;
                }
                TabViewVM.ShowContentDialog(new NewItem.NewItem() { DataContext = NewItemVM });
            });

            EditItem = new RelayCommand((parameter) =>
            {
                ShowBoxVisibility = !ShowBoxVisibility;
            });
        }

        private void BuildShowBox()
        {
            var filters = new ShowBoxFilters()
            {
                DependencyFilterPropertyName = dependencyFilterPropertyName,
                DependencyFilterPropertyValue = depenencyFilterPropertyValue,
                ParentFilterPropertyName = parentFilterPropertyName
            };

            string memberName = ResourceLoader.GetForCurrentView().GetString($"textbox_{typeof(T).Name}_{PropertyInfo.Name}");
            if (string.IsNullOrWhiteSpace(memberName))
                memberName = PropertyInfo.Name;

            if (showBoxVMType != null)
            {
                if (showBoxVMType.IsGenericType)
                    ShowBoxVM = (ShowBoxVM<T>)Activator.CreateInstance(showBoxVMType.MakeGenericType(new Type[] { typeof(T) }), new object[] { SelectedItem, memberName, PropertyInfo, ParentItem, filters, newItemVMType });
                else
                    ShowBoxVM = (ShowBoxVM<T>)Activator.CreateInstance(showBoxVMType, new object[] { SelectedItem, memberName, PropertyInfo, ParentItem, filters, newItemVMType });
            }
            else
                ShowBoxVM = new ShowBoxVM<T>(SelectedItem, memberName, PropertyInfo, ParentItem, filters, newItemVMType);

            ShowBoxVM.SelectionChanged = new RelayCommand((sbParameter) =>
            {
                ShowBoxVM.DisplayProperty = ShowBoxVM.GetDisplayProperty();
                if (ShowBoxVM.SelectedItem != null)
                    if (!ItemsSource.Any(i => i.ID == ShowBoxVM.SelectedItem.ID))
                        ItemsSource.Add(ShowBoxVM.SelectedItem);

                PropertyInfo.SetValue(ParentItem, ItemsSource.ToList());
            });
        }

        protected override void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (SelectedItem != null)
            {
                notFilteredItemsSource.Remove(SelectedItem);
                OnItemUpdated(this, new ItemUpdatedEventArgs(null));
            }
        }

        public virtual async Task LoadData(List<FilterExpression> filterExpressions = null)
        {
            var result = new List<T>();
            if (!string.IsNullOrWhiteSpace(dependencyFilterPropertyName))
            {
                if (!string.IsNullOrWhiteSpace(parentFilterPropertyName))
                {
                    var property = ParentItem.GetType().GetProperty(parentFilterPropertyName);
                    if (property != null)
                    {
                        int parentPropertyId = (int)property.GetValue(ParentItem, null);
                        if (parentPropertyId != 0)
                        {
                            var request = new GetDataSetRequestDTO() { Id = parentPropertyId, IdName = dependencyFilterPropertyName, RequestedEntriesNumber = null, FilterExpressions = filterExpressions };
                            result = await BaseClient.Get(request) ?? new List<T>();
                        }
                        else
                        {
                            result = await BaseClient.Get(new GetDataSetRequestDTO() { FilterExpressions = filterExpressions, RequestedEntriesNumber = null }) ?? new List<T>();
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(depenencyFilterPropertyValue))
                {
                    List<T> filteredSource = new List<T>();
                    foreach (T item in itemsSource)
                    {
                        var property = typeof(T).GetProperty(dependencyFilterPropertyName);
                        if (property != null)
                        {
                            var value = property.GetValue(item);
                            if (value != null)
                            {
                                if (value.ToString().ToLower().Equals(depenencyFilterPropertyValue.ToLower()))
                                {
                                    filteredSource.Add(item);
                                }
                            }
                        }
                    }
                    result = filteredSource;
                }
            }
            else
                result = await BaseClient.Get(new GetDataSetRequestDTO() { FilterExpressions = filterExpressions, RequestedEntriesNumber = null }) ?? new List<T>();

            ItemsSource = new ObservableCollection<T>(result);
        }
        #endregion
    }
}
