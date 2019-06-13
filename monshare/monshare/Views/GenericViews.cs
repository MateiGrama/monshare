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
        private static string FontAwsomeName = FontAwesome.GetFontAwsomeName();

        internal static async Task<View> GroupListElement(Group group)
        {

            StackLayout detailStackLayout = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            StackLayout labelStackLayout = new StackLayout() { HorizontalOptions = LayoutOptions.FillAndExpand };
            StackLayout wrapperLayout = new StackLayout() { Orientation = StackOrientation.Horizontal };

            double distance = await group.GetDistanceInKm();

            detailStackLayout.Children.Add(new Label()
            {
                FontFamily = FontAwsomeName,
                Text = distance > 0.15 ? string.Format(FontAwesome.Walking + " {0:N2}km away", distance) : FontAwesome.StreetView + " Near you",
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                HorizontalOptions = LayoutOptions.StartAndExpand
            });

            Label detailsLabel = new Label()
            {
                FontFamily = FontAwesome.GetFontAwsomeName(),
                Text = group.MembersNumber + "/" + group.TargetNumberOfPeople + " " + FontAwesome.Group,
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                HorizontalOptions = LayoutOptions.EndAndExpand
            };

            detailStackLayout.Children.Add(detailsLabel);

            labelStackLayout.Children.Add(new Label()
            {
                FontFamily = FontAwesome.GetFontAwsomeName(),
                Text = FontAwesome.RandomGroupIcon() + " " + group.Title,
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label))
            });

            labelStackLayout.Children.Add(new Label()
            {
                Text = group.Description.Length > 30 ? group.Description.Substring(0, 30) + " ..." : group.Description,
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label))
            });
            wrapperLayout.Children.Add(labelStackLayout);

            if (!group.HasJoined)
            {
                Button joinGroupButton = GetJoinGroupButton();
                joinGroupButton.Clicked += async (s, e) => {
                    if (await ServerCommunication.JoinGroup(group.GroupId)){
                        wrapperLayout.Children.Remove(joinGroupButton);
                        group.HasJoined = true;
                        group.MembersNumber++;
                        detailsLabel.Text = group.MembersNumber + "/" + group.TargetNumberOfPeople + " " + FontAwesome.Group;
                    }
                };

                wrapperLayout.Children.Add(joinGroupButton);
            }


            StackLayout frameStackLayout = new StackLayout()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            frameStackLayout.Children.Add(wrapperLayout);
            frameStackLayout.Children.Add(detailStackLayout);

            Frame frame = new Frame()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Padding = 30,
                Content = frameStackLayout,
                BackgroundColor = Color.FromHex("f0f0f0")
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
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.EndAndExpand
            };
            StackLayout labelStackLayout = new StackLayout() { HorizontalOptions = LayoutOptions.FillAndExpand };

            double distance = await group.GetDistanceInKm();

            detailStackLayout.Children.Add(new Label()
            {
                FontFamily = FontAwesome.GetFontAwsomeName(),
                Text = distance > 0.15 ? string.Format(FontAwesome.Walking + " {0:N2}km away", distance) : FontAwesome.StreetView + " Near you",
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                HorizontalOptions = LayoutOptions.StartAndExpand
            });

            Label detailsLabel = new Label()
            {
                FontFamily = FontAwesome.GetFontAwsomeName(),
                Text = group.MembersNumber + "/" + group.TargetNumberOfPeople + " " + FontAwesome.Group,
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                HorizontalOptions = LayoutOptions.EndAndExpand
            };

            detailStackLayout.Children.Add(detailsLabel);

            StackLayout firstRowLayout = new StackLayout{
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Orientation = StackOrientation.Horizontal
            };
            firstRowLayout.Children.Add(new Label()
            {
                VerticalOptions = LayoutOptions.CenterAndExpand,
                FontFamily = FontAwesome.GetFontAwsomeName(),
                Text = FontAwesome.RandomGroupIcon() + " " + group.Title,
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
            });

            if (!group.HasJoined)
            {
                Button joinGroupButton = GetJoinGroupButton();
                joinGroupButton.HorizontalOptions = LayoutOptions.EndAndExpand;
                joinGroupButton.Padding = 0;
                joinGroupButton.Margin = 0;


                joinGroupButton.Clicked += async (s, e) =>
                {
                    if (await ServerCommunication.JoinGroup(group.GroupId))
                    {
                        firstRowLayout.Children.Remove(joinGroupButton);
                        group.HasJoined = true;
                        group.MembersNumber++;
                        detailsLabel.Text = group.MembersNumber + "/" + group.TargetNumberOfPeople + " " + FontAwesome.Group;
                    }
                };

                firstRowLayout.Children.Add(joinGroupButton);
            }
            else
            {
                firstRowLayout.Children.Add(new Label()
                {
                    FontFamily = FontAwesome.GetFontAwsomeName(),
                    VerticalOptions =LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.EndAndExpand,
                    Text = FontAwesome.Award,
                    TextColor = Color.FromHex("a53a3b"),
                    FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label))
                });

                firstRowLayout.Children.Add(new Label()
                {
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.End,
                    Text = "Already\nmember",
                    TextColor = Color.FromHex("a53a3b"),
                    FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label))
                }); ;
            }

            labelStackLayout.Children.Add(firstRowLayout);
            labelStackLayout.Children.Add(new Label()
            {
                Margin = new Thickness(0,10),
                Text = group.Description.Length > 60 ? group.Description.Substring(0, 60) + " ..." : group.Description,
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label))
            });

            StackLayout frameStackLayout = new StackLayout()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            frameStackLayout.Children.Add(labelStackLayout);
            frameStackLayout.Children.Add(detailStackLayout);
            

            Frame frame = new Frame()
            {
                CornerRadius = 15,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Padding = 20,
                HeightRequest = Application.Current.MainPage.Height * 0.25,
                WidthRequest = Application.Current.MainPage.Width * 0.65,
                Margin = new Thickness(10, 20),
                Content = frameStackLayout,
                BackgroundColor = Color.FromHex("f0f0f0")
            };

            TapGestureRecognizer gestureRecognizer = new TapGestureRecognizer();
            gestureRecognizer.Tapped += async (s, e) => await Application.Current.MainPage.Navigation.PushAsync(new GroupDescriptionPage(group));
            labelStackLayout.GestureRecognizers.Add(gestureRecognizer);

            return frame;

        }

        private static Button GetJoinGroupButton()
        {
            return new Button() {
                Text = "Join",
                TextColor = Color.AntiqueWhite,
                BackgroundColor = Color.FromHex("9e2a2b"),
                CornerRadius = 15,
                Padding = 10,
                Margin = new Thickness(0, 10),
                HorizontalOptions = LayoutOptions.Center
            };
        }
        public static Frame GetJoinGroupButton(EventHandler joinGroupButton)
        {
            Frame joinButton = new Frame()
            {
                Margin = new Thickness(5, 15, 5, 0),
                HasShadow = false,
                Padding = 10,
                BorderColor = Color.FromHex("9e2a2b"),
                CornerRadius = 5,
                Content = new Label()
                {
                    Text = "Join Group",
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    TextColor = Color.FromHex("9e2a2b"),
                    FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label))
                }
            };
            TapGestureRecognizer gestRec = new TapGestureRecognizer();
            gestRec.Tapped += joinGroupButton;
            joinButton.GestureRecognizers.Add(gestRec);
            return joinButton;
        }

        //private static string getRandomEmoji()
        //{
        //    string[] s = { "🗺", "🗿", "🗽", "🗼", "🏰", "🏯", "🏟", "🎡", "🎢", "🎠", "⛲️", "⛱", "🏖", "🏝", "🏜", "🌋", "⛰" };
        //    return s[(new Random()).Next(0, s.Length - 1)];
        //}
    }
}
