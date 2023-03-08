using UNI.Core.Library;
using Windows.UI.Xaml;

namespace UNI.Core.UI.Tabs.DetailItem
{
    public class DetailItemVB<T> : ListDetail.ListDetailVB<T> where T : BaseModel
    {
        public override FrameworkElement RenderMainFrameworkElement(BaseModel selectedItem, string pageGroup = null, int? height = null)
        {
            string baseitem = ResourceLoader.GetString($"baseModel_{typeof(T).Name}");
            if (string.IsNullOrWhiteSpace(baseitem))
                baseitem = typeof(T).Name;

            if (pageGroup == baseitem)
                pageGroup = null;

            //iterate through properties to render control and assign it to container. THIS MUST be regenerated each time
            return RenderGrid(GetPropertyControl(selectedItem, pageGroup));
        }
    }
}
