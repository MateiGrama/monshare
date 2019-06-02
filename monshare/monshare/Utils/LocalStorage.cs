using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace monshare.Utils
{
    class LocalStorage
    {
        private const string USER_ID = "USERID";
        private const string SESSION_ID = "SSID";

        private const int DEFAULT_USER_ID = -1;
        private const int DEFAULT_SESSION_ID = -1;


        public static async Task UpdateCredetialsAsync(string userId, string ssid)
        {
            Application.Current.Properties[USER_ID] = userId;
            Application.Current.Properties[SESSION_ID] = ssid;
            await Application.Current.SavePropertiesAsync();
        }

        public static int GetUserId()
        {
            try
            {
                return Int32.Parse(Application.Current.Properties[USER_ID].ToString());
            }
            catch { }

            return DEFAULT_USER_ID;
        }

        public static int GetSessionId()
        {
            try
            {
                return Int32.Parse(Application.Current.Properties[SESSION_ID].ToString());
            }
            catch { }

            return DEFAULT_SESSION_ID;
        }
    }
}