using monshare.Models;
using monshare.Pages;
using monshare.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace monshare
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            CheckCredentials();
            InitializeComponent();
       
        }

        private async void CheckCredentials()
        {
            if (!await ServerCommunication.isLoggedIn())
            {
                await Navigation.PushAsync(new AuthentificationPage());
            }

            LoggedInLabel.Text = "Logged in";
            userNameLabel.Text = "User name: " + LocalStorage.GetUserName();
            userIDLabel.Text = "User ID: " + LocalStorage.GetUserId().ToString();
        }

        public async void CreateGroupButtonPressed(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new CreateGroupPage());
        }

        public async void LogoutButtonPressed(object sender, EventArgs args)
        {
            bool successfulCall = await ServerCommunication.logout();

            await DisplayAlert("Logout", (successfulCall ? "" : "not ") + "successful", "OK");

            if (successfulCall)
            {
                await Navigation.PushAsync(new AuthentificationPage());
            }
        }

        public async void MyGroupsButtonPressed(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new MyGroupsPage());
        }


        private async void DeleteAccountButtonPressed(object sender, EventArgs e)
        {
            if (await Utils.Utils.ShowLeaveGroupDialog(this, "Delete Account", "Are you sure you want to deleteyour account?"))
            {
                if (await ServerCommunication.DeleteAccount())
                {
                    await DisplayAlert("Account deleted", "You have successfully deleted your account", "Return to Login page");
                    //TODO: GO to login page
                    await Navigation.PopAsync();
                }
            }
        }
    }
}
