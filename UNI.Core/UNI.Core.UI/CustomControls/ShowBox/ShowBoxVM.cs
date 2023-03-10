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
using UNI.Core.Library.GenericModels;
using UNI.Core.UI.MainPage;
using UNI.Core.UI.Misc;
using UNI.Core.UI.NewItem;
using UNI.Core.UI.Tabs;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;

namespace UNI.Core.UI.CustomControls.ShowBox
{
    public class ShowBoxVM<T> : BaseTabVMTypeAgnostic where T : BaseModel
    {
        #region Properties and fields 
        /// <summary>
        /// Label (name) of the property populated by this control
        /// </summary>
        public string MemberName { get; set; }

        /// <summary>
        /// "Search here" in various languages
        /// </summary>
        public string SearchBoxPlaceHolderText { get; set; }

        /// <summary>
        /// Template for the ListView
        /// </summary>
        public DataTemplate ItemTemplate { get; set; }

        /// <summary>
        /// Property of the selected item to show 
        /// </summary>
        public PropertyInfo DisplayPropertyInfo { get; private set; }

        /// <summary>
        /// The search expression
        /// </summary>
        private string searchBoxText;
        public string SearchBoxText { get => searchBoxText; set => SetValue(ref searchBoxText, value); }

        /// <summary>
        /// What is actually shown in the textbox. Depends from ValueInfo attribute in the object's properties
        /// </summary>
        private string displayProperty;
        public string DisplayProperty { get => displayProperty; set => SetValue(ref displayProperty, value); }

        /// <summary>
        /// The selection from the list. It is the dependecy passed to this control from its parent BaseModel
        /// </summary>
        private T selectedItem;
        public T SelectedItem { get => selectedItem; set => SetValue(ref selectedItem, value); }

        /// <summary>
        /// List fed incrementally
        /// </summary>
        private ObservableCollection<T> itemsSource;
        public ObservableCollection<T> ItemsSource { get => itemsSource; set => SetValue(ref itemsSource, value); }

        /// <summary>
        /// Label for results pages
        /// </summary>
        private int selectedDataBlockNumber = 1;
        public int SelectedDataBlockNumber { get => selectedDataBlockNumber; set => SetValue(ref selectedDataBlockNumber, value); }

        /// <summary>
        /// Total number of data blocks (pages)
        /// </summary>
        private int dataBlocksNumber = 1;
        public int DataBlocksNumber { get => dataBlocksNumber; set => SetValue(ref dataBlocksNumber, value); }

        /// <summary>
        /// Loading ring is displayed when waiting api response
        /// </summary>
        private bool isLoading = false;
        public bool IsLoading { get => isLoading; set => SetValue(ref isLoading, value); }

        /// <summary>
        /// Creation button might need to be hidden (extended showbox vms)
        /// </summary>
        private Visibility newItemButtonVisibility = Visibility.Visible;
        public Visibility NewItemButtonVisibility { get => newItemButtonVisibility; set => SetValue(ref newItemButtonVisibility, value); }

        // property containing the selected item
        private readonly PropertyInfo PropertyInfo;
        // basemodel containg the property
        private readonly BaseModel Parent;
        // optional filters defined in the object's attributes
        private readonly ShowBoxFilters Filters;
        // api client 
        private readonly UNIClient<T> BaseClient;


        // customizable newItemVM
        protected Type NewItemType;
        private NewItemVM<T> NewItemVM;
        // To page the api requests for the list 
        private const int pageSize = 50;

        private readonly ResourceLoader ResourceLoader;

        /// <summary>
        /// Use these 2 in extending VMs to determine Create Item Command behaviour
        /// </summary>
        protected Type CreateItemVMType;
        protected string CreateItemTabName;

        #endregion

        #region CTOR
        public ShowBoxVM(T dependency, string memberName, PropertyInfo propertyInfo, BaseModel parent, ShowBoxFilters filters, Type newItemType)
        {
            SelectedItem = dependency;
            MemberName = memberName;
            PropertyInfo = propertyInfo;
            Parent = parent;
            Filters = filters;
            NewItemType = newItemType;

            if (Application.Current.Resources.TryGetValue(typeof(T).Name + "_ShowBox", out object template))
                ItemTemplate = template as DataTemplate;

            ResourceLoader = ResourceLoader.GetForCurrentView();
            SearchBoxPlaceHolderText = ResourceLoader.GetString("showBox_SearchBoxPlaceHolderText");

            BaseClient = new UNIClient<T>();

            DisplayProperty = GetDisplayProperty();

            BindCommands();

            // performs load all data by default
            _ = LoadData();
        }

        internal string GetDisplayProperty()
        {
            DisplayPropertyInfo = SelectedItem?.GetType().GetProperties().First(pinfo => pinfo.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo valueInfo && valueInfo.IsDisplayProperty);
            return Convert.ToString(DisplayPropertyInfo?.GetValue(SelectedItem));
        }
        #endregion

        #region Commands
        /// <summary>
        /// Add new item in the list item source
        /// </summary>
        public ICommand CreateNewItem { get; private set; }

        /// <summary>
        /// When user digits in the search box
        /// </summary>
        public ICommand SearchBoxTextChanged { get; private set; }

        /// <summary>
        /// Picking an object from the list
        /// </summary>
        public ICommand SelectionChanged { get; internal set; }

        /// <summary>
        /// Command for next items loading
        /// </summary>
        public ICommand NextItems { get; set; }

        /// <summary>
        /// Command for previous items
        /// </summary>
        public ICommand PrevItems { get; set; }

        /// <summary>
        /// Command for previous items
        /// </summary>
        public ICommand ShowBoxOpened { get; set; }

        private void BindCommands()
        {
            CreateNewItem = new RelayCommand(async (parameter) =>
            {
                if (CreateItemVMType == null && string.IsNullOrEmpty(CreateItemTabName))
                {
                    if (NewItemType.IsGenericType)
                        NewItemVM = (NewItemVM<T>)Activator.CreateInstance(NewItemType.MakeGenericType(typeof(T)), new object[] { Parent });
                    else
                        NewItemVM = (NewItemVM<T>)Activator.CreateInstance(NewItemType, new object[] { Parent });

                    TabViewVM.ShowContentDialog(new NewItem.NewItem() { DataContext = NewItemVM });
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
                _ = LoadData();
            });

            SearchBoxTextChanged = new RelayCommand((parameter) =>
            {
                SelectedDataBlockNumber = 1;
                DataBlocksNumber = 1;
                _ = LoadData();
            });

            SelectionChanged = new RelayCommand((parameter) =>
            {
                if (SelectedItem != null)
                {
                    DisplayProperty = GetDisplayProperty();
                    PropertyInfo.SetValue(Parent, SelectedItem);
                    SelectedItem?.Updated();
                }
            });

            NextItems = new RelayCommand((parameter) =>
            {
                if (SelectedDataBlockNumber < DataBlocksNumber)
                {
                    SelectedDataBlockNumber++;
                    _ = LoadData();
                }
            });

            PrevItems = new RelayCommand((parameter) =>
            {
                if (SelectedDataBlockNumber > 1)
                {
                    SelectedDataBlockNumber--;
                    _ = LoadData();
                }
            });

            ShowBoxOpened = new RelayCommand((parameter) =>
            {
                _ = LoadData();
            });
        }
        #endregion

        public virtual async Task LoadData(List<FilterExpression> filterExpressions = null)
        {
            IsLoading = true;

            int? parentPropertyId = null;

            if (!string.IsNullOrWhiteSpace(Filters.DependencyFilterPropertyName))
            {
                if (!string.IsNullOrWhiteSpace(Filters.ParentFilterPropertyName))
                {
                    var property = Parent.GetType().GetProperty(Filters.ParentFilterPropertyName);
                    if (property != null)
                        parentPropertyId = (int)property.GetValue(Parent, null);
                }
                else if (!string.IsNullOrWhiteSpace(Filters.DependencyFilterPropertyValue))
                    filterExpressions = new List<FilterExpression> { new FilterExpression() { PropertyName = Filters.DependencyFilterPropertyName, PropertyValue = Filters.DependencyFilterPropertyValue } };
            }

            ApiResponseModel<T> apiResponse;

            var request = new GetDataSetRequestDTO()
            {
                RequestedEntriesNumber = pageSize,
                FilterExpressions = filterExpressions,
                BlockToReturn = SelectedDataBlockNumber,
                FilterText = SearchBoxText
            };

            if (parentPropertyId.HasValue && parentPropertyId != 0)
            {
                request.Id = parentPropertyId;
                request.IdName = Filters.DependencyFilterPropertyName;
            }

            apiResponse = await BaseClient.GetDataSet(request);

            DataBlocksNumber = apiResponse?.DataBlocks ?? 0;

            ItemsSource = new ObservableCollection<T>(apiResponse?.ResponseBaseModels ?? new List<T>());

            IsLoading = false;
        }
    }
}