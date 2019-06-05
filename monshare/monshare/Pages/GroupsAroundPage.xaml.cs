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
    public partial class GroupsAroundPage : ContentPage
    {
        public GroupsAroundPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            loadGroupsAsync();

        }

        private async void loadGroupsAsync()
        {
            List<Group> groups = await ServerCommunication.GetMyGroupsAsync();
            resultLayout.Children.Clear();
            groups.ForEach(g => resultLayout.Children.Add(GenericViews.GroupListElement(g)));
            
        }
    }
}