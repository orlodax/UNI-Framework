using System;
using System.Collections.Generic;
using System.Windows.Input;
using UNI.API.Client;
using UNI.Core.UI.Menu;
using UNI.Core.UI.Misc;
using UNI.Core.UI.Services.Privileges;
using UNI.Core.UI.Tabs;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UNI.Core.UI.MainPage
{
    public abstract class MainPageVM : BaseTabVMTypeAgnostic
    {
        #region PUBLIC SURFACE
        /// <summary>
        /// Implement this in the frontend MainPageVM (and call in the constructor) to draw the menu
        /// </summary>
        public abstract void MenuBuilder();

        /// <summary>
        /// Implement this in the frontend MainPageVM (and call in the constructor) to populate UNICompositionRoot static list of VMs which will be extended via method UNICompositionRoot.BlockBaseViewModels(List(Type) vmTypes)
        /// </summary>
        public abstract void DisableDefaultVMBehaviour();
        #endregion

        #region PROPERTIES
        /// <summary>
        /// Displayed list in the treeview (can be made = null)
        /// </summary>
        private List<MenuNode> menuNodes;
        public List<MenuNode> MenuNodes { get => menuNodes; set => SetValue(ref menuNodes, value); }

        /// <summary>
        /// Display username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Display Name of the app
        /// </summary>
        private string mainWindowTitle = "UNI";
        public string MainWindowTitle { get => mainWindowTitle; set => SetValue(ref mainWindowTitle, value); }

        public TabViewVM TabViewVM { get; set; }

        #endregion

        #region GLOBALS
        //TODO mettere qui gestione delle tabview multiple



        /// <summary>
        /// Print event called by MainPage.cs only to link code behind with XAML print container to any viewmodel calling this
        /// </summary>
        public static event EventHandler<List<FrameworkElement>> PrintRequested;
        public static void Print(object sender, List<FrameworkElement> controls)
        {
            PrintRequested?.Invoke(sender, controls);
        }
        #endregion

        #region CTOR

        /// <summary>
        /// Call the logout method in App.xaml.cs
        /// </summary>
        public ICommand LogoutCommand { get; private set; }

        public MainPageVM()
        {
            TabViewVM = new TabViewVM();
            UNICompositionRoot.TabViewVM = TabViewVM;
            Username = UNIUser.Username;

            LogoutCommand = new RelayCommand((parameter) =>
            {
                var appData = ApplicationData.Current.LocalSettings;
                bool saveCredentials = Convert.ToBoolean(appData.Values?["saveCredentials"]);
                if (!saveCredentials)
                {
                    appData.Values["username"] = string.Empty;
                    appData.Values["password"] = string.Empty;
                }

                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(LoginPage));
            });
        }
        #endregion
    }
}