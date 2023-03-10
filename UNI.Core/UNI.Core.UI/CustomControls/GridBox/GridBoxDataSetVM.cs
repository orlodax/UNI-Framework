using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using UNI.API.Client;
using UNI.Core.Library;
using UNI.Core.UI.Misc;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace UNI.Core.UI.CustomControls.GridBox
{
    public class GridBoxDataSetVM<T> : GridBoxVM<T> where T : BaseModel
    {
        private readonly UNIDataSet<T> UniDataSet;

        /// <summary>
        /// Data Block displayed index
        /// </summary>
        private int selectedDataBlockNumber = 1;
        public int SelectedDataBlockNumber { get => selectedDataBlockNumber; set => SetValue(ref selectedDataBlockNumber, value); }
        /// <summary>
        /// Total number of data blocks
        /// </summary>
        private int dataBlocksNumber = 1;
        public int DataBlocksNumber { get => dataBlocksNumber; set => SetValue(ref dataBlocksNumber, value); }

        /// <summary>
        /// Command for next items loading
        /// </summary>
        public ICommand NextItems { get; set; }
        /// <summary>
        /// Command for previous items
        /// </summary>
        public ICommand PrevItems { get; set; }


        public GridBoxDataSetVM(BaseModel parentItem,
                                UNIDataSet<T> uniDataSet,
                                string name,
                                PropertyInfo propertyInfo,
                                Type newItemType,
                                Type editItemType) : base(parentItem, new List<T>(), name, propertyInfo, newItemType, editItemType)
        {
            NavButtonsVisibility = Visibility.Visible;

            UniDataSet = uniDataSet;

            NextItems = new RelayCommand(NextItemsCommand);
            PrevItems = new RelayCommand(PrevItemsCommand);

            PopulateItemsSource();
        }

        /// <summary>
        /// Click on next items loading
        /// </summary>
        /// <param name="parameter"></param>
        private void NextItemsCommand(object parameter)
        {
            if (SelectedDataBlockNumber < UniDataSet.DataBlocks)
            {
                SelectedDataBlockNumber++;
                PopulateItemsSource();
            }
        }

        /// <summary>
        /// Click on previous items loading
        /// </summary>
        /// <param name="parameter"></param>
        private void PrevItemsCommand(object parameter)
        {
            if (SelectedDataBlockNumber > 1)
            {
                SelectedDataBlockNumber--;
                PopulateItemsSource();
            }

        }

        /// <summary>
        /// Override Load data using unidataset
        /// </summary>
        protected override async void PopulateItemsSource(List<FilterExpression> filterExpressions = null)
        {
            IsLoading = true;
            if (UniDataSet != null)
            {
                notFilteredItemsSource?.Clear();
                if (ParentItem != null)
                {
                    string idName = ParentItem.GetType().Name;

                    List<Type> extendedTypes = UtilityMethods.FindAllParentsTypes(ParentItem.GetType());
                    extendedTypes.Add(ParentItem.GetType());
                    foreach (var type in extendedTypes)
                    {
                        var parentProperty = PropertyInfo.PropertyType.GenericTypeArguments[0].GetProperties().ToList().Find(i => i.Name == $"Id{type.Name}");
                        if (parentProperty != null)
                            idName = type.Name;
                    }

                    notFilteredItemsSource = await UniDataSet?.Get(id: ParentItem.ID,
                                                                   idName: idName,
                                                                   blockToReturn: SelectedDataBlockNumber,
                                                                   requestedEntriesNumber: 50,
                                                                   filterText: SearchText,
                                                                   filterExpressions: filterExpressions);
                }
                else
                    notFilteredItemsSource = await UniDataSet?.Get(blockToReturn: SelectedDataBlockNumber,
                                                                   requestedEntriesNumber: 50,
                                                                   filterText: SearchText,
                                                                   filterExpressions: filterExpressions);

                ItemsSource = new ObservableCollection<T>(notFilteredItemsSource);
                DataBlocksNumber = UniDataSet.DataBlocks;
            }
            IsLoading = false;
        }

        /// <summary>
        /// Called by the keydown event of the textbox search for enter key and also by the button search
        /// </summary>
        /// <param name="parameter"></param>
        protected override void SearchCommand(object parameter)
        {
            if (parameter == null || (parameter is KeyRoutedEventArgs args && args.Key == Windows.System.VirtualKey.Enter))
            {
                SelectedDataBlockNumber = 1;
                DataBlocksNumber = 1;
                PopulateItemsSource();
            }
        }
    }
}
