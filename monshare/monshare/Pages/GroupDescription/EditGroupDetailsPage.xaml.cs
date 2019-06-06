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
    public partial class EditGroupDetailsPage : ContentPage
    {
        private readonly Group group;

        public EditGroupDetailsPage(Group group)
        {
            InitializeComponent();
            this.group = group;

            title.Text = group.Title;
            description.Text = group.Description;
            targetNoPeople.SelectedItem = targetNoPeople.Items[group.TargetNumberOfPeople / 5 - 1];

            GenerateSaveDiscardButtons();

        }

        private void GenerateSaveDiscardButtons()
        {
            var buttonsStackLayout = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.CenterAndExpand
            };

            var discardChangesButton = new Button()
            {
                Text = "Discard"
            };

            discardChangesButton.Clicked += DiscardChangesButtonPressed;

            var saveChangesButton = new Button()
            {
                Text = "Save"
            };

            saveChangesButton.Clicked += SaveChangesButtonPressed;

            buttonsStackLayout.Children.Add(discardChangesButton);
            buttonsStackLayout.Children.Add(saveChangesButton);
            GroupDetailsLayout.Children.Add(buttonsStackLayout);
        }

        private async void DiscardChangesButtonPressed(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void SaveChangesButtonPressed(object sender, EventArgs e)
        {
            string oldTitle = group.Title;
            string oldDescription = group.Description;
            int oldTargetNumOfPeople = group.TargetNumberOfPeople;

            group.Title = title.Text;
            group.Description = description.Text;
            group.TargetNumberOfPeople = Int32.Parse(targetNoPeople.SelectedItem.ToString());

            bool successfulCall = await ServerCommunication.UpdateGroup(group); 

            await DisplayAlert("Updated", (successfulCall ? "" : "not ") + "successful", "OK");

            if (!successfulCall) {
                group.Title = oldTitle;
                group.Description = oldDescription;
                group.TargetNumberOfPeople = oldTargetNumOfPeople;
            }

            await Navigation.PopAsync();
        }
    }
}