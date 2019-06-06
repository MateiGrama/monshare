using monshare.Models;
using monshare.Pages.GroupDescription;
using monshare.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace monshare.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GroupDescriptionPage : ContentPage
    {

        private Group Group;

        public GroupDescriptionPage(Group group)
        {
            InitializeComponent();
            Group = group;
            DisplayToolbarItems();
        }

        private Map GetMapView()
        {
           var map = new Map(
               MapSpan.FromCenterAndRadius(
                  new Position(Group.Latitude, Group.Longitude), Distance.FromMiles(0.3)))
                    {
                        IsShowingUser = true,
                        HeightRequest = 200,
                        WidthRequest = 960,
                        VerticalOptions = LayoutOptions.FillAndExpand
                    };

            var pin = new Pin()
            {
                Position = new Position(Group.Latitude, Group.Longitude),
                Label = "Meeting point!"
            };
            map.Pins.Add(pin);

            return map;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            populateView();
        }

        private void populateView()
        {
            StackLayout stackLayout = new StackLayout();

            stackLayout.Children.Add(new Label()
            {
                Text = Group.Title,
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label))
            });

            stackLayout.Children.Add(new Label()
            {
                Text = Group.Description,
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label))
            });

            stackLayout.Children.Add(GetMapView());

            Frame frame = new Frame()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Padding = 30,
                Margin = new Thickness(30),
                Content = stackLayout,
            };

            GroupDetailsLayout.Children.Clear();

            GroupDetailsLayout.Children.Add(frame);

            if (Group.HasJoined)
            {
                var viewChatButton = new Button()
                {
                    Text = "View chat",
                    HorizontalOptions = LayoutOptions.Center
                };
                viewChatButton.Clicked += ViewChatButtonClicked;
                GroupDetailsLayout.Children.Add(viewChatButton);
            }
        }

        private void DisplayToolbarItems()
        {

            if (!Group.HasJoined)
            {
                return;
            }

            if (Group.OwnerId == LocalStorage.GetUserId())
            {
                var editToolbarItem = new ToolbarItem()
                {
                    Text = "Edit",
                    Order = ToolbarItemOrder.Secondary

                };
                editToolbarItem.Clicked += EditGroupButtonPressed;
                this.ToolbarItems.Add(editToolbarItem);
            }

            var groupMembersToolbarItem = new ToolbarItem()
            {
                Text = "Group members",
                Order = ToolbarItemOrder.Secondary

            };
            groupMembersToolbarItem.Clicked += GroupMembersClicked;
            this.ToolbarItems.Add(groupMembersToolbarItem);

            var leaveGroupToolbarItem = new ToolbarItem()
            {
                Text = "Leave Group",
                Order = ToolbarItemOrder.Secondary

            };
            leaveGroupToolbarItem.Clicked += LeaveGroupClicked;
            this.ToolbarItems.Add(leaveGroupToolbarItem);

            if (Group.OwnerId == LocalStorage.GetUserId())
            {
                var deleteGroupToolbarItem = new ToolbarItem()
                {
                    Text = "Delete group",
                    Order = ToolbarItemOrder.Secondary

                };
                deleteGroupToolbarItem.Clicked += DeleteGroupButtonPressed;
                this.ToolbarItems.Add(deleteGroupToolbarItem);
            }
            
            
        }

        private async void ViewChatButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ChatPage(Group));
        }

        private async void EditGroupButtonPressed(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EditGroupDetailsPage(Group));
        }

        private async void LeaveGroupClicked(object sender, EventArgs e)
        {
            if (await Utils.Utils.ShowLeaveGroupDialog(this, "Leave group", "Are you sure you want to leave the group?"))
            {
                if (await ServerCommunication.LeaveGroupAsync(Group.GroupId))
                {
                    await DisplayAlert("Group Left", "You have successfully left the group", "OK");
                    Group.HasJoined = false;
                    await Navigation.PopAsync();
                }
            }
        }

        private async void GroupMembersClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new groupMembersPage(Group.GroupId));
        }


        private async void DeleteGroupButtonPressed(object sender, EventArgs e)
        {
            if (await Utils.Utils.ShowLeaveGroupDialog(this, "Delete Group", "Are you sure you want to delete this group?"))
            {
                if (await ServerCommunication.DeleteGroup(Group.GroupId))
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