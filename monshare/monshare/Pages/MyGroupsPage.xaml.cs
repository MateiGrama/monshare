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
                groupListLayout.Children.Clear();
                Groups.ForEach(async g => groupListLayout.Children.Add(await GenericViews.GroupListElement(g)));

                Utils.Utils.DisplayVisualElement(groupListLayout, true);
                Utils.Utils.DisplayVisualElement(resultsView, true);

                Utils.Utils.DisplayVisualElement(NoGroupsLabel, false);
                Utils.Utils.DisplayVisualElement(CreateGroupButton, false);
               
            }
            else
            {
                Utils.Utils.DisplayVisualElement(groupListLayout, false);
                Utils.Utils.DisplayVisualElement(resultsView, false);

                Utils.Utils.DisplayVisualElement(NoGroupsLabel, true);
                Utils.Utils.DisplayVisualElement(CreateGroupButton, true);
            }
        }


        private async void GroupTapped(object sender, SelectedItemChangedEventArgs e)
        {
            await Navigation.PushAsync(new GroupDescriptionPage(Groups[e.SelectedItemIndex]));
        }

        private async void CreateNewGroupButtonPressed(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreateGroupPage(Place.DummyPlace, new List<Group>()));
        }
    }
}