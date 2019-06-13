using monshare.Models;
using monshare.Utils;
using monshare.Views;
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
	public partial class ChatPage : ContentPage
	{
        Group Group;
        List<User> members;

		public ChatPage (Group group)
		{
			InitializeComponent ();
            this.Group = group;
		}

        protected override void OnAppearing()
        {
            base.OnAppearing();
            loadMessagesAsync();
        }

        private async void loadMessagesAsync()
        {
            members = await ServerCommunication.getGroupMembers(Group.GroupId);
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

        private void addMessageInLayout(Message msg) {
             Thickness margin = msg.IsOwnMessage ? new Thickness(60, 5, 15, 0) : new Thickness(15, 5, 60, 0);
                Color color = msg.IsOwnMessage ? Color.FromHex("657b83") : Color.FromHex("93a1a1");

                Frame msgFrame = new Frame() { HorizontalOptions = LayoutOptions.FillAndExpand,
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
            foreach (User user in members)
            {
                if (user.UserId == senderId) {
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