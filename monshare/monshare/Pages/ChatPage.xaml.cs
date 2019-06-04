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
        Group group;

		public ChatPage (Group group)
		{
			InitializeComponent ();
            this.group = group;
		}

        protected override void OnAppearing()
        {
            base.OnAppearing();
            loadMessagesAsync();
        }

        private async void loadMessagesAsync()
        {
            Chat chat = await ServerCommunication.getGroupChatAsync(group);
            bool isAPICallSuccessful = chat != Chat.NullInstance;

            if (!isAPICallSuccessful)
            {
                await DisplayAlert("Alert", "Messages not loaded; reason: " + chat.message, "ok");
                return;
            }
            List<Message> sortedMesages = chat.messages.OrderBy(msg => msg.dateTime).ToList();
            foreach (Message msg in sortedMesages)
            {
                //ReceivedMessageView messageView = new ReceivedMessageView() { BindingContext = msg };
                addMessageInLayout(msg);

            }
            scrollToBottom();
        }

        public async void SendButtonPressed(object sender, EventArgs args)
        {
            if (await ServerCommunication.sendMessage(messageEntry.Text, group.GroupId))
            {
                await DisplayAlert("Message Sent", "", "Ok");
                addMessageInLayout(new Message() { senderId = LocalStorage.GetUserId(), text = messageEntry.Text });

                scrollToBottom();
            }
        }

        private void addMessageInLayout(Message msg) {
             Thickness margin = msg.isOwnMessage ? new Thickness(60, 5, 15, 0) : new Thickness(15, 5, 60, 0);
                Color color = msg.isOwnMessage ? Color.FromHex("657b83") : Color.FromHex("93a1a1");

                Frame msgFrame = new Frame() { HorizontalOptions = LayoutOptions.FillAndExpand,
                    Padding = 15,
                    Margin = margin,
                    BackgroundColor = color,
                    CornerRadius = 8
                };

                StackLayout stack = new StackLayout() { HorizontalOptions = LayoutOptions.FillAndExpand };
                stack.Children.Add(new Label() { Text = msg.text });
                msgFrame.Content = stack;
                chatLayout.Children.Add(msgFrame);
        }

        private async void scrollToBottom()
        {
            await chatScrollView.ScrollToAsync(chatLayout, ScrollToPosition.End, true);
        }

    }
}