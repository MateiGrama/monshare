using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace monshare.Utils
{
    class Utils
    {
        public static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string hashPassword(string password)
        {

            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(password))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        public static bool IsValidEmail(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
 
        public static async void ShowError(string msg, Label errorLabel, int height, int duration)
        {
            errorLabel.Text = msg;
            errorLabel.HeightRequest = height;
            errorLabel.IsVisible = true;
            await Task.Delay(duration);
            errorLabel.Text = "";
            errorLabel.IsVisible = true;
            errorLabel.HeightRequest = 0;
        }
 
    }
}
