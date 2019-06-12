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
	public partial class SearchPage : ContentPage
    {
        private StackLayout resultLayout;
        private StackLayout groupsAroundLayout;
        private StackLayout titleAndGroupsAroundLayout;
        private List<StackLayout> suggestionsLayouts = new List<StackLayout>();
        private Place selectedPlace = Place.DummyPlace;
        private bool ableToProcessInput;
        private bool freeToSuggest = true;

        public SearchPage ()
		{
			InitializeComponent ();
            CheckCredentials();

            intializeScrollView();

            //Title = "🔍 Find";
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            queryEntry.TextChanged += (s,a) => { ProcessingInput(); };
            resetPageAsync();
        }

        private void resetPageAsync()
        {
            RemoveCurrentPredictionFromRelativeLayout();
            loadGroupsAroundAsync();

            resultLayout.IsVisible = false;
            titleAndGroupsAroundLayout.IsVisible = true;

            pageLayout.RaiseChild((Layout)groupsAroundLayout.Parent);
        }

        private async void CheckCredentials()
        {
            if (!await ServerCommunication.isLoggedIn())
            {
                await Navigation.PushAsync(new AuthentificationPage());
            }

        }

        private void intializeScrollView()
        {
            resultLayout = new StackLayout() { Padding = 20,
                IsVisible = false,
                HorizontalOptions = LayoutOptions.FillAndExpand };
            ScrollView resultsScrollView = new ScrollView() {
                Content = resultLayout
            };

            pageLayout.Children.Add(resultsScrollView, Constraint.RelativeToParent((parent) => {
                return parent.X;
            }), Constraint.RelativeToView(searchBar, (Parent, sibling) => {
                return sibling.Y + sibling.Height + 30;
            }), Constraint.RelativeToParent((parent) => {
                return parent.Width;
            }), Constraint.RelativeToView(searchBar, (parent, sibling) => {
                return 0.9 * parent.Height - (sibling.Y + sibling.Height) -30;
            }));;
            pageLayout.RaiseChild(CreateGroupButton);

            titleAndGroupsAroundLayout = new StackLayout()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            groupsAroundLayout = new StackLayout()
            {
                Padding = 20,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Orientation = StackOrientation.Horizontal,
            };

            ScrollView groupsAroundScrollView = new ScrollView()
            {
                Content = groupsAroundLayout,
                Orientation = ScrollOrientation.Horizontal 
            };
            titleAndGroupsAroundLayout.Children.Add(new Label()
            {
                Text = "GROUPS NEARBY",
                FontSize = 22,
                TextColor = Color.FromHex("#25151d"),
                FontAttributes = FontAttributes.Bold,
                FontFamily = "{StaticResource BlackFont}",
                Margin = new Thickness(35, 0, 0, 0)

            });

            titleAndGroupsAroundLayout.Children.Add(groupsAroundScrollView);

            pageLayout.Children.Add(titleAndGroupsAroundLayout, Constraint.RelativeToParent((parent) => {
                return parent.X;
            }), Constraint.RelativeToView(searchBar, (Parent, sibling) => {
                return sibling.Y + sibling.Height + 30;
            }), Constraint.RelativeToParent((parent) => {
                return parent.Width;
            }), Constraint.RelativeToView(searchBar, (parent, sibling) => {
                return 0.9 * parent.Height - (sibling.Y + sibling.Height) - 30;
            })); ;
            pageLayout.RaiseChild(titleAndGroupsAroundLayout);

        }

        private async void ProcessingInput()
        {
            string old = String.Copy(queryEntry.Text);
            await Task.Delay(250);
            if(old == queryEntry.Text)
            {
                freeToSuggest = true;
                ableToProcessInput = false;
                LoadPredictions();
            }
        }

        private async void LoadPredictions()
        {
            if (queryEntry.Text == null || queryEntry.Text == "" || queryEntry.Text == selectedPlace.Name.Trim('"'))
            {
                RemoveCurrentPredictionFromRelativeLayout();
                return;
            }

            const int suggestionFrameHeight = 30;
            StackLayout suggestionsLayout = new StackLayout() { Opacity = 0.9, Padding = new Thickness(10,2,10,0) , Spacing = 2 };
            RemoveCurrentPredictionFromRelativeLayout();

            if (queryEntry.Text != null && queryEntry.Text != "")
            {
                List<Place> suggestedPlaces = await FoursquarePlaces.SearchPlacesAsync(queryEntry.Text);
                foreach (Place place in suggestedPlaces)
                {
                    Frame frame = new Frame() { HeightRequest = suggestionFrameHeight, Padding = new Thickness(30, 0) };
                    frame.Content = new Label() { Text = place.Name.Trim('"') };
                    suggestionsLayout.Children.Add(frame);

                    TapGestureRecognizer gestureRecognizer = new TapGestureRecognizer();
                    gestureRecognizer.Tapped += (s, e) =>
                    {
                        selectedPlace = place;
                        queryEntry.Text = place.Name.Trim('"');
                        queryEntry.Unfocus();

                        RemoveCurrentPredictionFromRelativeLayout();
                        loadGroupsAsync();
                    };
                    frame.GestureRecognizers.Add(gestureRecognizer);

                }
            }
            if (!freeToSuggest)
            {
                return;
            }

            pageLayout.Children.Add(suggestionsLayout, Constraint.RelativeToParent((parent) => {
                return parent.X;
            }), Constraint.RelativeToView(searchBar, (Parent, sibling) => {
                return sibling.Y + sibling.Height;
            }), Constraint.RelativeToParent((parent) => {
                return parent.Width;
            }), Constraint.RelativeToParent((parent) => {
                return suggestionsLayout.Padding.VerticalThickness + suggestionsLayout.Children.Count * suggestionFrameHeight;
            }));

            ableToProcessInput = true;

            suggestionsLayouts.Add(suggestionsLayout);
        }

        private void toggleShownGroupList()
        {
            Utils.Utils.DisplayVisualElement(resultLayout, !resultLayout.IsVisible);
            Utils.Utils.DisplayVisualElement(titleAndGroupsAroundLayout, !titleAndGroupsAroundLayout.IsVisible);
            if (resultLayout.IsVisible) {
                pageLayout.RaiseChild((Layout)resultLayout.Parent);
            }
            else
            {
                pageLayout.RaiseChild((Layout)titleAndGroupsAroundLayout.Parent);
            }
            pageLayout.RaiseChild(CreateGroupButton);
            
        }

        private void RemoveCurrentPredictionFromRelativeLayout()
        {
            foreach (StackLayout s in suggestionsLayouts)
            {
                try
                {
                    pageLayout.Children.Remove(s);
                }
                catch { }
            }
            suggestionsLayouts.Clear();
        }

        private void QueryEntryCompleted(object sender, EventArgs e)
        {
            queryEntry.Unfocus();
            RemoveCurrentPredictionFromRelativeLayout();
            if (queryEntry.Text == null || queryEntry.Text == "" ) { 
                return;
            }
            freeToSuggest = false;
            loadGroupsAsync();
        }

        private async void loadGroupsAsync()
        {
            toggleLoadingVisibility(true);
            List<Group> groups= await ServerCommunication.SearchGroups(queryEntry.Text, selectedPlace.Id);
            resultLayout.Children.Clear();
            groups.ForEach(async g => resultLayout.Children.Add(await GenericViews.GroupListElement(g)));
            toggleShownGroupList();
            toggleLoadingVisibility(false);
        }

        private async void loadGroupsAroundAsync()
        {
            toggleLoadingVisibility(true);
            List<Group> groups = await ServerCommunication.SearchGroups(queryEntry.Text, selectedPlace.Id);
            groupsAroundLayout.Children.Clear();
            groups.ForEach(async g => groupsAroundLayout.Children.Add(await GenericViews.GroupCardList(g)));
            toggleLoadingVisibility(false);
        }

        private void toggleLoadingVisibility(bool show)
        {
            activityIndicator.IsVisible = show;
        }

        private async void CreateNewGroupTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreateGroupPage());
        }


        /* IMPORTED FROM GMAIN PAGE*/
        public async void LogoutButtonPressed(object sender, EventArgs args)
        {
            bool successfulCall = await ServerCommunication.logout();

            await DisplayAlert("Logout", (successfulCall ? "" : "not ") + "successful", "OK");

            if (successfulCall)
            {
                await Navigation.PushAsync(new AuthentificationPage());
            }
        }

        public async void MyGroupsButtonPressed(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new MyGroupsPage());
        }

        public async void SearchButtonPressed(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new SearchPage());
        }

        public async void MyAccountButtonPressed(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MyAccountPage());
        }

        private async void GroupsAroundPressed(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new GroupsAroundPage());
        }
    }
}