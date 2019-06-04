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

            //TODO: cache locally
            Groups = await ServerCommunication.GetMyGroupsAsync();
            if (Groups.Count > 0)
            {
                GroupsListView.ItemsSource = Groups;
                GroupsListView.HeightRequest = -1;
                GroupsListView.IsVisible = true;
            }
            else
            {

                NoGroupsLabel.IsVisible = true;
                NoGroupsLabel.HeightRequest = -1;
                CreateGroupButton.IsVisible = true;
                CreateGroupButton.HeightRequest = -1;
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