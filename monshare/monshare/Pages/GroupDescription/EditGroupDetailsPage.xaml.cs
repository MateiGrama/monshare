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
        private readonly GroupDetailVisualElementsGenerator groupDetailsFields;

        public EditGroupDetailsPage(Group group)
        {
            InitializeComponent();
            this.group = group;
            groupDetailsFields = new GroupDetailVisualElementsGenerator();
            groupDetailsFields.CreateGroupDetailFields(group, false, GroupDetailsLayout);
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

            group.Title = groupDetailsFields.GroupNameEntry.Text;
            group.Description = groupDetailsFields.GroupDescriptionEditor.Text;

            bool successfulCall = await ServerCommunication.UpdateGroup(group);

            await DisplayAlert("Updated", (successfulCall ? "" : "not ") + "successful", "OK");

            if (!successfulCall) {
                group.Title = oldTitle;
                group.Description = oldDescription;
            }

            await Navigation.PopAsync();
        }
    }
}