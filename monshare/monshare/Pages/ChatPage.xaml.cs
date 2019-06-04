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

            foreach (Message msg in chat.messages) {
                //ReceivedMessageView messageView = new ReceivedMessageView() { BindingContext = msg };
                addMessageInLayout(msg.text);
             }
        }

        public async void SendButtonPressed(object sender, EventArgs args)
        {
            if (await ServerCommunication.sendMessage(messageEntry.Text, group.GroupId))
            {
                await DisplayAlert("Message Sent", "", "Ok");
                addMessageInLayout(messageEntry.Text);
            }
        }

        private void addMessageInLayout(String msg) {
            Frame msgFrame = new Frame() { HorizontalOptions = LayoutOptions.FillAndExpand, Padding = 20, Margin = 20, };
            StackLayout stack = new StackLayout() { HorizontalOptions = LayoutOptions.FillAndExpand };
            stack.Children.Add(new Label() { Text = msg });
            msgFrame.Content = stack;
            chatLayout.Children.Add(msgFrame);
        }

    }
}