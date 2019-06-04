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
	public partial class AuthentificationPage : ContentPage
	{
		public AuthentificationPage ()
		{
			InitializeComponent ();
            NavigationPage.SetHasBackButton(this, false);
        }

        protected override bool OnBackButtonPressed()
        {  
            return true;
        }

        public async void LoginButtonPressed(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new LoginPage());
        }

        public async void RegisterButtonPressed(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new RegisterPage());
        }
    }
}