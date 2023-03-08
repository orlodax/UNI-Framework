using System;
using System.Windows.Input;
using UNI.Core.Library;
using UNI.Core.UI.Misc;
using Windows.Storage;

namespace UNI.Core.UI.Services.Privileges
{
    public class LoginPageVM : Observable
    {
        private string username;
        public string Username { get => username; set => SetValue(ref username, value); }

        private string password;
        public string Password { get => password; set => SetValue(ref password, value); }

        private bool saveCredentials;
        public bool SaveCredentials { get => saveCredentials; set => SetValue(ref saveCredentials, value); }

        private string saveCredentialsLabel;
        public string SaveCredentialsLabel { get => saveCredentialsLabel; set => SetValue(ref saveCredentialsLabel, value); }

        private bool isLoading;
        public bool IsLoading { get => isLoading; set => SetValue(ref isLoading, value); }

        private bool isUsernameValid;

        public bool IsUsernameValid { get => isUsernameValid; set { SetValue(ref isUsernameValid, value); ValidateUI(); } }

        private string usernameNotValidMessage;
        public string UsernameNotValidMessage { get => usernameNotValidMessage; set => SetValue(ref usernameNotValidMessage, value); }

        public ICommand LoginCommand { get; private set; }

        public LoginPageVM()
        {
            IsLoading = false;

            var appData = ApplicationData.Current.LocalSettings;
            SaveCredentials = Convert.ToBoolean(appData.Values?["saveCredentials"]);
            SaveCredentialsLabel = ResourcesHelper.GetString("login_saveCredentialsLabel", "Save credentials");

            ValidateUI();

            if (SaveCredentials)
            {
                Username = appData.Values["username"]?.ToString();
                Password = appData.Values["password"]?.ToString();
            }

            LoginCommand = new RelayCommand((parameter) =>
            {
                if (IsUsernameValid)
                {
                    IsLoading = true;
                    UNICompositionRoot.TryLogin(Username, Password, SaveCredentials);
                }
            });
        }

        private void ValidateUI()
        {
            if (IsUsernameValid)
                UsernameNotValidMessage = string.Empty;
            else
                UsernameNotValidMessage = ResourcesHelper.GetString("usernameNotValidMessage", "Username must be an email address");
        }
    }
}
