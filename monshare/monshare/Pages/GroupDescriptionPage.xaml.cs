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

            GroupDetailVisualElementsGenerator.CreateGroupDetailFields(group, true, GroupDetailsLayout);
            DisplayToolbarItems();
            GenerateViewChatButton();
        }

        private void GenerateViewChatButton()
        {
            var viewChatButton = new Button()
            {
                Text = "View chat"
            };
            viewChatButton.Clicked += ViewChatButtonClicked;
            GroupDetailsLayout.Children.Add(viewChatButton);
        }

        private void DisplayToolbarItems()
        {
            if (CurrentGroup.OwnerId == LocalStorage.GetUserId())
            {
                var editToolbarItem = new ToolbarItem()
                {
                    Text = "Edit",
                    Order = ToolbarItemOrder.Secondary

                };
                editToolbarItem.Clicked += EditGroupButtonPressed;


                var leaveGroupToolbarItem = new ToolbarItem()
                {
                    Text = "Leave Group",
                    Order = ToolbarItemOrder.Secondary

                };
                leaveGroupToolbarItem.Clicked += LeaveGroupClicked;

                var deleteGroupToolbarItem = new ToolbarItem()
                {
                    Text = "Delete group",
                    Order = ToolbarItemOrder.Secondary

                };
                deleteGroupToolbarItem.Clicked += DeleteGroupButtonPressed;

                this.ToolbarItems.Add(editToolbarItem);
                this.ToolbarItems.Add(leaveGroupToolbarItem);
                this.ToolbarItems.Add(deleteGroupToolbarItem);
            }
        }

        private async void ViewChatButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ChatPage(CurrentGroup));
        }

        private async void EditGroupButtonPressed(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EditGroupDetailsPage(CurrentGroup));
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

        private async Task<bool> ShowLeaveGroupDialog()
        {
            return await DisplayAlert("Leave group", "Are you sure you want to leave the group?", "Yes", "No");

        }
    }
}