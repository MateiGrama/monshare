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
    public partial class MainPage : TabbedPage
    {
        public MainPage()
        {
            InitializeComponent();
            CheckCredentials();
            Children.Add(new SearchPage());
            Children.Add(new GroupsAroundPage());
            Children.Add(new CreateGroupPage());
        }

        private async void CheckCredentials()
        {
            if (!await ServerCommunication.isLoggedIn())
            {
                await Navigation.PushAsync(new AuthentificationPage());
            }

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
        
        public async void SearchButtonPressed(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new SearchPage());
        }

        public async void MyAccountButtonPressed(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MyAccountPage());
        }

        private async void GroupsAroundPressed(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new GroupsAroundPage());
        }
    }
}
