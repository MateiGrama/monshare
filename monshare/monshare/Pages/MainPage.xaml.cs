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
            InitializeComponent();
        }

        public async void MyGroupsButtonPressed(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new MyGroupsPage());
        }

        public async void CreateGroupButtonPressed(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new CreateGroupPage());
        }

        public async void LoginButtonPressed(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new LoginPage());
        }

        public async void RegisterButtonPressed(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new RegisterPage());
        }

        private void Button_Clicked(object sender, EventArgs e)
        {

        }
    }
}
