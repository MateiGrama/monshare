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
        public groupMembersPage(int group_id)
        {
            InitializeComponent();
            DisplayGroupMembers(group_id);
        }

        private async void DisplayGroupMembers(int groupId)
        {
            List<string> groupMembersData = new List<string>();
            (await ServerCommunication.getGroupMembers(groupId)).ForEach(user =>
            groupMembersData.Add(user.FirstName + " " + user.LastName + " id: " + user.UserId.ToString()));

            var listView = new ListView()
            {
                ItemsSource = groupMembersData
            };

            PageLayout.Children.Add(listView);
        }
    }
}