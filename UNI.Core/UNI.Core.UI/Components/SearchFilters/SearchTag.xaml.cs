using System;
using UNI.Core.Library;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UNI.Core.UI.Components.SearchFilters
{
    public sealed partial class SearchTag : UserControl
    {
        public FilterExpression FilterExpression { get; set; }

        public event EventHandler<RoutedEventArgs> Close;

        public SearchTag(FilterExpression filterExpression)
        {
            this.InitializeComponent();
            FilterExpression = filterExpression;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close?.Invoke(sender, e);
        }
    }
}
