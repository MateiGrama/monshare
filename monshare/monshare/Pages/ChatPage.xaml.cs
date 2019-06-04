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

                Thickness margin = msg.isOwnMessage ? new Thickness(20, 5, 0, 100): new Thickness(100, 5, 0, 20);
                Color color = msg.isOwnMessage ? Color.FromHex("657b83") : Color.FromHex("93a1a1");

                Frame msgFrame = new Frame() { HorizontalOptions = LayoutOptions.FillAndExpand,
                    Padding = 20,
                    Margin = margin,
                    BackgroundColor = color
                };

                StackLayout stack = new StackLayout() { HorizontalOptions = LayoutOptions.FillAndExpand };
                stack.Children.Add(new Label() { Text = msg.text });
                msgFrame.Content = stack;
                chatLayout.Children.Add(msgFrame);
             }
        }
    }
}