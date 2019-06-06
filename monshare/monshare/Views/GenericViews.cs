using monshare.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using monshare.Pages;
using System.Threading.Tasks;
using monshare.Utils;

namespace monshare.Views
{
    class GenericViews
    {
        internal static View GroupListElement(Group group)
        {

            StackLayout stackLayout = new StackLayout() { HorizontalOptions = LayoutOptions.FillAndExpand};
            StackLayout wrapperLayout = new StackLayout() { Orientation = StackOrientation.Horizontal };
            stackLayout.Children.Add(new Label()
            {
                Text = "👋 " + group.Title,
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label))
            });

            stackLayout.Children.Add(new Label()
            {
                Text = group.Description,
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label))
            });

            Button joinGroupButton = new Button() { Text = "J🌟in"};
            joinGroupButton.Clicked += async (s, e) => { await ServerCommunication.JoinGroup(group.GroupId); };

            wrapperLayout.Children.Add(stackLayout);
            wrapperLayout.Children.Add(joinGroupButton);

            Frame frame = new Frame()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Padding = 30,
                Content = wrapperLayout,
            };

            TapGestureRecognizer gestureRecognizer = new TapGestureRecognizer();
            gestureRecognizer.Tapped += async (s, e) => await Application.Current.MainPage.Navigation.PushAsync(new GroupDescriptionPage(group));
            stackLayout.GestureRecognizers.Add(gestureRecognizer);

            return frame;
            
        }
    }
}
