using monshare.Models;
using monshare.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace monshare.Pages.GroupDescription
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class groupMembersPage : ContentPage
    {
        public groupMembersPage(int groupId)
        {
            InitializeComponent();
            DisplayGroupMembers(groupId);
        }

        private async void DisplayGroupMembers(int groupId)
        {
            List<User> members = await ServerCommunication.getGroupMembers(groupId);

            var listView = new ListView()
            {
                ItemsSource = members.Select(user => 
                    user.FirstName + " " + user.LastName + " id: " + user.UserId.ToString())
            };

            PageLayout.Children.Add(listView);
        }
    }
}