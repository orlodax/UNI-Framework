using System;
using System.Reflection;
using System.Windows.Input;
using UNI.Core.Client;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;
using UNI.Core.UI.Misc;
using UNI.Core.UI.Tabs;
using Windows.Storage;

namespace UNI.Core.UI.CustomControls.ImageBox
{
    public class ImageBoxVM : BaseTabVMTypeAgnostic
    {
        private readonly UniImage uniImage;
        private readonly PropertyInfo property;
        private readonly BaseModel parent;
        private readonly string name;
        private readonly UniClient<UniImage> baseClient;
        public ImageBoxVM(UniImage uniImage, PropertyInfo property, BaseModel parent, string name)
        {
            baseClient = new UniClient<UniImage>();

            this.uniImage = uniImage;
            this.property = property;
            this.parent = parent;
            this.name = name;
            AddImage = new RelayCommand(AddImageCommand);
            DeleteImage = new RelayCommand(DeleteImageCommand);
        }

        public UniImage UniImage => uniImage;
        public ICommand AddImage { get; set; }
        public ICommand DeleteImage { get; set; }
        public string Name => name;

        private async void AddImageCommand(object parameter)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
            };
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // Application now has read/ write access to the picked file
                _ = await ImageUtilities.PreviewImage(file);

                UniImage.Source = await ImageUtilities.FileToByteArray(file);
                if (uniImage.ID == 0)
                {
                    UniImage.ModelName = parent.GetType().Name;
                    UniImage.IdBaseModel = parent.ID;
                    UniImage.PropertyName = property.Name;
                    await baseClient.CreateItem(UniImage);
                }
                else
                    await baseClient.UpdateItem(UniImage);

            }
        }

        private async void DeleteImageCommand(object parameter)
        {
            await baseClient.DeleteItem(UniImage);
        }
    }
}
