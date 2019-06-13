using monshare.Models;
using monshare.Utils;
using System;
using System.Collections.Generic;
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
        private List<Group> GroupsAround;

        public CreateGroupPage(Place SelectedPlace, List<Group> GroupsAround)
        {
            InitializeComponent();
            timePicker.Time = DateTime.Now.AddDays(1).TimeOfDay;
            Title = "Create Group";
            this.SelectedPlace = SelectedPlace;
            this.GroupsAround = GroupsAround;

            if (SelectedPlace != Place.DummyPlace)
            {
                title.Text = SelectedPlace.Name.Trim('"');
            }
        }


        private async void CreateGroupClicked(object sender, EventArgs e)
        {
            string groupTitle = title.Text ?? "";
            string groupDescription = description.Text ?? "";
            string rangeString = System.Text.RegularExpressions.Regex.Replace(range.SelectedItem.ToString() ?? "", "[^0-9.]", "");

            bool foundSimilarGroup = false;
            Group similarGroup = null;

            foreach (Group group in GroupsAround)
            {
                if (group.Title.Equals(groupTitle)) {
                    foundSimilarGroup = true;
                    similarGroup = group;
                    break;
                }
            }

            if (foundSimilarGroup && await DisplayAlert("Similar group found", "Would you like to check it out?", "Check it out now", "Create my own"))
            {
                await Navigation.PushAsync(new GroupDescriptionPage(similarGroup));
                return;
            }

            bool APICallResult = await ServerCommunication.CreateGroupAsync(
                groupTitle,
                groupDescription,
                Int32.Parse(rangeString),
                DateTime.Parse(timePicker.Time.ToString()),
                Int32.Parse(targetNoPeople.SelectedItem.ToString() ?? "0"),
                SelectedPlace) != null;

            await DisplayAlert("Create group", (APICallResult ? "" : "not ") + "successful", "OK");

            if (APICallResult)
            {
                await Navigation.PushAsync(new MyGroupsPage());
            }
        }
    }
}