using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using UNI.Core.Library.GenericModels;
using UNI.Core.UI.Misc;
using UNI.Core.UI.NewItem;
using Windows.UI.Xaml.Input;

namespace UNI.Core.Explorer.ViewModels
{
    class NewItemMultipleProductSerialVM<T> : NewItemVM<ProductSerial>
    {
        private string serialCode = string.Empty;
        private string allSerials = string.Empty;
        private List<ProductSerial> productSerials = new List<ProductSerial>();

        public ICommand SerialCodeAdded { get; set; }
        public string SerialCode { get => serialCode; set { serialCode = value; NotifyPropertyChanged(); } }
        public string AllSerials { get => allSerials; set { allSerials = value; NotifyPropertyChanged(); } }

        public List<ProductSerial> ProductSerials { get => productSerials; set => productSerials = value; }

        public event EventHandler SerialsAdded;

        public NewItemMultipleProductSerialVM()
        {
            SerialCodeAdded = new RelayCommand(SerialCodeAddedCommand);
        }

        private void SerialCodeAddedCommand(object parameter)
        {
            if (parameter is KeyRoutedEventArgs keyRoutedEventArgs)
            {
                if (keyRoutedEventArgs.Key == Windows.System.VirtualKey.Enter)
                {
                    AllSerials += SerialCode + ';';
                    SerialCode = string.Empty;
                }
            }
        }

        public override async void SaveCommand(object parameter)
        {
            List<ProductSerial> Serials = new List<ProductSerial>();
            if (!string.IsNullOrWhiteSpace(AllSerials))
            {
                List<string> serialsString = AllSerials.Split(';').ToList();
                foreach (var serial in serialsString ?? new List<string>())
                {
                    if (!string.IsNullOrWhiteSpace(serial))
                    {
                        var productSerial = ModelFactory.CreateModel<T>(ParentItem) as ProductSerial;
                        productSerial.SerialCode = serial;
                        productSerials.Add(await BaseClient.CreateItem(productSerial));
                    }

                }
            }
            SerialsAdded?.Invoke(this, new EventArgs());
        }
    }
}
