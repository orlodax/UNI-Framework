using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using UNI.Core.Library;
using UNI.Core.UI.Components.SearchFilters;
using UNI.Core.UI.MainPage;
using UNI.Core.UI.Misc;
using UNI.Core.UI.Tabs.DetailItem;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;

namespace UNI.Core.UI.Tabs.ListGrid
{
    public class ListGridVM<T> : BaseTabVM<T> where T : BaseModel
    {
        #region Props and fields
        /// <summary>
        /// To customize the detail item
        /// </summary>
        public Type DetailItemVMType { get; set; }

        /// <summary>
        /// VM and customise the search component
        /// </summary>
        public SearchFilterSetup SearchFilterSetup { get; set; }

        private SearchFiltersVM<T> searchFiltersVM;
        public SearchFiltersVM<T> SearchFiltersVM { get => searchFiltersVM; set => SetValue(ref searchFiltersVM, value); }

        /// <summary>
        /// Searchbar placeholder text
        /// </summary>
        public string PlaceholderText { get; set; }

        /// <summary>
        /// Show/hide datepicker
        /// </summary>
        private Visibility datePickerVisibility = Visibility.Collapsed;
        public Visibility DatePickerVisibility { get => datePickerVisibility; set => SetValue(ref datePickerVisibility, value); }

        /// <summary>
        /// Show/hide universal search
        /// </summary>
        private Visibility universalSearchVisibility = Visibility.Visible;
        public Visibility UniversalSearchVisibility { get => universalSearchVisibility; set => SetValue(ref universalSearchVisibility, value); }

        /// <summary>
        /// The reference date to use in combination for time ranges to build queries for the api
        /// </summary>
        private DateTimeOffset selectedDate = DateTimeOffset.Now;
        public DateTimeOffset SelectedDate { get => selectedDate; set { SetValue(ref selectedDate, value); _ = LoadData(); } }

        /// <summary>
        /// Choices for time rages
        /// </summary>
        private string[] timeRanges = new string[3];
        public string[] TimeRanges { get => timeRanges; set => SetValue(ref timeRanges, value); }

        /// <summary>
        /// Requested range which will be sent as filter expression to the client to retrieve a slice of results
        /// </summary>
        private string selectedTimeRange;
        public string SelectedTimeRange { get => selectedTimeRange; set => SetValue(ref selectedTimeRange, value); }

        /// <summary>
        /// Choices for paging items
        /// </summary>
        private int[] itemsQuantities = { 20, 50, 100, 500, 1000 };
        public int[] ItemsQuantities { get => itemsQuantities; set => SetValue(ref itemsQuantities, value); }

        /// <summary>
        /// All changed items during grid edit
        /// </summary>
        private readonly List<T> ItemsToCommit = new List<T>();

        readonly ResourceLoader ResourceLoader = ResourceLoader.GetForCurrentView();
        #endregion

        public ListGridVM()
        {
            TimeRanges[0] = ResourceLoader.GetString("day");
            TimeRanges[1] = ResourceLoader.GetString("month");
            TimeRanges[2] = ResourceLoader.GetString("year");
            PlaceholderText = ResourceLoader.GetString("search");

            ViewType = typeof(ListGrid);
            DetailItemVMType = typeof(DetailItemVM<T>);

            // if not customized in later VMs, default open new tab with these parameters
            CreateItemVMType = typeof(DetailItemVM<T>);
            string createLabel = string.IsNullOrWhiteSpace(ResourceLoader.GetString("create")) ? "New" : ResourceLoader.GetString("create");
            string modelName = string.IsNullOrWhiteSpace(ResourceLoader.GetString(typeof(T).Name)) ? typeof(T).Name : ResourceLoader.GetString(typeof(T).Name);
            CreateItemTabName = $"{createLabel} {modelName}...";

            SearchFiltersVM = new SearchFiltersVM<T>();
            SearchFiltersVM.SearchTags.CollectionChanged += SearchTags_CollectionChanged;

            ViewBuilder = new ListGridVB<T>();
            (ViewBuilder as ListGridVB<T>).RowEditEnding += RowEditEnding;

            BindCommands();

            DependencyInitialized();

            if (!UNICompositionRoot.ContainsBaseViewModel(GetType()))
                _ = LoadData();
        }

        /// <summary>
        /// When a filter expression is created in the SearchFiltersVM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void SearchTags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                _ = LoadData();
        }

        protected async override Task LoadData(List<FilterExpression> filterExpressions = null, object parameter = null)
        {
            if (parameter == null || parameter.ToString() == "universalSearch")
            {
                FilterDateFormat = SearchFiltersVM.FilterDateFormat;

                if (filterExpressions == null)
                    filterExpressions = new List<FilterExpression>();

                filterExpressions.AddRange(SearchFiltersVM.FilterExpressions);
            }
            await base.LoadData(filterExpressions);
        }

        #region Commands
        public ICommand SaveGridItems { get; set; }
        void BindCommands()
        {
            EditItem = new RelayCommand((parameter) =>
            {
                if (SelectedItem != null)
                {
                    TabViewItem newDetailItemTab = UNICompositionRoot.TabViewVM.OpenNewTab(
                        name: $"{ResourceLoader.GetString("detail_item_title")} {ResourceLoader.GetString(SelectedItem.GetType().Name)} {FindIdentifierValue(SelectedItem)}",
                        viewModelType: DetailItemVMType,
                        args: new object[] { SelectedItem });

                    var detailItemTab = newDetailItemTab.Content as DetailItem.DetailItem;
                    (detailItemTab.DataContext as DetailItemVM<T>).DetailItemChanged += ListGridVM_DetailItemChanged;
                }
            });

            SaveGridItems = new RelayCommand(async (parameter) =>
            {
                foreach (T item in ItemsToCommit)
                    await BaseClient.UpdateItem(item);

                ItemsToCommit.Clear();
            });

            ExportItems = new RelayCommand((parameter) =>
            {
                if (Content != null)
                    MainPageVM.Print(this, (ViewBuilder as ListGridVB<T>).RenderDataGridForPrinting(ItemsSource));
            });
        }

        private async void ListGridVM_DetailItemChanged(object sender, T itemThatWasEdited)
        {
            await LoadData();
            SelectedItem = itemThatWasEdited;
        }
        #endregion

        protected override void DrawDetailsPane(string navigationViewItemName = null)
        {
            IsLoading = true;

            Content = (DataGrid)ViewBuilder.RenderMainFrameworkElement();

            IsLoading = false;
        }
        protected override void SelectedItemChanged()
        {
            // do not redraw the whole grid when selection changes!
        }

        /// <summary>
        /// event for row edit ending in the main grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (!ItemsToCommit.Contains(SelectedItem))
                ItemsToCommit.Add(SelectedItem);
        }

        /// <summary>
        /// Customize the initial parameters of the search filter component, call this from extended vm if needed
        /// </summary>
        /// <param name="searchFilterSetup"></param>
        public void CustomizeSearchFilters(SearchFilterSetup searchFilterSetup)
        {
            SearchFiltersVM.CustomizeSearchFilter(searchFilterSetup);
        }
    }
}
