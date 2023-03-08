using Windows.UI.Xaml.Controls;

// Il modello di elemento Finestra di dialogo contenuto è documentato all'indirizzo https://go.microsoft.com/fwlink/?LinkId=234238

namespace UNI.Core.UI.ContentDialogs.LogDialog
{
    public sealed partial class LogDialog : ContentDialog
    {
        private string logContent = string.Empty;

        public LogDialog()
        {
            this.InitializeComponent();
        }

        public string LogContent
        {
            get => logContent;
            set
            {
                logContent = value;
                Log.Text = value;
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
