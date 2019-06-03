using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Json;
using monshare.Models;
using Plugin.Geolocator;

namespace monshare.Utils
{
    class ServerCommunication
    {

        const string SUCCESS = "success";
        const string FAIL = "fail";
        const string BASEURL = "https://monshare.azurewebsites.net";
        const string REGISTER_API = BASEURL + "/register";
        const string LOGIN_API = BASEURL + "/login";
        const string CREATE_GROUP_API = BASEURL + "/createGroup";
        const string GET_GROUPS_AROUND_API = BASEURL + "/getGroupsAround";
        const string GET_MY_GROUPS_API = BASEURL + "/getMyGroups";
        const string GET_GROUP_CHAT_API = BASEURL + "/getGroupChat";
        const string LEAVE_GROUP_API = BASEURL + "/leaveGroup";
        const string CHECK_IS_LOGGED_IN = BASEURL + "/isLoggedIn";
        const string LOGOUT_API = BASEURL + "/logout";

        private static HttpClient client = new HttpClient();

        public static async Task<User> Login(string email, string password)
        {
            User newUser = User.NullInstance;

            string url = LOGIN_API + "?" +
                "email=" + email + "&" +
                "password_hash=" + Utils.hashPassword(password);

            JsonValue result = await GetResponse(url);

            try
            {
                if (result["status"] == SUCCESS)
                {
                    newUser = new User
                    {
                        userId = (int)result["user"]["user_id"],
                        firstName = result["user"]["first_name"],
                        lastName = result["user"]["last_name"]
                    };

                    await LocalStorage.UpdateCredetialsAsync(result["user"]["user_id"].ToString(), result["user"]["session_id"], newUser.firstName, newUser.lastName);
                }
                else if (result["status"] == FAIL)
                {
                    newUser.message = result["description"];
                }
            }

            catch
            {
                newUser = User.NullInstance;
                newUser.message = "Error happened in frontend";
            }
            return newUser;
        }

        internal static async Task<bool> logout()
        {
            string url = LOGOUT_API + "?" +
              "session_id=" + LocalStorage.GetSessionId() + "&" +
              "user_id=" + LocalStorage.GetUserId();

            JsonValue result = await GetResponse(url);

            try
            {
             return result["status"] == SUCCESS;
            }
            catch { }
            return false;
        }

        internal static async Task<Chat> getGroupChatAsync(Group group)
        {
            Chat chat = Chat.NullInstance;
            List<Message> messages = new List<Message>();
            var client = new HttpClient();

            string url = GET_GROUP_CHAT_API + "?" +
                "session_id=" + LocalStorage.GetSessionId() + "&" +
                "user_id=" + LocalStorage.GetUserId() + "&" +
                "group_id=" + group.GroupId;

            var uri = new Uri(url);
            var json = await client.GetStringAsync(uri);
            var result = JsonValue.Parse(json);

            try
            {
                if (result["status"] == SUCCESS)
                {
                    chat = new Chat {
                        group = group,
                        messages = new List<Message>()
                    };

                    foreach(JsonValue jsonMessage in result["messages"])
                    {
                        chat.messages.Add(new Message() {
                            senderId = jsonMessage["msg_sender_id"],
                            text = jsonMessage["msg"],
                            dateTime = DateTime.Parse(jsonMessage["date_time"])
                        });
                    }
                }
                else if (result["status"] == FAIL)
                {
                    chat.message = result["description"];
                }
            }
            catch
            {
                chat = Chat.NullInstance;
                chat.message = "Error happened in frontend";
            }

            return chat;
        }

        public static async Task<User> Register(string email, string firstName, string lastName, string password)
        {
            User newUser = User.NullInstance;

            string url = REGISTER_API + "?" +
                "first_name=" + firstName + "&" +
                "last_name=" + lastName + "&" +
                "email=" + email + "&" +
                "password_hash=" + Utils.hashPassword(password);

            JsonValue result = await GetResponse(url);

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

                    await LocalStorage.UpdateCredetialsAsync(result["user"]["user_id"].ToString(), result["user"]["session_id"], firstName, lastName);
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
        public static async Task<bool> CreateGroupAsync(string title, string description, int range, DateTime time, int targetNoPeople)
        {
            var position = await CrossGeolocator.Current.GetPositionAsync(TimeSpan.FromSeconds(5));

            string url = CREATE_GROUP_API + "?" +
                "user_id=" + LocalStorage.GetUserId() + "&" +
                "session_id=" + LocalStorage.GetSessionId() + "&" +
                "group_name=" + title + "&" +
                "group_description=" + description + "&" +
                "target=" + targetNoPeople + "&" +
                "lifetime=" + time.Subtract(DateTime.Now).TotalMinutes + "&" +
                "lat=" + position.Latitude + "&" +
                "long=" + position.Longitude + "&" +
                "range=" + range;

            JsonValue result = await GetResponse(url);

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

            string url = GET_MY_GROUPS_API + "?" +
                "user_id=" + LocalStorage.GetUserId() + "&" +
                "session_id=" + LocalStorage.GetSessionId();

            JsonValue result = await GetResponse(url);

            try
            {
                if (result["status"] == SUCCESS)
                {
                    foreach (JsonValue group in result["groups"])
                    {
                        Group newGroup = new Group();

                        newGroup.GroupId = group["GroupId"];
                        newGroup.Title = group["Title"];
                        newGroup.Description = group["Description"];
                        newGroup.CreationDateTime = DateTime.Parse(group["CreationDateTime"] ?? DateTime.Now.ToString());
                        newGroup.EndDateTime = DateTime.Parse(group["EndDateTime"] ?? DateTime.Now.ToString());
                        newGroup.MembersNumber = group["MembersNumber"];
                        newGroup.OwnerId = group["ownerId"];
                        newGroup.TargetNumberOfPeople = group["targetNum"] ?? 1;

                        myGroups.Add(newGroup);
                    }
                }
            }
            catch { }
            return myGroups;
        }

        public static async Task<bool> LeaveGroupAsync(int groupId)
        {
            string url = LEAVE_GROUP_API + "?" +
                "user_id=" + LocalStorage.GetUserId() + "&" +
                "session_id=" + LocalStorage.GetSessionId() + "&" +
                "group_id=" + groupId;

            JsonValue result = await GetResponse(url);

            try
            {
                return result["status"] == SUCCESS;
            }
            catch { }
            return false;
        }


        public static async Task<bool> isLoggedIn() {
            string url = CHECK_IS_LOGGED_IN + "?" +
               "user_id=" + LocalStorage.GetUserId() + "&" +
               "session_id=" + LocalStorage.GetSessionId();

            JsonValue result = await GetResponse(url);

            try
            {
                return result["status"] == SUCCESS;
            }
            catch { }
            return false;

        }

        private static async Task<JsonValue> GetResponse(string url)
        {
            var uri = new Uri(url);
            var json = await client.GetStringAsync(uri);
            var result = JsonValue.Parse(json);
            return result;
        }

    }
}
