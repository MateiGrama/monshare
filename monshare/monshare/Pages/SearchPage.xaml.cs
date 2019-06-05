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
	public partial class SearchPage : ContentPage
	{
        private bool waitingForInput;

        public SearchPage ()
		{
			InitializeComponent ();
		}

        protected override void OnAppearing()
        {
            base.OnAppearing();

            queryEntry.TextChanged += (s,a) => { processingInput(); };
        }

        private async void processingInput()
        {
            string old = String.Copy(queryEntry.Text);
            await Task.Delay(250);
            if(old == queryEntry.Text)
            {
                loadPredictions();
            }

        }

        private async void loadPredictions()
        {
            if (queryEntry.Text == null || queryEntry.Text == "")
            {
                removeCurrentPredictionFromRelativeLayout();
                return;
            }

            int suggestionFrameHeight = 30;
            StackLayout suggestionsLayout = new StackLayout() { Opacity = 0.9, Padding = new Thickness(10,2,10,0) , Spacing = 2 };

            if (queryEntry.Text != null && queryEntry.Text != "")
            {
                List<Place> suggestedPlaces = await FoursquarePlaces.SearchPlacesAsync(queryEntry.Text);
                foreach (Place place in suggestedPlaces)
                {
                    Frame frame = new Frame() { HeightRequest = suggestionFrameHeight, Padding = new Thickness(30, 0) };
                    frame.Content = new Label() { Text = place.Name.Trim('"') };
                    suggestionsLayout.Children.Add(frame);
                }
            }

            removeCurrentPredictionFromRelativeLayout();
            pageLayout.Children.Add(suggestionsLayout, Constraint.RelativeToParent((parent) => {
                return parent.X;
            }), Constraint.RelativeToView(searchBar, (Parent, sibling) => {
                return sibling.Y + sibling.Height;
            }), Constraint.RelativeToParent((parent) => {
                return parent.Width;
            }), Constraint.RelativeToParent((parent) => {
                return suggestionsLayout.Padding.VerticalThickness + suggestionsLayout.Children.Count * suggestionFrameHeight;
            }));


        }

        private void removeCurrentPredictionFromRelativeLayout()
        {
            if (pageLayout.Children[pageLayout.Children.Count - 1] != resultLayout)
            {
                pageLayout.Children.Remove(pageLayout.Children[pageLayout.Children.Count - 1]);
            }
        }
    }
}