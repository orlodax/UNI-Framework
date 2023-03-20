using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Windows.Input;
using UNI.API.Client;
using UNI.API.Contracts.RequestsDTO;
using UNI.Core.UI.Misc;
using UNI.Core.UI.Tabs;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UNI.Core.UI.Menu.Settings
{
    internal class SettingsPageVM : BaseTabVMTypeAgnostic
    {
        public string UserDataHeader { get; set; }
        public string UserLabel { get; set; }
        public string Username { get; set; }
        public string ChangePasswordLabel { get; set; }
        public bool IsUserAdmin { get; set; }
        public string AdminToolsHeader { get; set; }
        public UserManagerVM UserManagerVM { get; set; }

        public SettingsPageVM()
        {
            ViewType = typeof(SettingsPage);

            Username = UNIUser.Username;
            IsUserAdmin = JWTHelper.IsCurrentUserInRole("Admin");
            UserManagerVM = new UserManagerVM();

            GetStringResources();
            BindCommands();
        }

        private void GetStringResources()
        {
            UserDataHeader = ResourcesHelper.GetString("settings_userDataHeader", "User data");
            UserLabel = ResourcesHelper.GetString("settings_userLabel", "User");
            ChangePasswordLabel = ResourcesHelper.GetString("settings_changePasswordLabel", "Change my password...");
            AdminToolsHeader = ResourcesHelper.GetString("settings_adminToolsLabel", "Admin Tools");
        }

        #region Commands
        public ICommand ChangePassword { get; set; }

        private void BindCommands()
        {
            ChangePassword = new RelayCommand(async (parameter) =>
            {
                string oldPasswordLabel = ResourcesHelper.GetString("settings_changePasswordOldPasswordLabel", "Current Password");
                PasswordBox oldPassword = new PasswordBox()
                {
                    Header = oldPasswordLabel,
                    PlaceholderText = oldPasswordLabel,
                    Width = 250,
                    Margin = new Thickness(5),
                };
                string newPasswordLabel = ResourcesHelper.GetString("settings_changePasswordNewPasswordLabel", "New Password");
                PasswordBox newPassword = new PasswordBox()
                {
                    Header = newPasswordLabel,
                    PlaceholderText = newPasswordLabel,
                    Width = 250,
                    Margin = new Thickness(5),
                };
                StackPanel stackPanel = new StackPanel()
                {
                    Margin = new Thickness(10),
                    Orientation = Orientation.Vertical,
                };
                stackPanel.Children.Add(oldPassword);
                stackPanel.Children.Add(newPassword);
                ContentDialog dialog = new ContentDialog
                {
                    Title = ResourcesHelper.GetString("settings_changePasswordLabel", "Change Password"),
                    Content = stackPanel,
                    PrimaryButtonText = ResourcesHelper.GetString("confirm", "Confirm"),
                    CloseButtonText = ResourcesHelper.GetString("cancel", "Cancel"),
                    DefaultButton = ContentDialogButton.Primary,
                };

                ContentDialogResult result = await dialog.ShowAsync();

                //Change password if the user clicked the primary button.
                if (result != ContentDialogResult.Primary)
                    return;

                if (await new UNIClient<UNIUser>().ChangePassword(new ChangePasswordRequestDTO { Username = UNIUser.Username, OldPassword = oldPassword.Password, NewPassword = newPassword.Password }))
                {
                    new ToastContentBuilder()
                            .AddText(ResourcesHelper.GetString("settings_changePasswordSuccess", "Password changed!"))
                            .Show();
                }
                else
                {
                    new ToastContentBuilder()
                            .AddText(ResourcesHelper.GetString("settings_changePasswordError", "Error changing password, please try again later"))
                            .Show();
                }
            });
        }
        #endregion
    }
}
