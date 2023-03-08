using UNI.Core.Library;
using Windows.UI.Xaml;

namespace UNI.Core.UI.Tabs.ListDetail
{
    public class ListDetailVB<T> : ViewBuilder.ViewBuilder<T> where T : BaseModel
    {
        public DataTemplate SelectItemListTemplate()
        {
            if (Application.Current.Resources.TryGetValue(typeof(T).Name + "_List", out object template))
                return template as DataTemplate;

            return new DataTemplate();
        }

        public override FrameworkElement RenderMainFrameworkElement(BaseModel selectedItem, string pageGroup = null, int? height = null)
        {
            string baseitem = ResourceLoader.GetString($"baseModel_{typeof(T).Name}");
            if (string.IsNullOrWhiteSpace(baseitem))
                baseitem = typeof(T).Name;

            if (pageGroup == baseitem)
                pageGroup = null;

            //iterate through properties to render control and assign it to container. THIS MUST be regenerated each time
            var controls = GetPropertyControl(selectedItem, pageGroup);

            return RenderGrid(controls, isListDetail: true);
        }
    }
}
