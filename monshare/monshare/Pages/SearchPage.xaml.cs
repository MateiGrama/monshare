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
        List<Group> GroupsAround = new List<Group>();
        private List<StackLayout> suggestionsLayouts = new List<StackLayout>();
        private Place selectedPlace = Place.DummyPlace;
        private bool ableToProcessInput;
        private bool freeToSuggest = true;

        public SearchPage()
        {

            InitializeComponent();
            CheckCredentials();
            intializeScrollView();

            //Title = "🔍 Find";
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            resetPageAsync();
        }

        private async void UpdateGroupCards()
        {
            List<View> groupCards = new List<View>();
            bool addCardsOneByOne = groupsAroundLayout.Children.Count == 0;

            foreach (Group group in GroupsAround)
            {
                View groupCard = await GenericViews.GroupCardList(group, this);
                groupCards.Add(groupCard);
                if (addCardsOneByOne && groupCards.Count == 5)
                {
                    groupCards.ForEach(card => groupsAroundLayout.Children.Add(card));
                    groupCards.Clear();
                }
            }

            if (!addCardsOneByOne)
            {
                groupsAroundLayout.Children.Clear();
            }
            groupCards.ForEach(card => groupsAroundLayout.Children.Add(card));
            

        }

        private void resetPageAsync()
        {

            freeToSuggest = false;
            selectedPlace = Place.DummyPlace;
            CreateGroupButtonLabel.Text = "+ Create a group";

            if (resultLayout.IsVisible)
            {
                ToggleShownGroupList();
            }

            AddTapGestureRecognizerToCreateGroupButton(CreateNewGroupTapped);
            RemoveCurrentPredictionFromRelativeLayout();
            loadGroupsAroundAsync();

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
            queryEntry.TextChanged += (s, a) => { ProcessingInput(); };

            resultLayout = new StackLayout()
            {
                Padding = 20,
                IsVisible = false,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            ScrollView resultsScrollView = new ScrollView()
            {
                Content = resultLayout
            };

            pageLayout.Children.Add(resultsScrollView, Constraint.RelativeToParent((parent) =>
            {
                return parent.X;
            }), Constraint.RelativeToView(searchBar, (Parent, sibling) =>

            {
                return sibling.Y + sibling.Height + 30;
            }), Constraint.RelativeToParent((parent) =>
            {
                return parent.Width;
            }), Constraint.RelativeToView(searchBar, (parent, sibling) =>
            {
                return 0.9 * parent.Height - (sibling.Y + sibling.Height) - 30;
            })); ;
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

            pageLayout.Children.Add(titleAndGroupsAroundLayout, Constraint.RelativeToParent((parent) =>
            {
                return parent.X;
            }), Constraint.RelativeToView(searchBar, (Parent, sibling) =>
            {
                return sibling.Y + sibling.Height + 30;
            }), Constraint.RelativeToParent((parent) =>
            {
                return parent.Width;
            }), Constraint.RelativeToView(searchBar, (parent, sibling) =>
            {
                return 0.9 * parent.Height - (sibling.Y + sibling.Height) - 30;
            }));

            AddTapGestureRecognizerToCreateGroupButton(CreateNewGroupTapped);

            pageLayout.RaiseChild(groupsAroundScrollView);

        }

        internal async void AddNearbyGroup(Group createdGroup)
        {
            GroupsAround.Add(createdGroup);
            View groupCard = await GenericViews.GroupCardList(createdGroup, this);
            groupsAroundLayout.Children.Add(groupCard);

        }

        private void AddTapGestureRecognizerToCreateGroupButton(Action<object, EventArgs> ButtonTappedHandler)
        {
            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (o, e) => ButtonTappedHandler(o, e);
            CreateGroupButton.GestureRecognizers.Clear();
            CreateGroupButton.GestureRecognizers.Add(tapGestureRecognizer);
        }

        private async void ProcessingInput()
        {
            string old = String.Copy(queryEntry.Text);
            await Task.Delay(250);

            if (queryEntry.Text != "" && !DeleteEntryButton.IsVisible)
            {
                ToggleDeleteTextEntryButton();
            }

            // Don't update predictions continuosly
            if (queryEntry.Text != "" && old == queryEntry.Text)
            {
                freeToSuggest = true;
                ableToProcessInput = false;
                LoadPredictions();
            }

            if (queryEntry.Text == "")
            {
                if (DeleteEntryButton.IsVisible)
                {
                    ToggleDeleteTextEntryButton();
                }
                RemoveCurrentPredictionFromRelativeLayout();
            }

            if (queryEntry.Text == "" && resultLayout.IsVisible)
            {
                resetPageAsync();
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

            StackLayout suggestionsLayout = new StackLayout() {
                Opacity = 0.95,
                Padding = new Thickness(10,2,10,0),
                Spacing = 1};

            RemoveCurrentPredictionFromRelativeLayout();

            if (queryEntry.Text != null && queryEntry.Text != "")
            {
                List<Place> suggestedPlaces = await FoursquarePlaces.SearchPlacesAsync(queryEntry.Text);

                foreach (Place place in suggestedPlaces)
                {
                    Frame frame = new Frame() {
                        HeightRequest = suggestionFrameHeight,
                        Padding = new Thickness(30, 0),
                        BackgroundColor = Color.FromHex("f0f0f0"),
                        HasShadow = false,
                        BorderColor = Color.Transparent,
                    };

                    string suggestion = place.Name.Trim('"');
                    frame.Content = new Label(){
                        Text = suggestion.Length > 25 ? suggestion.Substring(0, 25) + " ..." : suggestion
                    }; ;

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

            pageLayout.Children.Add(suggestionsLayout, Constraint.RelativeToParent((parent) =>
            {
                return parent.Width * 0.1;
            }), Constraint.RelativeToView(searchBar, (Parent, sibling) =>
            {
                return sibling.Y + sibling.Height;
            }), Constraint.RelativeToParent((parent) =>
            {
                return parent.Width * 0.8;
            }), Constraint.RelativeToParent((parent) =>
            {
                return suggestionsLayout.Padding.VerticalThickness + suggestionsLayout.Children.Count * suggestionFrameHeight;
            }));

            ableToProcessInput = true;

            suggestionsLayouts.Add(suggestionsLayout);
        }

        internal async void UpdateCard(Frame groupCard, Group group)
        {
            int cardIndex = groupsAroundLayout.Children.IndexOf(groupCard);
            if (cardIndex != -1)
            {
                groupsAroundLayout.Children[cardIndex] = await GenericViews.GroupCardList(group, this);
            }
        }

        private void ToggleShownGroupList()
        {
            Utils.Utils.DisplayVisualElement(resultLayout, !resultLayout.IsVisible);
            Utils.Utils.DisplayVisualElement(titleAndGroupsAroundLayout, !titleAndGroupsAroundLayout.IsVisible);

            ((ScrollView)(resultLayout.Parent)).IsEnabled = resultLayout.IsVisible;
            ((ScrollView)(groupsAroundLayout.Parent)).IsEnabled = titleAndGroupsAroundLayout.IsVisible;

            if (resultLayout.IsVisible)
            {
                pageLayout.RaiseChild((Layout)resultLayout.Parent);
            }
            else
            {
                pageLayout.RaiseChild((Layout)groupsAroundLayout.Parent);
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
            if (queryEntry.Text == null || queryEntry.Text == "")
            {
                return;
            }
            freeToSuggest = false;
            loadGroupsAsync();
        }

        private void ToggleDeleteTextEntryButton()
        {
            DeleteEntryButton.IsVisible = !DeleteEntryButton.IsVisible;
            DeleteEntryButton.IsEnabled = !DeleteEntryButton.IsEnabled;
        }

        private async void loadGroupsAsync()
        {
            toggleLoadingVisibility(true);
            List<Group> groups = await ServerCommunication.SearchGroups(queryEntry.Text, selectedPlace.Id);

            resultLayout.Children.Clear();
            groups.ForEach(async g => resultLayout.Children.Add(await GenericViews.GroupListElement(g)));
            ToggleShownGroupList();

            if (groups.Count == 0 && selectedPlace != Place.DummyPlace)
            {
                resultLayout.Children.Add(new Label()
                {
                    Text = "No results found.",
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    FontSize = 16,
                    Margin = new Thickness(0, 30, 0, 0)
                });

                resultLayout.Children.Add(new Label()
                {
                    Text = "Press the button below to create one!",
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    FontSize = 16,
                    Margin = new Thickness(0, 30, 0, 0)
                });

                CreateGroupButtonLabel.Text = "+ Create a group for this location";
                AddTapGestureRecognizerToCreateGroupButton(CreateNewGroupWithPlaceIdTapped);

            }
            else
            {
                CreateGroupButtonLabel.Text = "+ Create a group";
                AddTapGestureRecognizerToCreateGroupButton(CreateNewGroupTapped);
            }

            toggleLoadingVisibility(false);
        }

        private async void loadGroupsAroundAsync()
        {
            toggleLoadingVisibility(true);

            if (groupsAroundLayout.Children.Count == 0)
            {
                GroupsAround = await ServerCommunication.SearchGroups(queryEntry.Text, selectedPlace.Id);
            }

            UpdateGroupCards();
            toggleLoadingVisibility(false);

        }

        private void toggleLoadingVisibility(bool show)
        {
            pageLayout.RaiseChild(activityIndicator);
            activityIndicator.IsVisible = show;
        }

        private async void CreateNewGroupTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreateGroupPage(selectedPlace, GroupsAround));
        }


        private async void CreateNewGroupWithPlaceIdTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreateGroupPage(selectedPlace, GroupsAround));
        }


        ///* IMPORTED FROM GMAIN PAGE*/
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

        private void DeleteSearchBarTextButtonTapped(object sender, EventArgs e)
        {
            queryEntry.Text = "";
            resetPageAsync();
        }

        private async void PageLayoutSwiped(object sender, SwipedEventArgs e)
        {
            if (titleAndGroupsAroundLayout.IsVisible)
            {
                toggleLoadingVisibility(true);
                GroupsAround = await ServerCommunication.SearchGroups(queryEntry.Text, selectedPlace.Id);
                UpdateGroupCards();
                toggleLoadingVisibility(false);
            }
        }
    }
}