using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using monshare.Pages;
using monshare.Models;

namespace monshare.Utils
{
    public class Utils
    {
        private static CachedData<Position> cachedPosition;
        public static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string HashPassword(string password)
        {

            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(password))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        public static SearchPage GetSearchPage(INavigation navigation)
        {
            foreach (Page page in navigation.NavigationStack)
            {
                if (page.GetType() == typeof(SearchPage)) {
                    return (SearchPage)page;
                }
            }

            return null;
        }

        public static bool IsValidEmail(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static async void ShowError(string msg, Label errorLabel, int height, int duration)
        {
            errorLabel.Text = msg;
            errorLabel.HeightRequest = height;
            errorLabel.IsVisible = true;
            await Task.Delay(duration);
            errorLabel.Text = "";
            errorLabel.IsVisible = true;
            errorLabel.HeightRequest = 0;
        }

        public static async Task<PermissionStatus> CheckPermissions(Permission permission)
        {
            var permissionStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(permission);
            bool request = false;
            if (permissionStatus == PermissionStatus.Denied)
            {
                if (Device.RuntimePlatform == Device.iOS)
                {

                    var title = $"{permission} Permission";
                    var question = $"{permission} permissions are required for a group to be created. Please go into Settings and turn on {permission}.";
                    var positive = "Settings";
                    var negative = "Maybe Later";
                    var task = Application.Current?.MainPage?.DisplayAlert(title, question, positive, negative);
                    if (task == null)
                    {
                        return permissionStatus;
                    }


                    var result = await task;
                    if (result)
                    {
                        CrossPermissions.Current.OpenAppSettings();
                    }

                    return permissionStatus;
                }

                request = true;

            }

            if (request || permissionStatus != PermissionStatus.Granted)
            {
                var newStatus = await CrossPermissions.Current.RequestPermissionsAsync(permission);

                if (!newStatus.ContainsKey(permission))
                {
                    return permissionStatus;
                }

                permissionStatus = newStatus[permission];

                if (newStatus[permission] != PermissionStatus.Granted)
                {
                    permissionStatus = newStatus[permission];
                    var title = $"{permission} Permission";
                    var question = $"{permission} permissions are required for a group to be created. Please go into Settings and turn on {permission}.";
                    var positive = "Settings";
                    var negative = "Maybe Later";
                    var task = Application.Current?.MainPage?.DisplayAlert(title, question, positive, negative);
                    if (task == null)
                    {
                        return permissionStatus;
                    }

                    var result = await task;
                    if (result)
                    {
                        CrossPermissions.Current.OpenAppSettings();
                    }
                    return permissionStatus;
                }
            }
            return permissionStatus;
        }

        public static async Task<bool> ShowLeaveGroupDialog(Page sender, string err, string message)
        {
            return await sender.DisplayAlert(err, message, "Yes", "No");

        }

        public static void DisplayVisualElement(VisualElement element, bool visible)
        {
            element.HeightRequest = visible ? -1 : 0;
            element.IsVisible = visible;
        }

        public static async Task<Position> GetLocationAfterCheckingPermisionsAsync()
        {
            if (cachedPosition != null && cachedPosition.LastCached.AddSeconds(20) > DateTime.Now)
            {
                return cachedPosition.Data;
            }

            var status = await CheckPermissions(Permission.Location);

            if (status != PermissionStatus.Granted)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please grant location permissions", "Ok");
                return null;
            }

            cachedPosition = new CachedData<Position>()
            {
                Data = await CrossGeolocator.Current.GetPositionAsync(TimeSpan.FromSeconds(5)),
                LastCached = DateTime.Now

            };

            return cachedPosition.Data;
        }

    }
}
