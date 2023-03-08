namespace UNI.Core.Explorer.ViewModels
{
    public class ListGridServiceVM<T> //: ListGridVM<ServiceColmar>
    {
        //public override Task LoadData(List<FilterExpression> filterExpressions = null)
        //{
        //    filterExpressions = new List<FilterExpression>();
        //    filterExpressions.Add(new FilterExpression() { PropertyName = "IsService", PropertyValue = "true" });
        //    return base.LoadData(filterExpressions);
        //}

        //override create item to set a service new item logic
        //public override void CreateItemCommand(object parameter)
        //{

        //    NewItemServiceVM<ServiceColmar> newItemVM = new NewItemServiceVM<ServiceColmar>();
        //    newItemVM.ItemUpdated += NewItemVM_ItemUpdated; ;
        //    var newItem = new NewItem() { DataContext = newItemVM };
        //    ContentDialogManager.ShowContentDialog(newItem);
        //}
    }
}
