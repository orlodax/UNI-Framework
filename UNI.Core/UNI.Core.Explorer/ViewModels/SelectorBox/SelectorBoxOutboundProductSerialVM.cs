using System.Collections.Generic;
using System.Threading.Tasks;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;
using UNI.Core.UI.CustomControls.SelectorBox;

namespace UNI.Core.Explorer.ViewModels.SelectorBox
{
    public class SelectorBoxOutboundProductSerialVM<T> : SelectorBoxVM<ProductSerial>
    {
        public SelectorBoxOutboundProductSerialVM(
            BaseModel parent,
            string dependencyFilterPropertyName,
            string parentFilterPropertyName,
            string dependencyFilterPropertyValue) : base(
                parent,
                dependencyFilterPropertyName,
                parentFilterPropertyName,
                dependencyFilterPropertyValue)
        { 
        
        }

        public override Task LoadData(List<FilterExpression> filterExpressions = null)
        {
            filterExpressions = new List<FilterExpression>
            {
                new FilterExpression() { PropertyName = "Quantity", PropertyValue = "1" }
            };
            return base.LoadData(filterExpressions);
        }
    }
}
