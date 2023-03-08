using Microsoft.Toolkit.Uwp.UI.Controls;
using System.Collections.Generic;
using System.Threading.Tasks;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;
using UNI.Core.UI.Tabs.ListGrid;
using Windows.UI.Xaml;

namespace UNI.Core.Explorer.ViewModels
{
    public class ListGridProductVM : ListGridVM<BaseProduct>
    {
        // arbitrary size of the main data grid created below
        const int dataGridHeight = 300;

        #region IExtendedVM implementation DEPRECATED, keep as reference
        public FrameworkElement RenderMainFrameworkElement(BaseModel selectedItem, string pageGroup = null, int? height = null)
        {
            return ViewBuilder.RenderMainFrameworkElement(selectedItem, pageGroup, height);
        }
        public void SetCustomProperties()
        {
            throw new System.NotImplementedException();
        }
        #endregion

        public ListGridProductVM()
        {
            NewItemType = typeof(NewItemVMProductMovement);
            Content = (DataGrid)RenderMainFrameworkElement(SelectedItem, null, dataGridHeight);
        }

        protected override Task LoadData(List<FilterExpression> filterExpressions = null, object parameter = null)
        {
            filterExpressions = new List<FilterExpression> { new FilterExpression() { PropertyName = "IsService", PropertyValue = "false" } };
            return base.LoadData(filterExpressions);
        }
    }

    internal class NewItemVMProductMovement : UI.NewItem.NewItemVM<BaseProduct>
    {
        public NewItemVMProductMovement()
        {
            var visibleProps = new List<string>
            {
                 nameof(BaseProduct.Barcode),
                 nameof(BaseProduct.Description),
                 nameof(BaseProduct.DefaultSellPrice),
                 //nameof(ProductMovement.Product.MeasurementUnit),
            };
            ViewBuilder.CustomizeVisibleProperties(visibleProps);
        }
    }
}