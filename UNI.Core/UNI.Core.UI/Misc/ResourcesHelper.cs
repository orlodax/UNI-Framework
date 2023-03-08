using Windows.ApplicationModel.Resources;

namespace UNI.Core.UI.Misc
{
    public static class ResourcesHelper
    {
        public static string GetString(string resourceLabel, string fallBackValue)
        {
            string res = ResourceLoader.GetForCurrentView().GetString(resourceLabel);
            return string.IsNullOrWhiteSpace(res) ? fallBackValue : res;
        }
    }
}
