using monshare.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using monshare.Pages;
using System.Threading.Tasks;

namespace monshare.Views
{
    class GenericViews
    {
        internal static View GroupListElement(Group group)
        {

            StackLayout stackLayout = new StackLayout();

            stackLayout.Children.Add(new Label()
            {
                Text = "👋 " + group.Title,
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label))
            });

            stackLayout.Children.Add(new Label()
            {
                Text = group.Description,
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label))
            });

            Frame frame = new Frame()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Padding = 30,
                Content = stackLayout,
            };

            TapGestureRecognizer gestureRecognizer = new TapGestureRecognizer();
            gestureRecognizer.Tapped += async (s, e) => await Application.Current.MainPage.Navigation.PushAsync(new GroupDescriptionPage(group));
            frame.GestureRecognizers.Add(gestureRecognizer);

            return frame;
            
        }
    }
}
