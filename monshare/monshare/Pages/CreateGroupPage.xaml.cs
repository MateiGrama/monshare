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
    public partial class CreateGroupPage : ContentPage
    {
        public CreateGroupPage()
        {
            InitializeComponent();
            timePicker.Time = DateTime.Now.TimeOfDay;
        }

        private void CreateGroupClicked(object sender, EventArgs e)
        {

        }
    }
}