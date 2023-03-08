using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Collections.Generic;
using UNI.Core.UI.Services.Printing;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace UNI.Core.UI.MainPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            // To customize titlebar
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(BackgroundElementAppTitleBar);
        }

        /// <summary>
        /// Every App SHALL provide the MainPage's VM Type to the UniCompositionRoot or main container 
        /// rootFrame.Navigate(typeof(UI.MainPage.MainPage), new MainPageVM());
        /// where new MainPageVM is the final app implementation, not the base UNI.Core MainPageVM 
        /// </summary>
        /// <param name="e">Contains the VM</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            DataContext = e.Parameter;

            // register to handle all print jobs
            MainPageVM.PrintRequested += Vm_PrintRequested;
        }

        #region Printing

        private PrintHelper _printHelper;

        private async void Vm_PrintRequested(object sender, List<FrameworkElement> controls)
        {
            var printBuilder = new PrintBuilder(sender);

            var pages = printBuilder.BuildPages(controls);

            // Provide the invisible container to the printhelper - bound from xaml to codebehind, reason is not fully MVVM?
            _printHelper = new PrintHelper(printContainer);

            _printHelper.OnPrintCanceled += PrintHelper_OnPrintCanceled;
            _printHelper.OnPrintFailed += PrintHelper_OnPrintFailed;
            _printHelper.OnPrintSucceeded += PrintHelper_OnPrintSucceeded;

            foreach (var page in pages)
                _printHelper.AddFrameworkElementToPrint(page);

            await _printHelper.ShowPrintUIAsync("UNI");
        }

        private void ReleasePrintHelper()
        {
            _printHelper.Dispose();

            printContainer.Children.Clear();
        }

        private void PrintHelper_OnPrintSucceeded()
        {
            ReleasePrintHelper();

            new ToastContentBuilder()
                .AddText(ResourceLoader.GetForCurrentView().GetString("print_printDone"))
                .Show();
        }

        private void PrintHelper_OnPrintFailed()
        {
            ReleasePrintHelper();

            new ToastContentBuilder()
                .AddText(ResourceLoader.GetForCurrentView().GetString("print_printFailed"))
                .Show();
        }

        private void PrintHelper_OnPrintCanceled()
        {
            ReleasePrintHelper();
        }

        #endregion
    }
}