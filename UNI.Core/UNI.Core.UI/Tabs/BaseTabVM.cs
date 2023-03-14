using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using UNI.API.Client;
using UNI.API.Contracts.RequestsDTO;
using UNI.Core.Library;
using UNI.Core.Library.Mapping;
using UNI.Core.UI.CustomControls.UniTextBox;
using UNI.Core.UI.CustomEventArgs;
using UNI.Core.UI.MainPage;
using UNI.Core.UI.Misc;
using UNI.Core.UI.NewItem;
using UNI.Core.UI.ViewBuilder;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace UNI.Core.UI.Tabs
{
    /// <summary>
    /// shared logic for T instantiations of single tabs' VMs and all I/O operation between GUI and API Client.
    /// </summary>
    /// <typeparam> T the basemodel final type </typeparam>
    public class BaseTabVM<T> : BaseTabVMTypeAgnostic where T : BaseModel
    {
        #region PROPERTIES (and bindings)
        /// <summary>
        /// The main element in all the inheriting views, populated by ViewBuilder.RenderMainFrameworkElement
        /// </summary>
        private FrameworkElement content;
        public FrameworkElement Content { get => content; set => SetValue(ref content, value); }

        /// <summary>
        /// Main list of items in the view
        /// </summary>
        private ObservableCollection<T> itemsSource;
        public ObservableCollection<T> ItemsSource { get => itemsSource; set => SetValue(ref itemsSource, value); }

        /// <summary>
        /// Selected item XAML-bound
        /// </summary>
        private T selectedItem;
        public T SelectedItem { get => selectedItem; set { SetValue(ref selectedItem, value); SelectedItemChanged(); } }

        /// <summary>
        /// Index of the datablock do load
        /// </summary>
        private int selectedDataBlockNumber = 1;
        public int SelectedDataBlockNumber { get => selectedDataBlockNumber; set => SetValue(ref selectedDataBlockNumber, value); }

        /// <summary>
        /// Total datablocks number
        /// </summary>
        private int dataBlocksNumber = 1;
        public int DataBlocksNumber { get => dataBlocksNumber; set => SetValue(ref dataBlocksNumber, value); }

        /// <summary>
        /// Loading flag
        /// </summary>
        private bool isLoading = false;
        public bool IsLoading { get => isLoading; set => SetValue(ref isLoading, value); }

        /// <summary>
        /// The string to search when searching
        /// </summary>
        private string searchText;
        public string SearchText { get => searchText; set => SetValue(ref searchText, value); }

        /// <summary>
        /// Menu sections for sub-object navigation (large BaseModels)
        /// </summary>
        private List<string> navigationViewItems;
        public List<string> NavigationViewItems { get => navigationViewItems; set => SetValue(ref navigationViewItems, value); }

        /// <summary>
        /// For data paging, default 50
        /// </summary>
        private int selectedItemsQuantity = 50;
        public int SelectedItemsQuantity { get => selectedItemsQuantity; set => SetValue(ref selectedItemsQuantity, value); }

        /// <summary>
        /// Format of date to search with
        /// </summary>
        public string FilterDateFormat { get; set; } = "%Y-%m-%d";

        /// <summary>
        /// The ViewBuilder of the view, which will be extended by the final VM
        /// </summary>
        public ViewBuilder<T> ViewBuilder { get; set; }

        /// <summary>
        /// New Item instance for this type
        /// </summary>
        public NewItemVM<T> NewItemVM { get; set; }

        /// <summary>
        /// To inject customized NewItemVM
        /// </summary>
        public Type NewItemType { get; set; }

        /// <summary>
        /// Navigationview item used in the page Group logic
        /// </summary>
        protected string navigationViewItem;

        /// <summary>
        /// The API Client for this view
        /// </summary>
        readonly protected UNIClient<T> BaseClient = null;

        /// <summary>
        /// 
        /// </summary>
        protected int? BackupItemId = null;

        /// <summary>
        /// Use these 2 in extending VMs to determine Create Item Command behaviour
        /// </summary>
        protected Type CreateItemVMType;
        protected string CreateItemTabName;
        #endregion

        #region Public Surface
        public void CreateOrEditItemShouldOpenInNewTab(Type createItemVMType, string createItemTabName)
        {
            CreateItemVMType = createItemVMType;
            CreateItemTabName = createItemTabName;
        }
        public void CreateOrEditItemShouldOpenInContentDialog()
        {
            CreateItemVMType = null;
            CreateItemTabName = string.Empty;
        }
        #endregion

        #region CTOR
        public BaseTabVM()
        {
            BaseClient = new UNIClient<T>();

            BindCommands();

            Window.Current.SizeChanged += Current_SizeChanged;
            ItemUpdated += Tab_ItemUpdated;
        }

        /// <summary>
        /// Called on window size changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e) => DrawDetailsPane(navigationViewItem ?? null);

        #endregion

        #region COMMANDS
        /// <summary>
        /// Show the Content Dialog for the creation of any new object
        /// </summary>
        public ICommand CreateItem { get; set; }

        /// <summary>
        /// Show the content dialog for the item delete
        /// </summary>
        public ICommand DeleteItem { get; set; }

        /// <summary>
        /// Edit command for some derived VMs
        /// </summary>
        public ICommand EditItem { get; set; }

        /// <summary>
        /// Reload data
        /// </summary>
        public ICommand RefreshItems { get; set; }

        /// <summary>
        /// Opens Windows Print/PDF dialog to print/export a paper-friendly version of the current view
        /// </summary>
        public ICommand ExportItems { get; set; }

        /// <summary>
        /// Update items with the next data block
        /// </summary>
        public ICommand GetNextItems { get; set; }

        /// <summary>
        /// Update items with the previous data block
        /// </summary>
        public ICommand GetPreviousItems { get; set; }

        /// <summary>
        /// Reload data blocks passing the filter text search. Called by the search button and pressing enter when the search bar is focused
        /// </summary>
        public ICommand Search { get; set; }

        /// <summary>
        /// Manages the view in case of the changed selection of the navigation view created by PageGroup Attribute
        /// </summary>
        public ICommand NavigationViewSelectionChanged { get; set; }

        /// <summary>
        /// Combobox for adjusting number of elements changed
        /// </summary>
        public ICommand SelectedItemsQuantityChanged { get; set; }

        private void BindCommands()
        {
            CreateItem = new RelayCommand(async (parameter) =>
            {
                if (CreateItemVMType == null && string.IsNullOrEmpty(CreateItemTabName))
                {
                    if (NewItemType != null)
                        NewItemVM = (NewItemVM<T>)Activator.CreateInstance(NewItemType);
                    else
                        NewItemVM = new NewItemVM<T>();

                    NewItemVM.ItemUpdated += async (s, e) => await LoadData();
                    var newItem = new NewItem.NewItem() { DataContext = NewItemVM };
                    TabViewVM.ShowContentDialog(newItem);
                }
                else
                {
                    SelectedItem = ModelFactory.CreateModel<T>();

                    var response = await BaseClient.CreateItem(SelectedItem);
                    if (response != null)
                    {
                        SelectedItem = response;
                    }

                    UNICompositionRoot.TabViewVM.OpenNewTab(name: CreateItemTabName,
                                         viewModelType: CreateItemVMType,
                                         args: new object[] { SelectedItem });
                }
            });

            DeleteItem = new RelayCommand((parameter) =>
            {
                var resLoad = ResourceLoader.GetForCurrentView();
                var contentDialog = new ContentDialog()
                {
                    Title = resLoad.GetString("warning"),
                    Content = resLoad.GetString("confirm_Delete"),
                    CloseButtonText = resLoad.GetString("cancel"),
                    PrimaryButtonText = resLoad.GetString("confirm")
                };
                contentDialog.PrimaryButtonClick += async (sender, args) =>
                {
                    if (Content is DataGrid dg)
                    {
                        foreach (var item in dg.SelectedItems)
                            _ = await BaseClient.DeleteItem(item as T);
                    }
                    else
                        _ = await BaseClient.DeleteItem(SelectedItem);

                    await LoadData();
                };
                TabViewVM.ShowContentDialog(contentDialog);
            });

            GetNextItems = new RelayCommand(async (parameter) =>
            {
                if (SelectedDataBlockNumber < DataBlocksNumber)
                {
                    SelectedDataBlockNumber++;
                    await LoadData();
                }
            });

            GetPreviousItems = new RelayCommand(async (parameter) =>
            {
                if (SelectedDataBlockNumber > 1)
                {
                    SelectedDataBlockNumber--;
                    await LoadData();
                }
            });

            RefreshItems = new RelayCommand(async (parameter) => await LoadData());

            ExportItems = new RelayCommand((parameter) =>
            {
                if (SelectedItem != null)
                {
                    SelectedItem.Updated();
                    SelectedItem.Loaded();

                    MainPageVM.Print(this, ViewBuilder.GetControlsForPrinting(SelectedItem));
                }
            });

            Search = new RelayCommand(async (parameter) =>
            {
                if (parameter is null
                || (parameter is KeyRoutedEventArgs args && args.Key == Windows.System.VirtualKey.Enter)
                || (parameter is string par && par == "universalSearch"))
                {
                    SelectedDataBlockNumber = 1;
                    DataBlocksNumber = 1;
                    await LoadData(parameter: parameter);
                }
            });

            NavigationViewSelectionChanged = new RelayCommand((parameter) =>
            {
                if (parameter is Windows.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
                {
                    string baseitem = ResourceLoader.GetForCurrentView().GetString($"{typeof(T).Name}");
                    if (string.IsNullOrWhiteSpace(baseitem))
                        baseitem = typeof(T).Name;
                    if (SelectedItem != null)
                    {
                        navigationViewItem = args.SelectedItem.ToString();
                        if (navigationViewItem != baseitem)
                            DrawDetailsPane(navigationViewItem);
                        else
                            DrawDetailsPane();
                    }
                }
            });

            SelectedItemsQuantityChanged = new RelayCommand((parameter) => _ = LoadData());
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Called by some VMs when all dependencies are resolved.
        /// </summary>
        protected void DependencyInitialized()
        {
            //view builder events method associated
            ViewBuilder.ItemUpdated += ViewBuilder_ItemUpdated;
            ViewBuilder.LostFocus += ViewBuilder_LostFocus;


            var navViewItems = new List<string>();
            string baseitem = ResourceLoader.GetForCurrentView().GetString($"{typeof(T).Name}");
            if (string.IsNullOrWhiteSpace(baseitem))
                baseitem = typeof(T).Name;

            navViewItems.Add(baseitem);

            foreach (PropertyInfo p in typeof(T).GetProperties())
            {
                if (p.GetCustomAttribute(typeof(RenderInfo)) is RenderInfo renderInfo)
                {
                    if (!string.IsNullOrWhiteSpace(renderInfo.PageGroup))
                    {
                        string item = ResourceLoader.GetForCurrentView().GetString($"group_{typeof(T).Name}_{renderInfo.PageGroup}");
                        if (string.IsNullOrWhiteSpace(item))
                            item = renderInfo.PageGroup;

                        if (!navViewItems.Contains(item))
                            navViewItems.Add(item);
                    }
                }
            }

            NavigationViewItems = navViewItems;
        }

        protected bool ValidationSuccesful()
        {
            if (Content is Grid grid)
                foreach (var utb in grid.Children.Where(c => c.GetType() == typeof(UniTextBox)).Select(c => (UniTextBox)c))
                    if (!utb.ValidationSuccesful)
                        return false;

            return true;
        }

        //used to find a value that identifies the item for example the main numeration number or the displaymember path
        protected string FindIdentifierValue(T item)
        {
            if (item != null)
                foreach (var property in typeof(T).GetProperties())
                    if (property.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo valueInfo)
                        if (valueInfo.IsMainNumeration || valueInfo.IsDisplayProperty)
                            return property.GetValue(item)?.ToString();

            return string.Empty;
        }

        /// <summary>
        /// To refresh the main element calling the proper version of RenderMainFrameworkElement in the derived VMs
        /// </summary>
        protected virtual void DrawDetailsPane(string navigationViewItemName = null) { }

        /// <summary>
        /// Override this if redraw is not required
        /// </summary>
        protected virtual void SelectedItemChanged()
        {
            DrawDetailsPane(navigationViewItem ?? null);
        }
        #endregion

        #region Main method: LoadData
        /// <summary>
        /// Main method to load data
        /// </summary>
        /// <param name="filterExpressions"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected virtual async Task LoadData(List<FilterExpression> filterExpressions = null, object parameter = null)
        {
            IsLoading = true;

            if (SelectedItem != null)
                BackupItemId = SelectedItem.ID;

            var requestDto = new GetDataSetRequestDTO
            {
                BlockToReturn = SelectedDataBlockNumber,
                RequestedEntriesNumber = SelectedItemsQuantity,
                FilterText = SearchText,
                FilterExpressions = filterExpressions,
                FilterDateFormat = FilterDateFormat
            };

            var apiResponse = await BaseClient.GetDataSet(requestDto);

            DataBlocksNumber = apiResponse?.DataBlocks ?? 0;

            ItemsSource = new ObservableCollection<T>();
            if (apiResponse?.ResponseBaseModels != null)
                ItemsSource = new ObservableCollection<T>(apiResponse.ResponseBaseModels);

            // to select the same object in list in case of update
            if (BackupItemId.HasValue)
            {
                if (ItemsSource.Any())
                    SelectedItem = ItemsSource.FirstOrDefault(i => i.ID == BackupItemId);
            }

            DrawDetailsPane(navigationViewItem ?? null);

            IsLoading = false;
        }

        protected async Task LoadSingle(int id)
        {
            IsLoading = true;

            List<T> items = await BaseClient.Get(new GetDataSetRequestDTO() { Id = id, FilterDateFormat = FilterDateFormat }) ?? new List<T>();
            if (items.Any())
            {
                // this also calls DrawDetailsPane as SelectedItem changes
                SelectedItem = items.First();
                SelectedItem.Loaded();
            }
            //ItemsLoaded();

            IsLoading = false;
        }
        #endregion

        #region ViewBuilder events
        /// <summary>
        /// when a dependency object is updated like for example the content of a gridbox refresh the model
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual async void ViewBuilder_ItemUpdated(object sender, ItemUpdatedEventArgs e)
        {
            await BaseClient.UpdateItem(SelectedItem); // save current changes
            SelectedItem.Updated();
            await LoadData();
        }

        /// <summary>
        /// When other tabs update
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual async void Tab_ItemUpdated(object sender, ItemUpdatedEventArgs e)
        {
            SelectedItem?.Updated();
            await LoadData();
        }

        /// <summary>
        /// When a control lose focus execute a refresh of the current model calling NotifyPropertyChangedGlobal
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void ViewBuilder_LostFocus(object sender, RoutedEventArgs e)
        {
            SelectedItem?.Updated();
        }

        private void ViewBuilder_NavigateToTab(object sender, NavigationTabEventArgs e)
        {
            UNICompositionRoot.TabViewVM.OpenNewTab(
                   name: e.Node.Name,
                   viewModelType: e.Node.ViewModelType,
                   args: new object[] { e.Node.ArgumentObject });
        }

        /// <summary>
        /// When a dependency object is selected calling the method UpdateItemProperty the choosen object is assigned to the model. This is to the impossibility to Bind the selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void ViewBuilder_ObjectSelected(object sender, ObjectSelectedEventArgs e)
        {
            UpdateItemProperty(SelectedItem, e.Item, e.PropertyInfo);
            SelectedItem.Updated();
        }

        void UpdateItemProperty(BaseModel item, object value, PropertyInfo property)
        {
            if (value != null)
            {
                Type itemType = item.GetType();
                List<PropertyInfo> propertyInfos = itemType.GetProperties().ToList() ?? new List<PropertyInfo>();
                PropertyInfo propertyInfo = propertyInfos.Find(p => p.Name == property.Name);

                if (propertyInfo != null)
                    propertyInfo.SetValue(item, value);
                else //if cannot find property search in interfaces
                    foreach (PropertyInfo p in propertyInfos.FindAll(p => p.PropertyType.IsInterface))
                        if (value.GetType().GetInterfaces().ToList().Contains(p.PropertyType))
                            p.SetValue(item, value);
            }
        }

        #endregion
    }
}
