using Microsoft.UI.Xaml.Controls;
using System;
using System.Windows.Input;
using UNI.Core.Library;
using UNI.Core.UI.ContentDialogs;
using UNI.Core.UI.Misc;
using UNI.Core.UI.Tabs;
using Windows.ApplicationModel.Resources;

namespace UNI.Core.UI.NewItem
{
    public class EditItemVM<T> : BaseTabVM<T> where T : BaseModel
    {
        public string SaveButtonLabel { get; set; }
        public string CancelButtonLabel { get; set; }
        public string Title { get; set; }

        public ICommand Save { get; private set; }

        public EditItemVM(T item)
        {
            ViewBuilder = new NewOrEditItemVB<T>();
            SelectedItem = item;

            Title = ResourceLoader.GetForCurrentView().GetString("newItem_Title");
            SaveButtonLabel = ResourceLoader.GetForCurrentView().GetString("saveButtonLabel");
            CancelButtonLabel = ResourceLoader.GetForCurrentView().GetString("cancelButtonLabel");
            Content = ViewBuilder.RenderMainFrameworkElement(item);
            Save = new RelayCommand(SaveCommand);

            DependencyInitialized();

            _ = LoadSingle(SelectedItem.ID);
        }

        /// <summary>
        /// Method responsible for building the view
        /// </summary>
        protected override void DrawDetailsPane(string navigationViewItemName = null)
        {
            Content = ViewBuilder.RenderMainFrameworkElement(SelectedItem ?? (Activator.CreateInstance(typeof(T)) as BaseModel), pageGroup: navigationViewItemName);
        }

        /// <summary>
        /// Method that update the item before initialized
        /// </summary>
        /// <param name="parameter"></param>
        async void SaveCommand(object parameter)
        {
            if (ValidationSuccesful())
            {
                int statusCode = await BaseClient.UpdateItem(SelectedItem);
                //TODO manage codes
                if (statusCode >= 400 && statusCode != 0)
                {
                    _ = new TeachingTip()
                    {
                        Title = ResourceLoader.GetForCurrentView().GetString("error"),
                        Subtitle = $"error n. {statusCode}",
                        IsOpen = true 
                    };
                }
                else
                    OnItemUpdated(this, new CustomEventArgs.ItemUpdatedEventArgs(SelectedItem));
            }
        }
    }
}
