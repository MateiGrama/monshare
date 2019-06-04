using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace monshare.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ReceivedMessageView : ViewCell
	{
		public ReceivedMessageView ()
		{
			InitializeComponent ();
		}
	}
}