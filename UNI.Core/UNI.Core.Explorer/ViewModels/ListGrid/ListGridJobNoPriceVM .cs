namespace UNI.Core.Explorer.ViewModels.ListGrid
{
    public class ListGridJobWithPriceVM<T> /*: ListGridVM<JobWithPrice>*/
    {
        public ListGridJobWithPriceVM()
        {
            //DatePickerVisibility = Windows.UI.Xaml.Visibility.Visible;
        }

        //public override async Task LoadData(List<FilterExpression> filterExpressions = null)
        //{
        //    FilterDateFormat = "%Y-%m-%d";
        //    string dateValue = FilterSelectedDate.DateTime.ToString("yyyy-MM-dd");

        //    switch (SelectedDateSelectionType)
        //    {
        //        case "Giorno":
        //            FilterDateFormat = "%Y-%m-%d";
        //            dateValue = FilterSelectedDate.DateTime.ToString("yyyy-MM-dd");
        //            break;

        //        case "Mese":
        //            FilterDateFormat = "%Y-%m";
        //            dateValue = FilterSelectedDate.DateTime.ToString("yyyy-MM");
        //            break;

        //        case "Anno":
        //            FilterDateFormat = "%Y";
        //            dateValue = FilterSelectedDate.DateTime.ToString("yyyy");
        //            break;
        //    }


        //    filterExpressions = new List<FilterExpression>();
        //    filterExpressions.Add(new FilterExpression() { PropertyName = "DatePlanned", PropertyValue = dateValue });

        //    await base.LoadData(filterExpressions);
        //    ItemsSource = new System.Collections.ObjectModel.ObservableCollection<JobWithPrice>(ItemsSource.OrderByDescending(i => i.DatePlanned));
        //}
    }
}
