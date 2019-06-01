using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace monshare.Utils
{
    class LocalStorage
    {
        private const string USERID = "USERID";
        private const string SESSIONID = "SSID";

        private const int DEFAULTUSERID = -1;

        public static async Task UpdateCredatialsAsync(string userId, string ssid)
        {
            Application.Current.Properties[USERID] = userId;
            Application.Current.Properties[SESSIONID] = userId;
            await Application.Current.SavePropertiesAsync();
        }

        public static int GetUserId()
        {
            try
            {
                return Int32.Parse(Application.Current.Properties[USERID].ToString());
            }
            catch { }

            return DEFAULTUSERID;
        }

        public static int GetSessionId()
        {
            try
            {
                return Int32.Parse(Application.Current.Properties[SESSIONID].ToString());
            }
            catch { }

            return DEFAULTUSERID;
        }
    }
}
