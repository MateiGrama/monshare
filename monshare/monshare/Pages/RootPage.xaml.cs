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
    public partial class RootPage : TabbedPage
    {
        public RootPage ()
        {
            InitializeComponent();

            
            Children.Add(new SearchPage());
            Children.Add(new GroupsAroundPage());
            Children.Add(new CreateGroupPage());
        }
    }
}