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
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        public async void CreateGroupClickedAsync(object sender, EventArgs e)
        {
            if (!formInputIsValid())
            {
                return;
            }
            User APICallResult = await ServerCommunication.Register(
                email.Text, firstName.Text, lastName.Text, password.Text
                );
            bool successfulCall = User.NullInstance != APICallResult;

            await DisplayAlert("Register", (successfulCall ? "" : "not ") + "successful", "OK");

            if (successfulCall)
            {
                await Navigation.PopToRootAsync(false);
            }

        }

        private bool formInputIsValid()
        {
            //email.Text, firstName.Text, lastName.Text, password.Text
            if (email.Text == null || !Utils.Utils.IsValidEmail(email.Text))
            {
                showError("Invalid email.");
                return false;
            }

            if (firstName.Text == null || firstName.Text.Length == 0)
            {
                showError("Please insert your first name.");
                return false;
            }

            if (firstName.Text.Length > 30)
            {
                showError("Shorter first name please.");
                return false;
            }

            if (lastName.Text == null || lastName.Text.Length == 0)
            {
                showError("Please insert your last name.");
                return false;
            }

            if (lastName.Text.Length > 30)
            {
                showError("Shorter last name, please.");
                return false;
            }

            if (firstName.Text == null || firstName.Text.Length == 0)
            {
                showError("Shorter first name please.");
                return false;
            }

            if (password.Text == null || password.Text.Length < 7 || password.Text.Length > 20)
            {
                showError("Password length must contain between 8 and 20.");
                return false;
            }

            if(password.Text != repeatPassword.Text)
            {
                showError("Passwords do not match.");
                return false;
            }

            return true;
        }

        private void showError(string msg) => Utils.Utils.ShowError(msg, errorLabel, 20, 3000);
    }
}