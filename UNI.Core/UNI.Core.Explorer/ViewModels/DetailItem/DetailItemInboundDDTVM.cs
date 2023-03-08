using System.Collections.Generic;
using UNI.Core.Library;
using UNI.Core.UI.Tabs.DetailItem;
using UNI.Core.UI.ViewBuilder;
using Windows.UI.Xaml;

namespace UNI.Core.Explorer.ViewModels.DetailItem
{
    public class DetailItemInboundDDTVM<T> : DetailItemVM<T> where T : BaseModel
    {
        #region ExtendedVM interface implementation
        public void SetCustomProperties()
        {
            // either set ViewBuilder properties or inject a custom one
            //ViewBuilder.VisibleProperties = ...
            ViewBuilder = new DetailItemInboundDDTVB<T>();
            //ViewBuilder.CustomizeViewModel(typeof(GridBoxDataSet), typeof(GridBoxVMCustom))
            NewItemVM = new NewItemInboundProductSerialVM<T>();
            ViewBuilder.CustomizeNewItemVM(EnControlTypes.ListGrid, typeof(NewItemOutboundDDTRowVM<>));
            ViewBuilder.CustomizeVisibleProperties(new List<string> { "Property1", "Property2" });
        }
        #endregion

        /// <summary>
        /// CTOR: remember to call the RenderMainFramework as reminded by the interface. Also, write the out controls to BaseCrud<T>.Controls list property.
        /// </summary>
        /// <param name="selectedItem"></param>
        public DetailItemInboundDDTVM(T selectedItem) : base(selectedItem)
        {
            SetCustomProperties();

            ExportButtonVisibility = Visibility.Visible;

            ViewBuilder.RenderMainFrameworkElement(selectedItem);
        }


    }
}
