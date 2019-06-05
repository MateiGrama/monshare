using monshare.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace monshare.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyAccountPage : ContentPage
    {
        public MyAccountPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            LoggedInLabel.Text = "Logged in";
            userNameLabel.Text = "User name: " + LocalStorage.GetUserName();
            userIDLabel.Text = "User ID: " + LocalStorage.GetUserId().ToString();
            base.OnAppearing();

        }

        private async void DeleteAccountButtonPressed(object sender, EventArgs e)
        {
            if (await Utils.Utils.ShowLeaveGroupDialog(this, "Delete Account", "Are you sure you want to deleteyour account?"))
            {
                if (await ServerCommunication.DeleteAccount())
                {
                    await DisplayAlert("Account deleted", "You have successfully deleted your account", "Return to Login page");
                    await Navigation.PushAsync(new AuthentificationPage());
                }
            }
        }
    }
}