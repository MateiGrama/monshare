using Lottie.Forms;
using monshare.Models;
using monshare.Pages.GroupDescription;
using monshare.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        private string FontAwsomeName = FontAwesome.GetFontAwsomeName();
        private Group Group;
        List<User> Members;


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
                   new Position(Group.Latitude, Group.Longitude), Distance.FromMiles(0.3))){
                IsShowingUser = true,
                HeightRequest = 200,
                WidthRequest = 960,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Margin = 0,
            };

            var pin = new Pin()
            {

                Position = new Position(Group.Latitude, Group.Longitude),
                Label = "Meeting point!"
            };

            map.Pins.Add(pin);
            return map;
        }

        protected override bool OnBackButtonPressed()
        {
            if (ChatLayout.IsVisible)
            {
                OnSwipedDown(null, null);
                return true;
            }
            return base.OnBackButtonPressed();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Utils.Utils.DisplayVisualElement(AnimationLayout, Group.HasJoined);
            Utils.Utils.DisplayVisualElement(ChatLayout, false);

            populateGroupDetailsView();
            loadMessagesAsync();
        }

        private async void populateGroupDetailsView()
        {
            StackLayout detailsStackLayout = new StackLayout()
            {
                Spacing = 0,
                Padding = 10
            };

            detailsStackLayout.Children.Add(new Label()
            {
                TextColor = Color.FromHex("9a9a9a"),
                Text = Group.CreationDateTime.ToString("ddd d MMM",
                  CultureInfo.CreateSpecificCulture("en-UK")),
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label))
            });

            detailsStackLayout.Children.Add(new Label()
            {
                Margin = new Thickness(0, 10),
                TextColor = Color.FromHex("676767"),
                Text = Group.Title,
                FontSize = 32
            });

            StackLayout purpleText = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Margin = new Thickness(0, 5, 0, 10),
            };

            purpleText.Children.Add(new Label()
            {
                HorizontalOptions = LayoutOptions.StartAndExpand,
                TextColor = Color.FromHex("351e29"),
                Text = "by " + await getOwnerName(),
                FontSize = 16
            });



            purpleText.Children.Add(new Label()
            {
                HorizontalOptions = LayoutOptions.End,
                TextColor = Color.FromHex("351e29"),
                FontFamily = FontAwsomeName,
                Text = Group.MembersNumber + "/" + Group.TargetNumberOfPeople + " " + FontAwesome.Group,
                FontSize = 16
            });

            detailsStackLayout.Children.Add(purpleText);
            detailsStackLayout.Children.Add(new BoxView()
            {
                Color = Color.FromHex("9a9a9a"),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                HeightRequest = 1
            });

            detailsStackLayout.Children.Add(new Label()
            {
                Margin = new Thickness(0, 10),
                Text = Group.Description,
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label))
            });

            RelativeLayout detailsRelativeLayout = new RelativeLayout()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Padding = 0,
                Margin = 0
            };

            Frame frame = new Frame()
            {
                CornerRadius = 30,
                Content = detailsStackLayout
            };

            detailsRelativeLayout.Children.Add(GetMapView(),
                Constraint.RelativeToParent((parent) =>
                {
                    return 0;
                }), Constraint.RelativeToParent((parent) =>
                {
                    return 0;
                }), Constraint.RelativeToParent((parent) =>
                {
                    return parent.Width;
                }), Constraint.RelativeToParent((parent) =>
                {
                    return 0.6 * parent.Height;
                }));

            detailsRelativeLayout.Children.Add(frame,
                Constraint.RelativeToParent((parent) =>
                {
                    return parent.Width * 0.1;
                }), Constraint.RelativeToParent((parent) =>
                {
                    return parent.Height * 0.4;
                }), Constraint.RelativeToParent((parent) =>
                {
                    return parent.Width * 0.8;
                }), Constraint.RelativeToParent((parent) =>
                {
                    return 0.6 * parent.Height;
                }));

            GroupDetailsLayout.Children.Clear();
            GroupDetailsLayout.Children.Add(detailsRelativeLayout);

        }

        private async Task<string> getOwnerName()
        {
            if (Members?.Count > 0)
            {
                User owner = Members.Where(m => m.UserId == Group.OwnerId).FirstOrDefault();
                return owner.FirstName + " " + owner.LastName;
            }
            await Task.Delay(300);
            string result = await getOwnerName();
            return result;

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
        private async void OnSwipedUp(object sender, EventArgs e)
        {
            if (ChatLayout.IsVisible || !Group.HasJoined) return;

            Title = Group.Title;
            NavigationPage.SetHasBackButton(this, false);

            GroupRelativeLayout.RaiseChild(ChatLayout);
            GroupRelativeLayout.LowerChild(GroupDetailsLayout);

            await ChatLayout.TranslateTo(0, Application.Current.MainPage.Height, 0);
            Utils.Utils.DisplayVisualElement(ChatLayout, true);
            await Task.Delay(50);
            await ChatLayout.TranslateTo(0, 0, 250, Easing.Linear);
            Utils.Utils.DisplayVisualElement(GroupDetailsLayout, false);
        }
        private async void OnSwipedDown(object sender, EventArgs e)
        {
            if (GroupDetailsLayout.IsVisible) return;

            Title = "";
            NavigationPage.SetHasBackButton(this, true);

            GroupRelativeLayout.RaiseChild(GroupDetailsLayout);
            GroupRelativeLayout.LowerChild(ChatLayout);

            await GroupDetailsLayout.TranslateTo(0, (-1) * Application.Current.MainPage.Height, 0);
            Utils.Utils.DisplayVisualElement(GroupDetailsLayout, true);
            await GroupDetailsLayout.TranslateTo(0, 0, 250, Easing.Linear);
            Utils.Utils.DisplayVisualElement(ChatLayout, false);

            GroupRelativeLayout.RaiseChild(AnimationLayout);
        }
        private async void loadMessagesAsync()
        {
            Members = await ServerCommunication.getGroupMembers(Group.GroupId);
            Chat chat = await ServerCommunication.GetGroupChatAsync(Group);
            bool isAPICallSuccessful = chat != Chat.NullInstance;

            if (!isAPICallSuccessful)
            {
                await DisplayAlert("Alert", "Messages not loaded; reason: " + chat.message, "ok");
                return;
            }
            List<Message> sortedMesages = chat.messages.OrderBy(msg => msg.DateTime).ToList();
            foreach (Message msg in sortedMesages)
            {
                //ReceivedMessageView messageView = new ReceivedMessageView() { BindingContext = msg };
                addMessageInLayout(msg);

            }
            scrollToBottom();
        }

        public async void SendButtonPressed(object sender, EventArgs args)
        {
            if (await ServerCommunication.sendMessage(messageEntry.Text, Group.GroupId))
            {
                addMessageInLayout(new Message() { SenderId = LocalStorage.GetUserId(), Text = messageEntry.Text });
                scrollToBottom();
            }
            messageEntry.Text = "";
        }

        private void addMessageInLayout(Message msg)
        {
            Thickness margin = msg.IsOwnMessage ? new Thickness(60, 5, 15, 0) : new Thickness(15, 5, 60, 0);
            Color color = msg.IsOwnMessage ? Color.FromHex("f0f0f0") : Color.FromHex("dcdcdc");

            Frame msgFrame = new Frame()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Padding = 15,
                Margin = margin,
                BackgroundColor = color,
                CornerRadius = 8
            };

            StackLayout stack = new StackLayout() { HorizontalOptions = LayoutOptions.FillAndExpand };
            if (msg.SenderId != LocalStorage.GetUserId())
            {
                stack.Children.Add(new Label() { Text = getSenderName(msg.SenderId), Font = Font.SystemFontOfSize(15, FontAttributes.Bold) });
            }
            stack.Children.Add(new Label() { Text = msg.Text });
            msgFrame.Content = stack;
            chatLayout.Children.Add(msgFrame);
        }

        private string getSenderName(int senderId)
        {
            foreach (User user in Members)
            {
                if (user.UserId == senderId)
                {
                    return user.FirstName + " " + user.LastName;
                }
            }

            return "Random User";
        }

        private async void scrollToBottom()
        {
            await chatScrollView.ScrollToAsync(chatLayout, ScrollToPosition.End, false);
        }

    }
}