using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;
using UNI.Core.UI.CustomControls.SelectorBox;

namespace UNI.Core.Explorer.ViewModels.SelectorBox
{
    public class SelectorBoxOutboundProductSerialVM<T> : SelectorBoxVM<ProductSerial>
    {
        public SelectorBoxOutboundProductSerialVM(
            List<FilterExpression> filterExpressions,
            PropertyInfo displayPropertyInfo,
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
            filterExpressions = new List<FilterExpression>();
            filterExpressions.Add(new FilterExpression() { PropertyName = "Quantity", PropertyValue = "1" });
            return base.LoadData(filterExpressions);
        }
    }
}
