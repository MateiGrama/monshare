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
        private bool ableToProcessInput;
        private Place selectedPlace = Place.DummyPlace;

        public SearchPage ()
		{
			InitializeComponent ();
		}

        protected override void OnAppearing()
        {
            base.OnAppearing();

            queryEntry.TextChanged += (s,a) => { ProcessingInput(); };
        }

        private async void ProcessingInput()
        {
            string old = String.Copy(queryEntry.Text);
            await Task.Delay(250);
            if(old == queryEntry.Text)
            {
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

            RemoveCurrentPredictionFromRelativeLayout();
            pageLayout.Children.Add(suggestionsLayout, Constraint.RelativeToParent((parent) => {
                return parent.X;
            }), Constraint.RelativeToView(searchBar, (Parent, sibling) => {
                return sibling.Y + sibling.Height;
            }), Constraint.RelativeToParent((parent) => {
                return parent.Width;
            }), Constraint.RelativeToParent((parent) => {
                return suggestionsLayout.Padding.VerticalThickness + suggestionsLayout.Children.Count * suggestionFrameHeight;
            }));
            ableToProcessInput =  false;
        }

        private void RemoveCurrentPredictionFromRelativeLayout()
        {
            if (pageLayout.Children[pageLayout.Children.Count - 1] != resultsView)
            {
                pageLayout.Children.Remove(pageLayout.Children[pageLayout.Children.Count - 1]);
            }
        }

        private void QueryEntryCompleted(object sender, EventArgs e)
        {
            if (queryEntry.Text == null || queryEntry.Text == "" ) { 
                return;
            }
            loadGroupsAsync();
        }

        private async void loadGroupsAsync()
        {
            toggleLoadingVisibility(true);
            List<Group> groups= await ServerCommunication.SearchGroups(queryEntry.Text, selectedPlace.Id);
            resultLayout.Children.Clear();
            groups.ForEach(g => resultLayout.Children.Add(GenericViews.GroupListElement(g)));
            toggleLoadingVisibility(false);
        }

        private void toggleLoadingVisibility(bool show)
        {
            activityIndicator.IsVisible = show;
        }
    }
}