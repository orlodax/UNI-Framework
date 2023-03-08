using Microsoft.UI.Xaml.Controls;
using System;
using System.Windows.Input;
using UNI.Core.Library;
using UNI.Core.UI.ContentDialogs;
using UNI.Core.UI.CustomEventArgs;
using UNI.Core.UI.Misc;
using UNI.Core.UI.Tabs;
using Windows.ApplicationModel.Resources;

namespace UNI.Core.UI.NewItem
{
    public class NewItemVM<T> : BaseTabVM<T> where T : BaseModel
    {
        public string SaveButtonLabel { get; set; }
        public string CancelButtonLabel { get; set; }
        public string Title { get; set; }
        public bool OnlyInit { get; set; }
        public BaseModel ParentItem { get; set; }

        public ICommand Save { get; private set; }
        public ICommand Cancel { get; private set; }

        #region CTOR
        public NewItemVM(BaseModel parentItem = null)
        {
            Init();
            InitParentItem(parentItem);
        }

        void Init()
        {
            ViewBuilder = new NewOrEditItemVB<T>();
            OnlyInit = true;

            Title = ResourceLoader.GetForCurrentView().GetString("newItem_Title");
            SaveButtonLabel = ResourceLoader.GetForCurrentView().GetString("saveButtonLabel");
            CancelButtonLabel = ResourceLoader.GetForCurrentView().GetString("cancelButtonLabel");

            Save = new RelayCommand(SaveCommand);
            Cancel = new RelayCommand(CancelCommand);

            DependencyInitialized();
        }

        async void InitParentItem(BaseModel parentItem)
        {
            SelectedItem = ModelFactory.CreateModel<T>(parentItem);

            IsLoading = true;
            var response = await BaseClient.CreateItem(SelectedItem);
            if (response != null)
            {
                SelectedItem = response;
            }
            DrawDetailsPane();
            IsLoading = false;
        }
        #endregion

        public override async void ViewBuilder_ItemUpdated(object sender, ItemUpdatedEventArgs e)
        {
            await BaseClient.UpdateItem(SelectedItem); // save current changes
            SelectedItem.Updated();
            DrawDetailsPane(navigationViewItem);
        }

        /// <summary>
        /// Method responsible for building the view
        /// </summary>
        protected override void DrawDetailsPane(string navigationViewItemName = null)
        {
            Content = ViewBuilder.RenderMainFrameworkElement(SelectedItem ?? (Activator.CreateInstance(typeof(T)) as BaseModel), navigationViewItemName);
        }

        protected override void SelectedItemChanged()
        {
            // do not redraw the whole grid when selection changes!
        }

        /// <summary>
        /// Method that update the item before initialized
        /// </summary>
        /// <param name="parameter"></param>
        public virtual async void SaveCommand(object parameter)
        {
            if (ValidationSuccesful())
            {
                OnlyInit = false;

                int statusCode = await BaseClient.UpdateItem(SelectedItem);

                //TODO manage codes
                if (statusCode >= 400)
                    _ = new TeachingTip()
                    {
                        Title = ResourceLoader.GetForCurrentView().GetString("error"),
                        Subtitle = $"error n. {statusCode}",
                        IsLightDismissEnabled = true,
                        IsOpen = true
                    };
                else
                    OnItemUpdated(this, new ItemUpdatedEventArgs(SelectedItem));
            }
        }

        /// <summary>
        /// Method to cancel the create operation. It deletes the initialized item
        /// </summary>
        /// <param name="parameter"></param>
        async void CancelCommand(object parameter)
        {
            if (SelectedItem != null)
            {
                await BaseClient.DeleteItem(SelectedItem);
                OnItemUpdated(this, new ItemUpdatedEventArgs(SelectedItem));
            }
        }

        /// <summary>
        /// Method that delete the initialized item if it is not requested the saving
        /// </summary>
        internal override async void CompleteClosing()
        {
            if (OnlyInit && SelectedItem != null)
            {
                await BaseClient.DeleteItem(SelectedItem);
                OnItemUpdated(this, new ItemUpdatedEventArgs(SelectedItem));
            }
        }
    }
}
