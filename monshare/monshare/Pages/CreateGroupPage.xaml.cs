using monshare.Models;
using monshare.Utils;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace monshare.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateGroupPage : ContentPage
    {
        private TimePicker timePicker = new TimePicker();
        private Place SelectedPlace;
        public CreateGroupPage(Place SelectedPlace)
        {
            InitializeComponent();
            timePicker.Time = DateTime.Now.AddDays(1).TimeOfDay;
            Title = "Create Group";
            this.SelectedPlace = SelectedPlace;

            if(SelectedPlace != Place.DummyPlace)
            {
                title.Text = SelectedPlace.Name.Trim('"');
            }
        }

        private async void CreateGroupClicked(object sender, EventArgs e)
        {
            string groupTitle = title.Text ?? "";
            string groupDescription = description.Text ?? "";
            string rangeString = System.Text.RegularExpressions.Regex.Replace(range.SelectedItem.ToString() ?? "", "[^0-9.]", "");

            bool APICallResult = await ServerCommunication.CreateGroupAsync(
                groupTitle,
                groupDescription,
                Int32.Parse(rangeString),
                DateTime.Parse(timePicker.Time.ToString()),
                Int32.Parse(targetNoPeople.SelectedItem.ToString() ?? "0"),
                SelectedPlace != Place.DummyPlace ? SelectedPlace.Id : "");

            await DisplayAlert("Create group", (APICallResult ? "" : "not ") + "successful", "OK");

            if (APICallResult)
            {
                await Navigation.PushAsync(new MyGroupsPage());
            }
        }
    }
}