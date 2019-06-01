using monshare.Models;
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
	public partial class LoginPage : ContentPage
	{
		public LoginPage()
		{
			InitializeComponent ();
		}

        public async void LoginClickedAsync(object sender, EventArgs e)
        {
            if (!formInputIsValid())
            {
                return;
            }

            User APICallResult = await ServerCommunication.Login(email.Text, password.Text);
            bool successfulCall = User.NullInstance != APICallResult;

            await DisplayAlert("Login", (successfulCall ? "" : "not ") + "successful", "OK");

            if (successfulCall)
            {
                await Navigation.PopAsync();
            }

        }

        private bool formInputIsValid()
        {
            if (email.Text == null)
            {
                showError("You must provide an email address.");
                return false;
            }

            if (password.Text == null)
            {
                showError("You must provide a password.");
                return false;
            }

            return true;
        }

        private void showError(string msg) => Utils.Utils.ShowError(msg, errorLabel, 20, 3000);
    }
}