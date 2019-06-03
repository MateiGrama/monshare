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
    public partial class GroupDescriptionPage : ContentPage
    {

        private bool _canClose = false;
        private Group CurrentGroup;

        public GroupDescriptionPage(Group group)
        {
            CurrentGroup = group;
            InitializeComponent();
            GroupNumber.Text = group.GroupId.ToString();
        }

        private async void ViewChatButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ChatPage(CurrentGroup));
        }

        private void LeaveGroupClicked(object sender, EventArgs e)
        {
            ShowExitDialog();
        }

        protected override bool OnBackButtonPressed()
        {
            if (_canClose)
            {
                ShowExitDialog();
            }
            return _canClose;
        }

        private async void ShowExitDialog()
        {
            var answer = await DisplayAlert("Leave group", "Are you sure you want to leave the group?", "Yes", "No");
            if (answer)
            {
                _canClose = false;
                OnBackButtonPressed();
               if (await ServerCommunication.LeaveGroupAsync(CurrentGroup.GroupId))
                {
                    await DisplayAlert("Group Left", "You have successfully left the group", "OK");
                }
            }
        }
    }
}