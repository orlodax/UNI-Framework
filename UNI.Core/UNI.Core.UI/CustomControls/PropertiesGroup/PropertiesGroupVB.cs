using System;
using UNI.Core.Library;
using UNI.Core.UI.ViewBuilder;
using Windows.UI.Xaml;

namespace UNI.Core.UI.CustomControls.PropertiesGroup
{
    internal class PropertiesGroupVB<T> : ViewBuilder<T> where T : BaseModel
    {
        public override FrameworkElement RenderMainFrameworkElement(BaseModel selectedItem = null, string pageGroup = null, int? height = null)
        {
            throw new NotImplementedException();
        }
    }
}
