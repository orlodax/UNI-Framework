using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using UNI.API.Contracts.RequestsDTO;
using UNI.Core.Client;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;
using UNI.Core.UI.Misc;
using UNI.Core.UI.Tabs;
using UNI.Core.UI.Tabs.ListDetail;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace UNI.Core.UI.CustomControls.SelectorBox
{
    // TODO usato in combinazione con showbox, vanno sostituiti entrambi 
    public class SelectorBoxVM<T> : BaseTabVMTypeAgnostic where T : BaseModel
    {
        #region Props and fields
        public DataTemplate ItemTemplate { get; set; }
        public string SearchPlaceHolderText { get; set; }


        private ObservableCollection<T> itemsSource;
        public ObservableCollection<T> ItemsSource { get => itemsSource; set => SetValue(ref itemsSource, value); }

        private T selectedItem;
        public T SelectedItem { get => selectedItem; set => SetValue(ref selectedItem, value); }

        private string searchText = string.Empty;
        public string SearchText { get => searchText; set => SetValue(ref searchText, value); }

        private bool isLoading = false;
        public bool IsLoading { get => isLoading; set => SetValue(ref isLoading, value); }

        private int selectedDataBlockNumber = 1;
        public int SelectedDataBlockNumber { get => selectedDataBlockNumber; set => SetValue(ref selectedDataBlockNumber, value); }

        /// <summary>
        /// Total number of data blocks
        /// </summary>
        private int dataBlocksNumber = 1;
        public int DataBlocksNumber { get => dataBlocksNumber; set => SetValue(ref dataBlocksNumber, value); }



        private readonly BaseModel Parent;
        private readonly string DependencyFilterPropertyName;
        private readonly string ParentFilterPropertyName;
        private readonly string DependencyFilterPropertyValue;
        readonly protected UniClient<T> BaseClient;
        private const int pageSize = 50;
        #endregion

        public SelectorBoxVM(BaseModel parent, string dependencyFilterPropertyName, string parentFilterPropertyName, string dependencyFilterPropertyValue)
        {
            Parent = parent;
            DependencyFilterPropertyName = dependencyFilterPropertyName;
            ParentFilterPropertyName = parentFilterPropertyName;
            DependencyFilterPropertyValue = dependencyFilterPropertyValue;

            BaseClient = new UniClient<T>();

            ItemTemplate = new ListDetailVB<T>().SelectItemListTemplate();
            SearchPlaceHolderText = ResourceLoader.GetForCurrentView().GetString("search_PlaceholderText");

            BindCommands();

            _ = LoadData();
        }

        #region Commands
        /// <summary>
        /// Command for next items loading
        /// </summary>
        public ICommand NextItems { get; set; }
        /// <summary>
        /// Command for previous items
        /// </summary>
        public ICommand PrevItems { get; set; }

        //TODO inutilizzato al momento?
        public ICommand CreateItem { get; private set; }
        /// <summary>
        /// Called by the keydown event of the textbox search for enter key and also by the button search
        /// </summary>
        public ICommand Search { get; set; }

        private void BindCommands()
        {
            CreateItem = new RelayCommand((parameter) =>
            {
                //NewItemVM<T> newItemVM = (NewItemVM<T>)ClassImplementationHelper.GetNewItemDataContext<T>(new object[] { parent });
                //newItemVM.ItemUpdated += NewItemVM_ItemUpdated;
                //var newItem = new NewItem.NewItem() { DataContext = newItemVM };
                //TabViewVM.ShowContentDialog(newItem);
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

            Search = new RelayCommand((parameter) =>
            {
                if (parameter == null || (parameter is KeyRoutedEventArgs args && args.Key == Windows.System.VirtualKey.Enter))
                {
                    SelectedDataBlockNumber = 1;
                    DataBlocksNumber = 1;
                    _ = LoadData();
                }
            });
        }

        //TODO inutilizzato al momento?
        //After creation of a new item in the newitem, the item is inserted inside the itemsource
        //private void NewItemVM_ItemUpdated(object sender, CustomEventArgs.ItemUpdatedEventArgs e)
        //{
        //    ItemsSource.Insert(0, (T)Convert.ChangeType(e.Item, typeof(T)));
        //    SelectedItem = ItemsSource[0];
        //}
        #endregion

        /// <summary>
        /// Override Load data using unidataset
        /// </summary>
        public virtual async Task LoadData(List<FilterExpression> filterExpressions = null)
        {
            IsLoading = true;

            int? parentPropertyId = null;

            if (!string.IsNullOrWhiteSpace(DependencyFilterPropertyName))
            {
                if (!string.IsNullOrWhiteSpace(ParentFilterPropertyName))
                {
                    var property = Parent.GetType().GetProperty(ParentFilterPropertyName);
                    if (property != null)
                        parentPropertyId = (int)property.GetValue(Parent, null);
                }
                else if (!string.IsNullOrWhiteSpace(DependencyFilterPropertyValue))
                    filterExpressions = new List<FilterExpression> { new FilterExpression() { PropertyName = DependencyFilterPropertyName, PropertyValue = DependencyFilterPropertyValue } };
            }

            ApiResponseModel<T> apiResponse;
            if (parentPropertyId.HasValue && parentPropertyId != 0)
            {
                var request = new GetDataSetRequestDTO() { Id = parentPropertyId, IdName = DependencyFilterPropertyName, RequestedEntriesNumber = pageSize, FilterExpressions = filterExpressions, BlockToReturn = SelectedDataBlockNumber, FilterText = searchText };
                apiResponse = await BaseClient.GetDataSet(request);
            }
            else
            {
                var request = new GetDataSetRequestDTO() { FilterExpressions = filterExpressions, RequestedEntriesNumber = pageSize, BlockToReturn = SelectedDataBlockNumber, FilterText = searchText };
                apiResponse = await BaseClient.GetDataSet(request);
            }

            DataBlocksNumber = apiResponse?.DataBlocks ?? 0;

            ItemsSource = new ObservableCollection<T>(apiResponse.ResponseBaseModels ?? new List<T>());

            IsLoading = false;
        }
    }
}
