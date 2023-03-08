using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using UNI.Core.Library;
using UNI.Core.UI.CustomEventArgs;
using UNI.Core.UI.Misc;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;

namespace UNI.Core.UI.Tabs.DetailItem
{
    public class DetailItemVM<T> : BaseTabVM<T> where T : BaseModel
    {
        /// <summary>
        /// Parent tab (i.e. ListGrid) uses this to detect changes to the selected item edited here
        /// </summary>
        public event EventHandler<T> DetailItemChanged;

        /// <summary>
        /// Update the object then reload data
        /// </summary>
        public ICommand UpdateItem { get; set; }

        public DetailItemVM(T selectedItem)
        {
            ViewType = typeof(DetailItem);
            ViewBuilder = new DetailItemVB<T>();

            SelectedItem = selectedItem;

            ExportButtonVisibility = Visibility.Visible;

            BindCommands();

            DependencyInitialized();

            _ = LoadSingle(SelectedItem.ID);
        }

        void BindCommands()
        {
            UpdateItem = new RelayCommand(async (parameter) =>
            {
                if (ValidationSuccesful())
                {
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
                        DetailItemChanged?.Invoke(this, SelectedItem);
                    }
                }
            });
        }

        protected override void DrawDetailsPane(string navigationViewItemName = null)
        {
            IsLoading = true;

            if (SelectedItem != null)
                Content = ViewBuilder.RenderMainFrameworkElement(SelectedItem ?? (BaseModel)Activator.CreateInstance(typeof(T)), pageGroup: navigationViewItemName);

            IsLoading = false;
        }

        protected override Task LoadData(List<FilterExpression> filterExpressions = null, object parameter = null)
        {
            return LoadSingle(SelectedItem.ID);
        }
    }
}
