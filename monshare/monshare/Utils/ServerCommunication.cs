using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Json;
using monshare.Models;

namespace monshare.Utils
{
    class ServerCommunication
    {
        //TODO:
        public static int SESSION_ID { get; internal set; }
        public static int USER_ID { get; internal set; }

        const string SUCCESS = "success";
        const string FAIL = "fail";
        const string BASEURL = "https://monshare.azurewebsites.net";
        const string REGISTER_API = BASEURL + "/register";
        const string LOGIN_API = BASEURL + "/login";
        const string CREATE_GROUP_API = BASEURL + "/createGroup";
        const string GET_GROUPS_AROUND_API = BASEURL + "/getGroupsAround";
        const string GET_MY_GROUPS_API = BASEURL + "/getMyGroups";

        public static async Task<User> Login(string email, string password)
        {
            User newUser = User.NullInstance;

            var client = new HttpClient();
            string url = LOGIN_API + "?" +
                "email=" + email + "&" +
                "password_hash=" + Utils.hashPassword(password);

            var uri = new Uri(url);
            var json = await client.GetStringAsync(uri);
            var result = JsonValue.Parse(json);

            try
            {
                if (result["status"] == SUCCESS)
                {
                    newUser = new User();

                    newUser.userId = (int)result["user"]["user_id"];
                    newUser.firstName = result["user"]["first_name"];
                    newUser.lastName = result["user"]["last_name"];

                    USER_ID = newUser.userId;
                    SESSION_ID = (int)result["user"]["session_id"];

                    await LocalStorage.UpdateCredatialsAsync(result["user"]["user_id"], result["user"]["session_id"]);
                }
                else if (result["status"] == FAIL)
                {
                    newUser.message = result["description"];
                }
            }

            catch
            {
                newUser.message = "Error happened in frontend";
            }
            return newUser;
        }
        public static async Task<User> Register(string email, string firstName, string lastName, string password)
        {
            User newUser = User.NullInstance;

            var client = new HttpClient();
            string url = REGISTER_API + "?" +
                "first_name=" + firstName + "&" +
                "last_name=" + lastName + "&" +
                "email=" + email + "&" +
                "password_hash=" + Utils.hashPassword(password);

            var uri = new Uri(url);
            var json = await client.GetStringAsync(uri);
            var result = JsonValue.Parse(json);

            try
            {
                if (result["status"] == SUCCESS)
                {
                    newUser = new User
                    {
                        userId = (int)result["user"]["user_id"],
                        email = email,
                        firstName = result["user"]["first_name"],
                        lastName = result["user"]["last_name"]
                    };

                    await LocalStorage.UpdateCredatialsAsync(result["user"]["user_id"], result["user"]["session_id"]);
                }
                else if (result["status"] == FAIL)
                {
                    newUser.message = result["description"];
                }
            }

            catch
            {
                newUser.message = "Error happened in frontend";
            }
            return newUser;
        }
        public static async Task<bool> CreateGroupAsync(string title, string description, double range, DateTime time, int targetNoPeople)
        {
            var client = new HttpClient();
            string url = CREATE_GROUP_API + "?" +
                "user_id=" + USER_ID + "&" +
                "session_id=" + SESSION_ID + "&" +
                "group_name=" + title + "&" +
                "group_description=" + description;

            var uri = new Uri(url);
            var json = await client.GetStringAsync(uri);
            var result = JsonValue.Parse(json);

            try
            {
                return result["status"] == SUCCESS;
            }
            catch { }
            return false;
        }
        public static async Task<List<Group>> GetMyGroupsAsync()
        {
            List<Group> myGroups = new List<Group>();

            var client = new HttpClient();
            string url = GET_MY_GROUPS_API + "?" +
                "user_id=" + USER_ID + "&" +
                "session_id=" + SESSION_ID;

            var uri = new Uri(url);
            var json = await client.GetStringAsync(uri);
            var result = JsonValue.Parse(json);

            try
            {
                if (result["status"] == SUCCESS)
                {
                    foreach (JsonValue group in result["groups"])
                    {
                        Group newGroup = new Group();
                        newGroup.groupId = group["GroupId"];
                        newGroup.title = group["Title"];
                        newGroup.description = group["Description"];
                        newGroup.creationDateTime = DateTime.Parse(group["CreationDateTime"] ?? DateTime.Now.ToString());
                        newGroup.endDateTime = DateTime.Parse(group["EndDateTime"] ?? DateTime.Now.ToString());
                        newGroup.membersNumber = group["MembersNumber"] ?? -1;
                        newGroup.ownerId = group["ownerId"];
                        myGroups.Add(newGroup);
                    }
                }
            }
            catch { }
            return myGroups;
        }

    }
}
