using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using UNI.Core.Library;
using UNI.Core.UI.Misc;
using UNI.Core.UI.Tabs;
using UNI.Core.UI.Tabs.ListGrid;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace UNI.Core.UI.CustomControls.GridBox
{
    public class GridBoxViewVM<T> : BaseTabVMTypeAgnostic where T : BaseModel
    {

        #region Properties/Bindings

        internal ObservableCollection<T> itemsSource = new ObservableCollection<T>();
        public ObservableCollection<T> ItemsSource { get => itemsSource; set => SetValue(ref itemsSource, value); }


        private string searchText = string.Empty;
        public string SearchText { get => searchText; set => SetValue(ref searchText, value); }


        private string gridBoxName = string.Empty;
        public string GridBoxName { get => gridBoxName; set => SetValue(ref gridBoxName, value); }


        private DataGrid mainGrid;
        public DataGrid MainGrid { get => mainGrid; set => SetValue(ref mainGrid, value); }


        public string PlaceholderText { get; set; }

        #endregion

        #region XAML Visibilities
        /// <summary>
        /// Show the Showbox in the flyout when edit is pressed in the mtm GridBox
        /// </summary>
        private bool showBoxVisibility = false;
        public bool ShowBoxVisibility { get => showBoxVisibility; set => SetValue(ref showBoxVisibility, value); }

        internal Visibility createItemVisibility = Visibility.Collapsed;
        public Visibility CreateItemVisibility { get => createItemVisibility; set => SetValue(ref createItemVisibility, value); }

        internal Visibility saveItemVisibility = Visibility.Collapsed;
        public Visibility SaveItemVisibility { get => saveItemVisibility; set => SetValue(ref saveItemVisibility, value); }

        private Visibility editItemVisibility = Visibility.Collapsed;
        public Visibility EditItemVisibility { get => editItemVisibility; set => SetValue(ref editItemVisibility, value); }

        private Visibility deleteItemVisibility = Visibility.Collapsed;
        public Visibility DeleteItemVisibility { get => deleteItemVisibility; set => SetValue(ref deleteItemVisibility, value); }

        private Visibility navButtonsVisibility = Visibility.Collapsed;
        public Visibility NavButtonsVisibility { get => navButtonsVisibility; set => SetValue(ref navButtonsVisibility, value); }
        #endregion
        public ICommand Search { get; set; }


        protected ListGridVB<T> ViewBuilder = new ListGridVB<T>();
        protected List<T> notFilteredItemsSource;


        public GridBoxViewVM(List<T> itemsSource, string name)
        {
            GridBoxName = name;

            PlaceholderText = ResourceLoader.GetForCurrentView().GetString("search");

            MainGrid = (DataGrid)ViewBuilder.RenderMainFrameworkElement(height: 300);

            Search = new RelayCommand(SearchCommand);

            notFilteredItemsSource = new List<T>(itemsSource ?? new List<T>());
            PopulateItemsSource();
        }


        protected virtual void SearchCommand(object parameter)
        {
            if (parameter == null || (parameter is KeyRoutedEventArgs args && args.Key == Windows.System.VirtualKey.Enter))
                PopulateItemsSource();
        }

        /// <summary>
        /// Main method to retrieve data
        /// </summary>
        /// <param name="filterExpressions"></param>
        protected virtual void PopulateItemsSource(List<FilterExpression> filterExpressions = null)
        {
            ItemsSource = new ObservableCollection<T>(notFilteredItemsSource);

            FilterListCommand();
        }


        /// <summary>
        /// Method to filter the itemsource list based on the content of the searchtext property
        /// </summary>
        protected virtual void FilterListCommand()
        {
            var filteredItemsSource = new List<T>();

            if (!string.IsNullOrWhiteSpace(SearchText) && typeof(T).GetProperties().Any())
            {
                foreach (T item in notFilteredItemsSource.Where(i => i != null))
                {
                    foreach (PropertyInfo property in typeof(T).GetProperties())
                    {
                        if (!property.PropertyType.IsSubclassOf(typeof(BaseModel)) && !property.PropertyType.IsGenericType)
                        {
                            if (Convert.ToString(property?.GetValue(item)).Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                            {
                                filteredItemsSource.Add(item);
                                break;
                            }

                        }
                        else if (property.PropertyType.IsSubclassOf(typeof(BaseModel)))
                        {
                            if (SearchValueInObject(SearchText, property.GetValue(item)))
                            {
                                filteredItemsSource.Add(item);
                                break;
                            }
                        }
                    }
                }

                ItemsSource = new ObservableCollection<T>(filteredItemsSource);
            }
            else
                ItemsSource = new ObservableCollection<T>(notFilteredItemsSource);
        }

        private bool SearchValueInObject(string searchText, object item)
        {
            foreach (var property in item.GetType().GetProperties())
                if (property?.GetValue(item) != null)
                    if (Convert.ToString(property.GetValue(item)).Contains(searchText, StringComparison.OrdinalIgnoreCase))
                        return true;

            return false;
        }
    }
}
