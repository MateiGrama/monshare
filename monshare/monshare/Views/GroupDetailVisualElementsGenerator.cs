using monshare.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace monshare.Utils
{
    class GroupDetailVisualElementsGenerator
    {

        public Entry GroupNameEntry;
        public Editor GroupDescriptionEditor;
        public void CreateGroupDetailFields(Group group, bool isReadOnly, StackLayout groupDetailsLayout)
        {
            GroupNameEntry = new Entry()
            {
                BackgroundColor = Color.LightGray,
                IsReadOnly = isReadOnly,
                Margin = new Thickness(0, 30),
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                WidthRequest = Application.Current.MainPage.Width,
                Text = group.Title
            };

            GroupDescriptionEditor = new Editor()
            {
                BackgroundColor = Color.LightGray,
                IsReadOnly = isReadOnly,
                Margin = new Thickness(0, 10),
                HeightRequest = 150,
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                WidthRequest = Application.Current.MainPage.Width,
                Text = group.Description
            };

            groupDetailsLayout.Children.Add(GroupNameEntry);
            groupDetailsLayout.Children.Add(GroupDescriptionEditor);
        }
    }
}
