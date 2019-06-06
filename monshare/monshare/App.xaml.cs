using monshare.Pages;
using monshare.Utils;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace monshare
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            loggedInStatusRouting();
        }

        private void loggedInStatusRouting()
        {
            MainPage = new NavigationPage(new SearchPage())
            {
                BarTextColor = Color.FromHex("#ececeb"),
                BarBackgroundColor = Color.FromHex("#351E29")
            };


        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
