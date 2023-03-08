using Microsoft.UI.Xaml.Controls;
using System.Windows.Input;
using UNI.Core.Library;
using UNI.Core.UI.CustomEventArgs;
using UNI.Core.UI.Misc;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;

namespace UNI.Core.UI.Tabs.ListDetail
{
    public class ListDetailVM<T> : BaseTabVM<T> where T : BaseModel
    {
        private FrameworkElement detailsPane; // detail element of the view

        public string SearchBoxPlaceHolderText { get; set; }    
        public FrameworkElement DetailsPane { get => detailsPane; set => SetValue(ref detailsPane, value); }
        public DataTemplate ItemTemplate { get; set; }

        private int[] itemsQuantities = { 20, 50, 100, 500, 1000 }; //list of all possible selection items
        public int[] ItemsQuantities { get => itemsQuantities; set => SetValue(ref itemsQuantities, value); }

        /// <summary>
        /// Update the object then reload data
        /// </summary>
        public ICommand UpdateItem { get; set; }

        public ListDetailVM()
        {
            ViewType = typeof(ListDetail);
            ViewBuilder = new ListDetailVB<T>();
            SearchBoxPlaceHolderText  = ResourcesHelper.GetString("listDetail_Search", "Search...");
            ItemTemplate = (ViewBuilder as ListDetailVB<T>).SelectItemListTemplate();   // select template from dictionary according to Model type

            DependencyInitialized();
            BindCommands();

            _ = LoadData();
        }

        void BindCommands()
        {
            UpdateItem = new RelayCommand(async (parameter) =>
            {
                if (!ValidationSuccesful())
                    return;

                int statusCode = await BaseClient.UpdateItem(SelectedItem);

                //TODO manage codes
                if (statusCode >= 300 || statusCode < 200)
                {
                    _ = new TeachingTip()
                    {
                        Title = ResourceLoader.GetForCurrentView().GetString("error"),
                        Subtitle = $"error n. {statusCode}",
                        IsLightDismissEnabled = true,
                        IsOpen = true
                    };
                }
                else
                {
                    OnItemUpdated(this, new ItemUpdatedEventArgs(SelectedItem));
                }
            });
        }

        protected override void DrawDetailsPane(string navigationViewItemName = null)
        {
            IsLoading = true;

            if (SelectedItem != null)
            {
                DetailsPane = ViewBuilder.RenderMainFrameworkElement(SelectedItem, navigationViewItemName);
                SelectedItem.Loaded();
            }

            IsLoading = false;
        }
    }
}
