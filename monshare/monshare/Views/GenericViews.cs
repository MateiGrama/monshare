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
        internal static async Task<View> GroupListElement(Group group)
        {
            StackLayout detailStackLayout = new StackLayout() { Orientation = StackOrientation.Horizontal,
                                                          HorizontalOptions = LayoutOptions.FillAndExpand };
            StackLayout labelStackLayout = new StackLayout() { HorizontalOptions = LayoutOptions.FillAndExpand};
            StackLayout wrapperLayout = new StackLayout() { Orientation = StackOrientation.Horizontal };

            double distance = await group.GetDistanceInKm();
            
            detailStackLayout.Children.Add(new Label()
            {
                Text = distance > 0.15 ? string.Format("🚶 {0:N2}km away", distance) : "Near you",
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                HorizontalOptions = LayoutOptions.StartAndExpand
            });

            detailStackLayout.Children.Add(new Label()
            {
                Text = group.MembersNumber + "/"+ group.TargetNumberOfPeople + "🙎‍♂️",
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                HorizontalOptions = LayoutOptions.EndAndExpand
            });

            labelStackLayout.Children.Add(new Label()
            {
                Text = "👋 " + group.Title,
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label))
            });

            labelStackLayout.Children.Add(new Label()
            {
                Text = group.Description.Length > 30 ? group.Description.Substring(0,30) + " ..." : group.Description,
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label))
            });
            wrapperLayout.Children.Add(labelStackLayout);

            if (!group.HasJoined)
            {
                Button joinGroupButton = new Button() { Text = "Join" };
                joinGroupButton.Clicked += async (s, e) => {
                    if (await ServerCommunication.JoinGroup(group.GroupId)) {
                        wrapperLayout.Children.Remove(joinGroupButton);
                        group.HasJoined = true;
                    }
                };
                wrapperLayout.Children.Add(joinGroupButton);
            }


            StackLayout frameStackLayout = new StackLayout() {
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            frameStackLayout.Children.Add(wrapperLayout);
            frameStackLayout.Children.Add(detailStackLayout);

            Frame frame = new Frame()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Padding = 30,
                Content = frameStackLayout,
            };

            TapGestureRecognizer gestureRecognizer = new TapGestureRecognizer();
            gestureRecognizer.Tapped += async (s, e) => await Application.Current.MainPage.Navigation.PushAsync(new GroupDescriptionPage(group));
            labelStackLayout.GestureRecognizers.Add(gestureRecognizer);

            return frame;
            
        }


        internal static async Task<View> GroupCardList(Group group)
        {
            StackLayout detailStackLayout = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            StackLayout labelStackLayout = new StackLayout() { HorizontalOptions = LayoutOptions.FillAndExpand };

            double distance = await group.GetDistanceInKm();

            detailStackLayout.Children.Add(new Label()
            {
                Text = distance > 0.15 ? string.Format("🏃‍♀️ {0:N2}km away", distance) : "🚶‍♂️ Near you",
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                HorizontalOptions = LayoutOptions.StartAndExpand
            });

            detailStackLayout.Children.Add(new Label()
            {
                Text = group.MembersNumber + "/" + group.TargetNumberOfPeople + "🙎‍♂️",
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                HorizontalOptions = LayoutOptions.EndAndExpand
            });

            labelStackLayout.Children.Add(new Label()
            {
                Text = getRandomEmoji() + " " + group.Title,
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label))
            });

            labelStackLayout.Children.Add(new Label()
            {
                Text = group.Description.Length > 30 ? group.Description.Substring(0, 30) + " ..." : group.Description,
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label))
            });

            StackLayout frameStackLayout = new StackLayout()
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            frameStackLayout.Children.Add(labelStackLayout);
            frameStackLayout.Children.Add(detailStackLayout);
            if (!group.HasJoined)
            {
                Button joinGroupButton = new Button() { Text = "Join"};

                joinGroupButton.Clicked += async (s, e) =>
                {
                    if (await ServerCommunication.JoinGroup(group.GroupId))
                    {
                        frameStackLayout.Children.Remove(joinGroupButton);
                        group.HasJoined = true;
                    }
                };

                frameStackLayout.Children.Add(joinGroupButton);
            }

            Frame frame = new Frame()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Padding = 30,
                Content = frameStackLayout,
            };

            TapGestureRecognizer gestureRecognizer = new TapGestureRecognizer();
            gestureRecognizer.Tapped += async (s, e) => await Application.Current.MainPage.Navigation.PushAsync(new GroupDescriptionPage(group));
            labelStackLayout.GestureRecognizers.Add(gestureRecognizer);

            return frame;

        }

        private static string getRandomEmoji()
        {
            string[] s = { "🗺", "🗿", "🗽", "🗼", "🏰", "🏯", "🏟", "🎡", "🎢", "🎠", "⛲️", "⛱", "🏖", "🏝", "🏜", "🌋", "⛰" };
            return s[(new Random()).Next(0, s.Length - 1)];
        }
    }
}
