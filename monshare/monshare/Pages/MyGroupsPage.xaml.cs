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
            List<Group> groups = await ServerCommunication.GetMyGroupsAsync();
            GroupsListView.ItemsSource = groups;
        }
    }
}