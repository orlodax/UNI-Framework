using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using UNI.API.Contracts.RequestsDTO;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;
using UNI.Core.UI.MainPage;
using UNI.Core.UI.Misc;
using UNI.Core.UI.NewItem;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace UNI.Core.Explorer.ViewModels
{
    class NewItemInboundProductSerialVM<T> : NewItemVM<T> where T : BaseModel
    {
        private string serialCode = string.Empty;
        private string allSerials = string.Empty;

        public ICommand SerialCodeAdded { get; set; }
        public string SerialCode
        {
            get => serialCode;
            set { SerialAdded(serialCode, value); serialCode = value; NotifyPropertyChanged(); }
        }

        public string AllSerials { get => allSerials; set { allSerials = value; NotifyPropertyChanged(); } }

        public NewItemInboundProductSerialVM()
        {
            SerialCodeAdded = new RelayCommand(SerialCodeAddedCommand);
        }

        private void SerialAdded(string oldvalue, string newvalue)
        {
            if (oldvalue == newvalue) { }
            //{
            //    AllSerials += MultipleSerialCodes + ';';
            //    MultipleSerialCodes = string.Empty;
            //}

            //if (newSerialAdded)
            //{
            //    MultipleSerialCodes += ";";
            //    newSerialAdded = false;
            //}
        }

        private void SerialCodeAddedCommand(object parameter)
        {
            if (parameter is KeyRoutedEventArgs keyRoutedEventArgs)
            {
                if (keyRoutedEventArgs.Key == Windows.System.VirtualKey.Enter)
                {

                    AllSerials += SerialCode + ';';
                    SerialCode = string.Empty;
                    //newSerialAdded = true;
                    //if (SelectedItem != null)
                    //{
                    //}
                }
            }
        }

        public override async void SaveCommand(object parameter)
        {
            OnlyInit = false;
            var filterExpressions = new List<FilterExpression>
            {
                new FilterExpression() { PropertyName = "SerialCode", PropertyValue = (SelectedItem as ProductSerial).SerialCode }
            };
            var productSerials = await BaseClient.Get(new GetDataSetRequestDTO() { FilterExpressions = filterExpressions });

            if (productSerials == null || !productSerials.Any())
            {
                base.SaveCommand(parameter);
            }
            else
            {
                var contentDialog = new ContentDialog()
                {
                    Title = "Errore",
                    Content = "Codice seriale già presente in archivio",
                    CloseButtonText = "OK"
                };
                TabViewVM.ShowContentDialog(contentDialog);
            }
        }
    }
}
