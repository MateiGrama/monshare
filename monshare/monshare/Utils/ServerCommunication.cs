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
        const int SESSION_ID = 1;
        const int USER_ID = 4;
        

        const string SUCCESS = "success";
        const string FAIL = "fail";
        const string BASEURL = "https://monshare.azurewebsites.net";
        const string REGISTER_API = BASEURL + "/register";
        const string CREATE_GROUP_API = BASEURL + "/createGroup";
        const string GET_GROUPS_AROUND_API = BASEURL + "/getGroupsAround";
        const string GET_MY_GROUPS_API = GET_GROUPS_AROUND_API;

        public static async Task<User> Register(string email, string firstName, string lastName, string password)
        {
            User newUser = User.NullInstance;

            var client = new HttpClient();
            string url = REGISTER_API + "?" +
                "first_name=" + firstName + "&" +
                "last_name="  + lastName  + "&" +
                "email="      + email     + "&" +
                "password_hash =" + Utils.hashPassword(password);

            var uri = new Uri(url);
            var json = await client.GetStringAsync(uri);
            var result = JsonValue.Parse(json);

            try
            {
                if(result["status"] == SUCCESS)
                {
                    newUser = new User
                    {
                        userId    = result["user_id"],
                        email = result["email"],
                        firstName = result["first_name"],
                        lastName  = result["last_name"]
                    };

                    await LocalStorage.UpdateCredatialsAsync(result["user_id"], result["session_id"]);
                }
                else if (result["status"] == FAIL)
                {
                    newUser.message = result["description"];
                }
            }

            catch {
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
                if ( result["status"] == SUCCESS)
                {
                    foreach(JsonValue group in result["groups"])
                    {
                        Group newGroup = new Group();
                        newGroup.groupId = group["GroupId"];
                        newGroup.title = group["Title"];
                        newGroup.description = group["Description"];
                        newGroup.creationDateTime = DateTime.Parse(group["CreationDateTime"] ?? DateTime.Now.ToString());
                        newGroup.endDateTime = DateTime.Parse(group["EndDateTime"] ?? DateTime.Now.ToString());
                        newGroup.minMembers = group["MinMembers"] ?? -1;
                        newGroup.maxMembers = group["MaxMembers"] ?? -1;
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
