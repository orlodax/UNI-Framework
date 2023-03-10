using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UNI.API.Client;
using UNI.Core.UI.MainPage;
using UNI.Core.UI.Misc;
using UNI.Core.UI.Services.Privileges;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UNI.Core.UI
{
    public class UNICompositionRoot
    {
        public static TabViewVM TabViewVM { get; set; }

        #region VM load data behaviour
        private static readonly List<Type> vmTypes = new List<Type>();

        /// <summary>
        /// Called from final frontend to prevent default data loading behaviour
        /// </summary>
        /// <param name="addingVmTypes"></param>
        public static void BlockBaseViewModels(List<Type> addingVmTypes)
        {
            vmTypes.AddRange(addingVmTypes);
        }

        /// <summary>
        /// Called from base VMs (i.e. ListGridVM) to check on the above
        /// </summary>
        /// <param name="vmType"></param>
        /// <returns></returns>
        public static bool ContainsBaseViewModel(Type vmType)
        {
            return vmTypes.Any(v => v == vmType);
        }
        #endregion

        #region Login

        /// must be injected by any UNI.Explorer
        private static Type FinalMainPageVMType = null;

        public static async void TryLogin(string username, string password, bool saveCredentials, Type finalMainPageVMType = null)
        {
            // store the main page vm type when called first time
            if (finalMainPageVMType != null)
                FinalMainPageVMType = finalMainPageVMType;

            if (FinalMainPageVMType == null)
            {
                var resourceLoader = ResourceLoader.GetForCurrentView();
                var cd = new ContentDialog() { Title = resourceLoader.GetString("warning"), Content = resourceLoader.GetString("login_MainPageVMType_missingError"), CloseButtonText = "OK" };
                await cd.ShowAsync();
            }
            else
            {
                Frame rootFrame = Window.Current.Content as Frame;
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    rootFrame.Navigate(typeof(LoginPage));
                }
                else
                {
                    var client = new UNIClient<UNIUser>();
                    UNIUser.Token = await client.Authenticate(username, password);
                    if (JWTHelper.IsTokenValid())
                    {
                        // login done, start using the app
                        UNIUser.Username = username;
                        UNIUser.Password = password;

                        var appData = ApplicationData.Current.LocalSettings;
                        appData.Values["saveCredentials"] = saveCredentials;
                        appData.Values["username"] = saveCredentials ? username : string.Empty;
                        appData.Values["password"] = saveCredentials ? password : string.Empty;

                        try
                        {
                            // build the new injected type using reflection
                            var mainVM = Activator.CreateInstance(FinalMainPageVMType);

                            rootFrame?.Navigate(typeof(MainPage.MainPage), mainVM);
                        }
                        catch (Exception e)
                        {
                            Debug.Print(e.Message);
                        }
                    }
                    else
                    {
                        // display error and return to login form 
                        var resourceLoader = ResourceLoader.GetForCurrentView();
                        var cd = new ContentDialog() { Title = resourceLoader.GetString("warning"), Content = resourceLoader.GetString("login_connectionFailedError"), CloseButtonText = "OK" };
                        await cd.ShowAsync();

                        rootFrame.Navigate(typeof(LoginPage));
                    }

                }
            }
        }
        #endregion
    }
}
