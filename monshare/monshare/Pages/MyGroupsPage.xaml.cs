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
            GroupsListView.ItemsSource = Groups;
        }

        private async void GroupTapped(object sender, SelectedItemChangedEventArgs e)
        {
            await Navigation.PushAsync(new GroupDescriptionPage(Groups[e.SelectedItemIndex]));
        }
    }
}