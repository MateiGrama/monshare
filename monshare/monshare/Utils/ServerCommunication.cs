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

        const string SUCCESS = "success";
        const string FAIL = "fail";
        const string BASEURL = "https://monshare.azurewebsites.net";
        const string REGISTER_API = BASEURL + "/register";
        const string LOGIN_API = BASEURL + "/login";
        const string CREATE_GROUP_API = BASEURL + "/createGroup";
        const string GET_GROUPS_AROUND_API = BASEURL + "/getGroupsAround";
        const string GET_MY_GROUPS_API = BASEURL + "/getMyGroups";
        const string GET_GROUP_CHAT_API = BASEURL + "/getGroupChat";

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
                    newUser = new User
                    {
                        userId = (int)result["user"]["user_id"],
                        firstName = result["user"]["first_name"],
                        lastName = result["user"]["last_name"]
                    };

                    await LocalStorage.UpdateCredetialsAsync(result["user"]["user_id"].ToString(), result["user"]["session_id"].ToString());
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

        internal static async Task<Chat> getGroupChatAsync(Group group)
        {
            Chat chat = Chat.NullInstance;
            List<Message> messages = new List<Message>();
            var client = new HttpClient();

            string url = GET_GROUP_CHAT_API + "?" +
                "session_id=" + LocalStorage.GetSessionId() + "&" +
                "user_id=" + LocalStorage.GetUserId() + "&" +
                "group_id=" + group.groupId;

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
                            dateTime = jsonMessage["date_time"]
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

                    await LocalStorage.UpdateCredetialsAsync(result["user"]["user_id"].ToString(), result["user"]["session_id"].ToString());
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
                "user_id=" + LocalStorage.GetUserId() + "&" +
                "session_id=" + LocalStorage.GetSessionId() + "&" +
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
                "user_id=" + LocalStorage.GetUserId() + "&" +
                "session_id=" + LocalStorage.GetSessionId();

            var uri = new Uri(url);
            var json = await client.GetStringAsync(uri);
            var result = JsonValue.Parse(json);

            try
            {
                if (result["status"] == SUCCESS)
                {
                    foreach (JsonValue group in result["groups"])
                    {
                        Group newGroup = new Group
                        {
                            groupId = group["GroupId"],
                            title = group["Title"],
                            description = group["Description"],
                            creationDateTime = DateTime.Parse(group["CreationDateTime"] ?? DateTime.Now.ToString()),
                            endDateTime = DateTime.Parse(group["EndDateTime"] ?? DateTime.Now.ToString()),
                            membersNumber = group["MembersNumber"] ?? -1,
                            ownerId = group["ownerId"]
                        };
                        myGroups.Add(newGroup);
                    }
                }
            }
            catch { }
            return myGroups;
        }

    }
}
