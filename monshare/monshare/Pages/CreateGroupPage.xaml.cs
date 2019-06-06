using monshare.Utils;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace monshare.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateGroupPage : ContentPage
    {
        public CreateGroupPage()
        {
            InitializeComponent();
            timePicker.Time = DateTime.Now.TimeOfDay;
            Title = "➕ Create";
        }

        private async void CreateGroupClicked(object sender, EventArgs e)
        {
            string groupTitle = title.Text ?? "";
            string groupDescription = description.Text ?? "";
            string rangeString = System.Text.RegularExpressions.Regex.Replace(range.SelectedItem.ToString() ?? "", "[^0-9.]", "");

            bool APICallResult = await ServerCommunication.CreateGroupAsync(
                groupTitle,
                groupDescription,
                Int32.Parse(rangeString),
                DateTime.Parse(timePicker.Time.ToString()),
                Int32.Parse(targetNoPeople.SelectedItem.ToString() ?? "0"));

            await DisplayAlert("Create group", (APICallResult ? "" : "not ") + "successful", "OK");

            if (APICallResult)
            {
                await Navigation.PopAsync();
            }
        }
    }
}