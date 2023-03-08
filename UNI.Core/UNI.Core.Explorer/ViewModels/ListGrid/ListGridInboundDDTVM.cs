using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;
using UNI.Core.UI.Tabs.ListGrid;

namespace UNI.Core.Explorer.ViewModels.ListGrid
{
    public class ListGridInboundDDTVM<T> : ListGridVM<InboundDDT>
    {
        public ListGridInboundDDTVM()
        {
            ExportButtonVisibility = Windows.UI.Xaml.Visibility.Visible;
        }

        protected override async Task LoadData(List<FilterExpression> filterExpressions = null, object parameter = null)
        {
            await LoadData(filterExpressions);
            ItemsSource = new System.Collections.ObjectModel.ObservableCollection<InboundDDT>(ItemsSource.OrderByDescending(i => i.Date));
        }
    }
}
