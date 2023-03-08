using Windows.UI.Xaml.Controls;

namespace UNI.Core.UI.Services.Printing
{
    public sealed partial class PrintedField : UserControl
    {
        public string FieldValue { get; set; }
        public string FieldName { get; set; }

        public PrintedField()
        {
            this.InitializeComponent();
        }
    }
}