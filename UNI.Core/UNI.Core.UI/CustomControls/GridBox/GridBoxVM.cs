using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using UNI.Core.Client;
using UNI.Core.Library;
using UNI.Core.UI.CustomEventArgs;
using UNI.Core.UI.MainPage;
using UNI.Core.UI.Misc;
using UNI.Core.UI.NewItem;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UNI.Core.UI.CustomControls.GridBox
{
    public class GridBoxVM<T> : GridBoxViewVM<T> where T : BaseModel
    {
        #region Properties/Bindings/Fields
        /// <summary>
        /// 
        /// </summary>
        internal T selectedItem;
        public T SelectedItem { get => selectedItem; set { SetValue(ref selectedItem, value); } }

        /// <summary>
        /// 
        /// </summary>
        internal bool isLoading;
        public bool IsLoading { get => isLoading; set => SetValue(ref isLoading, value); }

        /// <summary>
        /// Constructed with activator and specified in Controls Factory via ViewModelResolver, will be anything inheriting this basetype
        /// </summary>
        protected NewItemVM<T> NewItemVM;
        protected EditItemVM<T> EditItemVM;
        protected Type editItemVMType;
        protected Type newItemVMType;

        /// <summary>
        /// The property in the BaseModel from which this control/VM was generated
        /// </summary>
        protected readonly PropertyInfo PropertyInfo;

        /// <summary>
        /// The BaseModel containing the nested object for this control
        /// </summary>
        protected BaseModel ParentItem;

        protected readonly UniClient<T> BaseClient = new UniClient<T>();


        /// <summary>
        /// Use these 2 in extending VMs to determine Create Item Command behaviour
        /// </summary>
        private Type createItemVMType;
        private string createItemTabName;

        /// <summary>
        /// All changed items during grid edit
        /// </summary>
        private readonly List<T> itemsToCommit = new List<T>();

        #endregion

        #region Public Surface

        public void CreateOrEditItemShouldOpenInNewTab(Type createItemVMType, string createItemTabName)
        {
            this.createItemVMType = createItemVMType;
            this.createItemTabName = createItemTabName;
        }
        #endregion

        #region CTOR

        public GridBoxVM(BaseModel parentItem,
                         List<T> itemsSource,
                         string name,
                         PropertyInfo propertyInfo,
                         Type newItemType,
                         Type editItemType) : base(itemsSource, name)
        {
            CreateItemVisibility = Visibility.Visible;
            SaveItemVisibility = Visibility.Visible;
            EditItemVisibility = Visibility.Visible;
            DeleteItemVisibility = Visibility.Visible;

            BaseClient = new UniClient<T>();
            PropertyInfo = propertyInfo;
            ParentItem = parentItem;

            BindCommands();

            ViewBuilder.RowEditEnding += RowEditEnding;

            newItemVMType = newItemType;
            editItemVMType = editItemType;

            MainGrid = (DataGrid)ViewBuilder.RenderMainFrameworkElement(height: 300);

            notFilteredItemsSource = itemsSource ?? new List<T>();
            PopulateItemsSource();
        }


        private void RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (!itemsToCommit.Contains(SelectedItem))
                itemsToCommit.Add(SelectedItem);
        }
        #endregion

        #region Commands
        public ICommand CreateItem { get; set; }
        public ICommand DeleteItem { get; set; }
        public ICommand EditItem { get; set; }
        public ICommand SaveGridItems { get; set; }

        private void BindCommands()
        {
            EditItem = new RelayCommand(EditItemCommand);
            SaveGridItems = new RelayCommand(SaveGridItemsCommand);

            CreateItem = new RelayCommand(async (parameter) =>
            {
                if (itemsToCommit.Any())
                    SaveGridItemsCommand(null);

                if (createItemVMType == null && string.IsNullOrEmpty(createItemTabName))
                {
                    if (ParentItem != null)
                    {
                        if (newItemVMType.IsGenericType)
                            NewItemVM = (NewItemVM<T>)Activator.CreateInstance(newItemVMType.MakeGenericType(new Type[] { typeof(T) }), new object[] { ParentItem });
                        else
                            NewItemVM = (NewItemVM<T>)Activator.CreateInstance(newItemVMType, new object[] { ParentItem });

                        NewItemVM.ItemUpdated += ItemVM_ItemUpdated;

                        TabViewVM.ShowContentDialog(new NewItem.NewItem() { DataContext = NewItemVM });
                    }
                }
                else
                {
                    SelectedItem = ModelFactory.CreateModel<T>();

                    var response = await BaseClient.CreateItem(SelectedItem);
                    if (response != null)
                    {
                        SelectedItem = response;
                    }

                    UNICompositionRoot.TabViewVM.OpenNewTab(name: createItemTabName,
                                         viewModelType: createItemVMType,
                                         args: new object[] { SelectedItem });
                }
            });

            DeleteItem = new RelayCommand((parameter) =>
            {
                if (SelectedItem != null)
                {
                    var contentDialog = new ContentDialog() { Title = "Attenzione", Content = "Sei sicuro di voler eliminare questa voce?", CloseButtonText = "Annulla", PrimaryButtonText = "Conferma" };
                    contentDialog.PrimaryButtonClick += ContentDialog_PrimaryButtonClick;
                    TabViewVM.ShowContentDialog(contentDialog);
                }
            });
        }


        protected virtual async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (itemsToCommit.Any())
                SaveGridItemsCommand(null);

            if (SelectedItem != null)
            {
                int statusCode = await BaseClient.DeleteItem(SelectedItem);
                if (statusCode == 200)
                {
                    ItemsSource.Remove(SelectedItem);
                    OnItemUpdated(this, new ItemUpdatedEventArgs(null));
                }
            }
        }

        /// <summary>
        /// Method to edit an item
        /// </summary>
        /// <param name="parameter"></param>
        public virtual void EditItemCommand(object parameter)
        {
            if (itemsToCommit.Any())
                SaveGridItemsCommand(null);

            if (SelectedItem != null)
            {
                if (createItemVMType == null && string.IsNullOrEmpty(createItemTabName))
                {
                    if (editItemVMType.IsGenericType)
                        EditItemVM = (EditItemVM<T>)Activator.CreateInstance(editItemVMType.MakeGenericType(typeof(T)), new object[] { SelectedItem });
                    else
                        EditItemVM = (EditItemVM<T>)Activator.CreateInstance(editItemVMType, new object[] { SelectedItem });

                    EditItemVM.ItemUpdated += ItemVM_ItemUpdated;
                    TabViewVM.ShowContentDialog(new EditItem.EditItem() { DataContext = EditItemVM });

                }
                else
                {
                    UNICompositionRoot.TabViewVM.OpenNewTab(name: createItemTabName,
                                         viewModelType: createItemVMType,
                                         args: new object[] { SelectedItem });
                }
            }
        }

        public virtual void ItemVM_ItemUpdated(object sender, ItemUpdatedEventArgs e)
        {
            if (e.Item != null)
            {
                T item = (T)Convert.ChangeType(e.Item, typeof(T));
                if (notFilteredItemsSource.Find(i => i.ID == item.ID) == null) //if the list not contains the selected base model
                    notFilteredItemsSource.Add(item);

                PopulateItemsSource();
            }

            OnItemUpdated(this, e);
        }

        /// <summary>
        /// Method to save direct modifications to the grid
        /// </summary>
        /// <param name="parameter"></param>
        public async void SaveGridItemsCommand(object parameter)
        {
            var notSuccessfulItems = new List<T>();
            try
            {
                foreach (T item in itemsToCommit)
                {
                    int statusCode = await BaseClient.UpdateItem(item);
                    if (statusCode >= 400)
                    {
                        notSuccessfulItems.Add(item);
                        _ = new TeachingTip()
                        {
                            Title = ResourceLoader.GetForCurrentView().GetString("error"),
                            Subtitle = $"error n. {statusCode}",
                            IsLightDismissEnabled = true,
                            IsOpen = true
                        };
                    }
                    else
                        OnItemUpdated(this, new ItemUpdatedEventArgs(item));
                }
                itemsToCommit.RemoveAll(i => !notSuccessfulItems.Contains(i));
            }
            catch (Exception)
            {

            }
        }

        #endregion

        /// <summary>
        /// Main method to retrieve data
        /// </summary>
        /// <param name="filterExpressions"></param>
        protected override void PopulateItemsSource(List<FilterExpression> filterExpressions = null)
        {
            ItemsSource = new ObservableCollection<T>(notFilteredItemsSource);

            FilterListCommand();

            foreach (DataGridColumn column in MainGrid.Columns)
                if (column.GetType().IsGenericType)
                    if (column.GetType().GetGenericTypeDefinition() == typeof(DataGridBaseModelColumn<>))
                        column.GetType().GetMethod("LoadData").Invoke(column, new object[] { ParentItem });
        }

        /// <summary>
        /// Method to filter the itemsource list based on the content of the searchtext property
        /// </summary>
        protected override void FilterListCommand()
        {
            var filteredItemsSource = new List<T>();

            if (!string.IsNullOrWhiteSpace(SearchText) && typeof(T).GetProperties().Any())
            {
                foreach (T item in notFilteredItemsSource.Where(i => i != null))
                    foreach (PropertyInfo property in typeof(T).GetProperties())
                        if (property.PropertyType.IsSubclassOf(typeof(BaseModel)) || !property.PropertyType.IsGenericType)
                            if (Convert.ToString(property?.GetValue(item)).Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                                if (!filteredItemsSource.Contains(item))
                                    filteredItemsSource.Add(item);

                if (filteredItemsSource.Any())
                    ItemsSource = new ObservableCollection<T>(filteredItemsSource);
            }
        }
    }
}
