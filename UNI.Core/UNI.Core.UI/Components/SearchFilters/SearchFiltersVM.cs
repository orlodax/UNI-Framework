using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using UNI.Core.Library;
using UNI.Core.UI.Misc;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;

namespace UNI.Core.UI.Components.SearchFilters
{
    public class SearchFiltersVM<T> : Observable
    {
        #region PUBLIC SURFACE
        /// <summary>
        /// Filter to send to the API to specify time range
        /// </summary>
        public string FilterDateFormat { get; set; }

        /// <summary>
        /// Values to build the query in the API/DAL
        /// </summary>
        public List<FilterExpression> FilterExpressions { get; set; } = new List<FilterExpression>();

        /// <summary>
        /// Call from extended VM to customize initialization of this component
        /// </summary>
        /// <param name="searchFilterSetup"></param>
        internal void CustomizeSearchFilter(SearchFilterSetup searchFilterSetup)
        {
            if (searchFilterSetup.PropertiesToShow != null && searchFilterSetup.PropertiesToShow.Any())
                Properties = Properties.Where(p => searchFilterSetup.PropertiesToShow.Any(n => n == p.Name)).ToList();

            if (searchFilterSetup.TimeRange.HasValue)
                SelectedTimeRangeIndex = searchFilterSetup.TimeRange.Value;

            if (searchFilterSetup.InitialDate.HasValue)
                DateInputToSearch = searchFilterSetup.InitialDate.Value;

            if (searchFilterSetup.InitialProperty != null)
            {
                SelectedProperty = Properties.FirstOrDefault(p => p.Name == searchFilterSetup.InitialProperty);
                isDateTypeSelected = SetUIAccordingToType();
                DateFormat = SetTimeRangeInfo(searchFilterSetup.TimeRange ?? (int)EnTimeRanges.Day);

                AddFilterCommand(null);
            }
        }
        #endregion

        #region BINDINGS
        /// <summary>
        /// Headers of controls
        /// </summary>
        public string SearchByLabel { get; set; }
        public string FilterByDateLabel { get; set; }
        public string DateOptionsLabel { get; set; }
        public string FilterByValueLabel { get; set; }
        public string SearchBoxPlaceHolderText { get; set; }


        /// <summary>
        /// Format of the date for SQL // TODO move this logic to API // make enum in lib
        /// </summary>
        public string DateFormat { get; set; }

        /// <summary>
        /// The properties of the type T in which to search. Not full list of properties, see GetPropertyInfos()
        /// </summary>
        private List<PropertyInfo> properties;
        public List<PropertyInfo> Properties { get => properties; set => SetValue(ref properties, value); }

        /// <summary>
        /// Selection of the property to filter
        /// </summary>
        private PropertyInfo selectedProperty;
        public PropertyInfo SelectedProperty { get => selectedProperty; set => SetValue(ref selectedProperty, value); }

        /// <summary>
        /// Selected time range in radio buttons matching the EnTimeRange
        /// </summary>
        private int selectedTimeRangeIndex;
        public int SelectedTimeRangeIndex { get => selectedTimeRangeIndex; set => SetValue(ref selectedTimeRangeIndex, value); }

        /// <summary>
        /// The search expression coming from the TextBox
        /// </summary>
        private string stringInputToSearch;
        public string StringInputToSearch { get => stringInputToSearch; set => SetValue(ref stringInputToSearch, value); }

        /// <summary>
        /// The search expression coming from the DatePicker
        /// </summary>
        private DateTimeOffset dateInputToSearch = DateTime.Today;
        public DateTimeOffset DateInputToSearch { get => dateInputToSearch; set => SetValue(ref dateInputToSearch, value); }

        /// <summary>
        /// Enable DatePicker and date options for date typed properties
        /// </summary>
        private Visibility datePickerVisibility = Visibility.Collapsed;
        public Visibility DatePickerVisibility { get => datePickerVisibility; set => SetValue(ref datePickerVisibility, value); }

        /// <summary>
        /// Enable TextBox for string/numeric typed properties
        /// </summary>
        private Visibility textBoxVisibility = Visibility.Collapsed;
        public Visibility TextBoxVisibility { get => textBoxVisibility; set => SetValue(ref textBoxVisibility, value); }

        /// <summary>
        /// Enable button
        /// </summary>
        private Visibility buttonVisibility = Visibility.Collapsed;
        public Visibility ButtonVisibility { get => buttonVisibility; set => SetValue(ref buttonVisibility, value); }


        /// <summary>
        /// Radio buttons labels item source
        /// </summary>
        private List<string> timeRanges = new List<string>();
        public List<string> TimeRanges { get => timeRanges; set => SetValue(ref timeRanges, value); }

        /// <summary>
        /// TODO quando abbiamo filtri multipli LIKE and OR - The active filters. Register to this collection's events to handle search function in hosting VM
        /// </summary>
        private ObservableCollection<SearchTag> searchTags = new ObservableCollection<SearchTag>();

        public ObservableCollection<SearchTag> SearchTags { get => searchTags; set => SetValue(ref searchTags, value); }
        #endregion

        #region Private fields
        private readonly List<Type> textBasedTypes = new List<Type>
        {
            typeof(string),
            typeof(int),
            typeof(double),
            typeof(float),
            typeof(decimal)
        };
        private readonly List<Type> dateBasedTypes = new List<Type>
        {
            typeof(DateTime),
            typeof(DateTimeOffset)
        };
        private bool isDateTypeSelected;
        private readonly ResourceLoader resourceLoader;
        #endregion

        #region CTOR
        public SearchFiltersVM()
        {
            resourceLoader = ResourceLoader.GetForCurrentView();
            var timeRangesCount = Enum.GetNames(typeof(EnTimeRanges)).Length;
            for (int i = 0; i < timeRangesCount; i++)
                TimeRanges.Add(resourceLoader.GetString(Enum.GetName(typeof(EnTimeRanges), i).ToLower()));

            FilterByValueLabel = resourceLoader.GetString("searchFilters_FilterByValue");
            DateOptionsLabel = resourceLoader.GetString("searchFilters_TimeRange");
            FilterByDateLabel = resourceLoader.GetString("searchFilters_FilterByDate");
            SearchByLabel = resourceLoader.GetString("searchFilters_SearchBy");
            SearchBoxPlaceHolderText = resourceLoader.GetString("search");

            BindCommands();
            Properties = GetPropertyInfos();
        }
        private List<PropertyInfo> GetPropertyInfos()
        {
            return typeof(T).GetProperties().Where(property => textBasedTypes.Any(type => type == property.PropertyType) || dateBasedTypes.Any(type => type == property.PropertyType)).ToList();
        }
        #endregion

        #region Commands
        /// <summary>
        /// Handle view changes according to type of property selected
        /// </summary>
        public ICommand PropertySelectionChanged { get; private set; }

        /// <summary>
        /// Add a filter expression (tag) to the list
        /// </summary>
        public ICommand AddFilter { get; private set; }

        /// <summary>
        /// Add a filter expression (tag) to the list
        /// </summary>
        public ICommand TimeRangeSelectionChanged { get; private set; }

        private void BindCommands()
        {
            PropertySelectionChanged = new RelayCommand((parameter) =>
            {
                isDateTypeSelected = SetUIAccordingToType();
            });

            AddFilter = new RelayCommand(AddFilterCommand);

            TimeRangeSelectionChanged = new RelayCommand((parameter) =>
            {
                DateFormat = SetTimeRangeInfo(SelectedTimeRangeIndex);
            });
        }

        private void AddFilterCommand(object parameter)
        {
            string searchExpression;
            if (isDateTypeSelected)
                searchExpression = DateFormat = SetTimeRangeInfo(SelectedTimeRangeIndex);
            else
                searchExpression = StringInputToSearch;

            if(FilterExpressions.Count > 0) FilterExpressions.Clear();
            if(SearchTags.Count > 0) SearchTags.Clear();

            if (!String.IsNullOrWhiteSpace(searchExpression) && selectedProperty != null)
            {
                var filterExpression = new FilterExpression() { PropertyName = SelectedProperty.Name, PropertyValue = searchExpression };

                // TODO un domani ci possono essere più filtri contemporaneamente
                //var existingExpression = FilterExpressions.FirstOrDefault(f => f.PropertyName == SelectedProperty.Name);
                //if (existingExpression != null)
                //    RemoveTagBox(existingExpression);

                AddTagBox(filterExpression);
            }
            //FilterExpressions.Clear();
            //SearchTags.Clear();
        }

        private bool SetUIAccordingToType()
        {
            bool isDateTypeSelected = dateBasedTypes.Contains(SelectedProperty.PropertyType);
            ButtonVisibility = Visibility.Visible;
            if (isDateTypeSelected)
            {
                TextBoxVisibility = Visibility.Collapsed;
                DatePickerVisibility = Visibility.Visible;
            }
            else
            {
                TextBoxVisibility = Visibility.Visible;
                DatePickerVisibility = Visibility.Collapsed;
            }
            return isDateTypeSelected;
        }

        private string SetTimeRangeInfo(int selectedTimeRangeIndex)
        {
            switch (selectedTimeRangeIndex)
            {
                default:
                case (int)EnTimeRanges.Day:
                    FilterDateFormat = "%Y-%m-%d";
                    return DateInputToSearch.DateTime.ToString("yyyy-MM-dd");

                case (int)EnTimeRanges.Month:
                    FilterDateFormat = "%Y-%m";
                    return DateInputToSearch.DateTime.ToString("yyyy-MM");

                case (int)EnTimeRanges.Year:
                    FilterDateFormat = "%Y";
                    return DateInputToSearch.DateTime.ToString("yyyy");
            }
        }

        public void AddTagBox(FilterExpression filterExpression)
        {
            FilterExpressions.Add(filterExpression);
            var searchTag = new SearchTag(filterExpression);
            SearchTags.Add(searchTag);

            searchTag.Close += SearchTag_Close;
        }

        private void RemoveTagBox(FilterExpression filterExpression)
        {
            FilterExpressions.Remove(filterExpression);
            SearchTags.Remove(SearchTags.First(s => s.FilterExpression == filterExpression));
        }

        private void SearchTag_Close(object sender, RoutedEventArgs e)
        {
            var searchTag = e.OriginalSource as SearchTag;
            FilterExpressions.Remove(searchTag.FilterExpression);
            SearchTags.Remove(searchTag);
        }
        #endregion
    }
}
