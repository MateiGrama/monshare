using monshare.Models;
using monshare.Utils;
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
    public partial class MyGroupsPage : ContentPage
    {
        private List<Group> Groups;

        public MyGroupsPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            RefreshMyGroupsAsync();
        }

        private async void RefreshMyGroupsAsync()
        {
            Groups = await ServerCommunication.GetMyGroupsAsync();
            if (Groups.Count > 0)
            {
                GroupsListView.ItemsSource = Groups;
            }
            else
            {
                Button button = new Button
                {
                    Text = "Create a new group"
                };
                button.Clicked += (s, e) => CreateNewGroupButtonPressed(s, e);
                //TODO: Fix this
                groupsStackLayout.Children.Add(new Label()
                {
                    Text = "You don't have any grups yet! Create a new group by clicking on the button below.",
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    Opacity = 0.75,
                    FontSize = 16
                });
            }
        }

        private async void GroupTapped(object sender, SelectedItemChangedEventArgs e)
        {
            await Navigation.PushAsync(new GroupDescriptionPage(Groups[e.SelectedItemIndex]));
        }

        private async void CreateNewGroupButtonPressed(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreateGroupPage());
        }
    }
}