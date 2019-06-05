using monshare.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace monshare.Utils
{
    class GroupDetailVisualElementsGenerator
    {
        public static void CreateGroupDetailFields(Group group, bool isReadOnly, StackLayout groupDetailsLayout)
        {
            var groupNameEntry = new Entry()
            {
                BackgroundColor = Color.LightGray,
                IsReadOnly = isReadOnly,
                Margin = new Thickness(0, 30),
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                WidthRequest = Application.Current.MainPage.Width,
                Text = group.Title
            };

            var groupDescriptionEditor = new Editor()
            {
                BackgroundColor = Color.LightGray,
                IsReadOnly = isReadOnly,
                Margin = new Thickness(0, 10),
                HeightRequest = 200,
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                WidthRequest = Application.Current.MainPage.Width,
                Text = group.Title
            };

            groupDetailsLayout.Children.Add(groupNameEntry);
            groupDetailsLayout.Children.Add(groupDescriptionEditor);
        }
    }
}
