using Gioiaspa.Warehouse.Library;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UNI.Core.Explorer.ViewModels.DetailItem;
using UNI.Core.Library;
using UNI.Core.UI.Components.SearchFilters;
using UNI.Core.UI.Tabs.ListGrid;
using Windows.UI.Xaml;

namespace UNI.Core.Explorer.ViewModels.ListGrid
{
    internal class ListGridVMBordero : ListGridVM<Bordero>
    {
        public ListGridVMBordero()
        {
            SelectedItemsQuantity = 20;
            DatePickerVisibility = Visibility.Visible;
            SetCustomProperties();
            _ = LoadData();
            CreateOrEditItemShouldOpenInNewTab(typeof(DetailItemVMBordero), "Edita");
        }
        protected override async Task LoadData(List<FilterExpression> filterExpressions = null, object parameter = null)
        {
            await base.LoadData();
            var orderedItemsSource = ItemsSource.OrderByDescending(i => i.Date);
            ItemsSource = new ObservableCollection<Bordero>(orderedItemsSource);
        }

        public void SetCustomProperties()
        {
            var visibleProps = new List<string>
            {
               nameof(Bordero.Date),
               nameof(Bordero.Name),
               nameof(Bordero.Carrier),
               nameof(Bordero.State),
               nameof(Bordero.Area),
            };

            ViewBuilder.CustomizeVisibleProperties(visibleProps);
            //NewItemType = typeof(NewItemVMBordero);
            DetailItemVMType = typeof(DetailItemVMBordero);

            CustomizeSearchFilters(new SearchFilterSetup
            {
                InitialDate = DateTime.Today,
                InitialProperty = nameof(Bordero.Date),
                TimeRange = (int)EnTimeRanges.Day,
                PropertiesToShow = new List<string>
                {
                    nameof(Bordero.Date),
                    nameof(Bordero.Carrier),
                }
            });
        }
    }
}
