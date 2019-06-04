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
    public partial class GroupDescriptionPage : ContentPage
    {

        private Group CurrentGroup;

        public GroupDescriptionPage(Group group)
        {
            InitializeComponent();

            CurrentGroup = group;
            GroupNumber.Text = group.GroupId.ToString();

            // Display the delete group button only for the owner of the group
            if(CurrentGroup.OwnerId == LocalStorage.GetUserId())
            {
                DeleteGroupButton.IsVisible = true;
                DeleteGroupButton.HeightRequest = -1;
            }
            
        }

        private async void ViewChatButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ConversationPage());
        }

        private async void LeaveGroupClicked(object sender, EventArgs e)
        {
            if (await Utils.Utils.ShowLeaveGroupDialog(this, "Leave group", "Are you sure you want to leave the group?"))
            {
                if (await ServerCommunication.LeaveGroupAsync(CurrentGroup.GroupId))
                {
                    await DisplayAlert("Group Left", "You have successfully left the group", "OK");
                    await Navigation.PopAsync();
                }
            }
        }


        private async void DeleteGroupButtonPressed(object sender, EventArgs e)
        {
            if (await Utils.Utils.ShowLeaveGroupDialog(this, "Delete Group", "Are you sure you want to delete this group?"))
            {
                if (await ServerCommunication.DeleteGroup(CurrentGroup.GroupId))
                {
                    await DisplayAlert("Group deleted", "You have successfully deleted your group", "Ok");
                    await Navigation.PopAsync();
                }
            }
        }
    }
}