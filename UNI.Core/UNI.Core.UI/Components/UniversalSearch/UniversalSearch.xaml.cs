using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UNI.Core.UI.Components.UniversalSearch
{
    public sealed partial class UniversalSearch : UserControl
    {
        public string UniversalSearchPlaceHolderText { get; set; }
        public string HeaderText { get; set; }

        private readonly ResourceLoader resourceLoader;
        public UniversalSearch()
        {
            resourceLoader = ResourceLoader.GetForCurrentView();
            UniversalSearchPlaceHolderText = resourceLoader.GetString("universalSearch_Search");
            var header = resourceLoader.GetString("universalSearch_UniversalSearch");
            HeaderText = !string.IsNullOrWhiteSpace(header) ? header : "Universal search";

            InitializeComponent();
        }
    }
}
