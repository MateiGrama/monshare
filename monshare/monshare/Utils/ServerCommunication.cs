using System;
using System.Collections.Generic;
using System.Net.Http;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Json;
using monshare.Models;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Plugin.Geolocator;
using monshare.Pages;

namespace monshare.Utils
{
    class ServerCommunication
    {
        private static Page page = new Page();

        private static readonly string SUCCESS = "success";
        private static readonly string FAIL = "fail";
        private static readonly string BASEURL = "https://monshare.azurewebsites.net";
        private static readonly string REGISTER_API = BASEURL + "/register";
        private static readonly string LOGIN_API = BASEURL + "/login";
        private static readonly string LOGOUT_API = BASEURL + "/logout";
        private static readonly string CREATE_GROUP_API = BASEURL + "/createGroup";
        private static readonly string GET_GROUPS_AROUND_API = BASEURL + "/getGroups";
        private static readonly string SEARCH_GRPUPS_API = GET_GROUPS_AROUND_API;
        private static readonly string GET_GROUP_CHAT_API = BASEURL + "/getGroupChat";
        private static readonly string GET_MY_GROUPS_API = BASEURL + "/getMyGroups";
        private static readonly string GET_GROUP_MEMBERS_API = BASEURL + "/getGroupMembers";
        private static readonly string LEAVE_GROUP_API = BASEURL + "/leaveGroup";
        private static readonly string DELETE_ACCOUNT_API = BASEURL + "/deleteAccount";
        private static readonly string JOIN_GROUP_API = BASEURL + "/joinGroup";
        private static readonly string DELETE_GROUP_API = BASEURL + "/deleteGroup";
        private static readonly string CHECK_IS_LOGGED_IN = BASEURL + "/isLoggedIn";
        private static readonly string SEND_MESSAGE_API = BASEURL + "/sendMessage";
        private static readonly string UPDATE_GROUP_GROUP_API = BASEURL + "/updateGroup";

        private static async Task<string> GetUserIDSeesionIdLocationAPICallParams()
        {
            var pos = await Utils.GetLocationAfterCheckingPermisionsAsync();

            return "user_id=" + LocalStorage.GetUserId() + "&" +
                   "session_id=" + LocalStorage.GetSessionId() + "&" +
                   "lat=" + pos.Latitude + "&" +
                   "long=" + pos.Longitude;
        }

        public static async Task<User> Login(string email, string password)
        {
            User newUser = User.NullInstance;

            string url = LOGIN_API + "?" +
                "email=" + email + "&" +
                "password_hash=" + Utils.HashPassword(password);

            JsonValue result = await GetResponse(url);

            try
            {
                if (result["status"] == SUCCESS)
                {
                    newUser = new User
                    {
                        UserId = (int)result["user"]["user_id"],
                        FirstName = result["user"]["first_name"],
                        LastName = result["user"]["last_name"]
                    };

                    await LocalStorage.UpdateCredetialsAsync(result["user"]["user_id"].ToString(), result["user"]["session_id"], newUser.FirstName, newUser.LastName);
                }
                else if (result["status"] == FAIL)
                {
                    newUser.message = result["description"];
                }
            }

            catch (Exception e)
            {
                await page.DisplayAlert("Database Error", "An error involving our database occurred. Please try again later.", "Ok");
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

        internal static async Task<bool> UpdateGroup(Group group)
        {
            string url = UPDATE_GROUP_GROUP_API + "?" +
                "session_id=" + LocalStorage.GetSessionId() + "&" +
                "user_id=" + LocalStorage.GetUserId() + "&" +
                "group_id=" + group.GroupId + "&" +
                "group_name=" + group.Title + "&" +
                "group_description=" + group.Description + "&" +
                "target=" + group.TargetNumberOfPeople.ToString();

            JsonValue result = await GetResponse(url);

            try
            {
                return result["status"] == SUCCESS;
            }
            catch { }
            return false;
        }

        internal static async Task<List<User>> getGroupMembers(int groupId)
        {
            String url = GET_GROUP_MEMBERS_API + "?" +
            "user_id=" + LocalStorage.GetUserId() + "&" +
            "session_id=" + LocalStorage.GetSessionId() + "&" +
            "group_id=" + groupId.ToString();

            JsonValue result = await GetResponse(url);

            List<User> members = new List<User>();

            try
            {
                if (result["status"] == SUCCESS)
                {
                    foreach (JsonValue user in result["groups"])
                    {
                        members.Add(new User()
                        {
                            UserId = user["UserId"],
                            FirstName = user["FirstName"],
                            LastName = user["LastName"]
                        });
                    }
                }
            }
            catch
            {
            }

            return members;
        }

        internal static async Task<Chat> GetGroupChatAsync(Group group)
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
                    chat = new Chat
                    {
                        group = group,
                        messages = new List<Message>()
                    };

                    foreach (JsonValue jsonMessage in result["messages"])
                    {
                        chat.messages.Add(new Message()
                        {
                            SenderId = jsonMessage["msg_sender_id"],
                            Text = jsonMessage["msg"],
                            DateTime = DateTime.Parse(jsonMessage["date_time"])
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
                "password_hash=" + Utils.HashPassword(password);

            JsonValue result = await GetResponse(url);

            try
            {
                if (result["status"] == SUCCESS)
                {
                    newUser = new User
                    {
                        UserId = (int)result["user"]["user_id"],
                        Email = email,
                        FirstName = result["user"]["first_name"],
                        LastName = result["user"]["last_name"]
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
                await page.DisplayAlert("Database Error", "An error involving our database occurred. Please try again later.", "Ok");
                newUser.message = "An error involving our database occurred. Please try again later.";
            }
            return newUser;
        }

        public static async Task<bool> CreateGroupAsync(string title, string description, int range, DateTime time, int targetNoPeople, string placeId)
        {
            var position = await Utils.GetLocationAfterCheckingPermisionsAsync();

            if (position == null)
            {
                return false;
            }

            string url = CREATE_GROUP_API + "?" +
                "user_id=" + LocalStorage.GetUserId() + "&" +
                "session_id=" + LocalStorage.GetSessionId() + "&" +
                "group_name=" + title + "&" +
                "group_description=" + description + "&" +
                "target=" + targetNoPeople + "&" +
                "lifetime=" + time.Subtract(DateTime.Now).TotalMinutes + "&" +
                "lat=" + position.Latitude + "&" +
                "long=" + position.Longitude + "&" +
                "range=" + range + "&" +
                "place_id=" + placeId;

            JsonValue result = await GetResponse(url);

            try
            {
                return result["status"] == SUCCESS;
            }
            catch (Exception e)
            {
                await page.DisplayAlert("Database Error", "An error involving our database occurred. Please try again later.", "Ok");
            }
            return false;
        }

        public static async Task<List<Group>> GetMyGroupsAsync()
        {
            var position = await Utils.GetLocationAfterCheckingPermisionsAsync();

            if (position == null)
            {
                return new List<Group>();
            }
            string url = GET_MY_GROUPS_API + "?" +
                "user_id=" + LocalStorage.GetUserId() + "&" +
                "session_id=" + LocalStorage.GetSessionId();

            return await RequestGroupList(url);
        }

        public static async Task<List<Group>> GetGroupsAround()
        {
            var position = await Utils.GetLocationAfterCheckingPermisionsAsync();

            if (position == null)
            {
                return new List<Group>();
            }
            string url = GET_GROUPS_AROUND_API + "?" + await GetUserIDSeesionIdLocationAPICallParams();

            return await RequestGroupList(url);
        }

        public static async Task<List<Group>> SearchGroups(string query, string placeId)
        {
            var position = await Utils.GetLocationAfterCheckingPermisionsAsync();

            if (position == null)
            {
                return new List<Group>();
            }
            string url = SEARCH_GRPUPS_API + "?" + await GetUserIDSeesionIdLocationAPICallParams() + "&"
                                   + "query=" + query + "&"
                                   + "place_id=" + placeId;

            return await RequestGroupList(url);
        }

        private static async Task<List<Group>> RequestGroupList(string url)
        {
            JsonValue result = await GetResponse(url);

            List<Group> myGroups = new List<Group>();
            try
            {
                if (result["status"] == SUCCESS)
                {
                    myGroups = ExtrctGroupListFromJson(result["groups"]);
                }
            }
            catch (Exception e)
            {
                await page.DisplayAlert("Database Error", "An error involving our database occurred. Please try again later.", "Ok");
            }
            return myGroups;
        }

        private static List<Group> ExtrctGroupListFromJson(JsonValue groups)
        {
            Group newGroup;
            List<Group> result = new List<Group>();

            foreach (JsonValue group in groups)
            {
                newGroup = new Group();

                newGroup.GroupId = group["GroupId"];
                newGroup.Title = group["Title"];
                newGroup.Description = group["Description"];
                newGroup.CreationDateTime = DateTime.Parse(group["CreationDateTime"] ?? DateTime.Now.ToString());
                newGroup.EndDateTime = DateTime.Parse(group["EndDateTime"] ?? DateTime.Now.ToString());
                newGroup.MembersNumber = group["MembersNumber"] ?? 1;
                newGroup.OwnerId = group["ownerId"];
                newGroup.TargetNumberOfPeople = group["targetNum"] ?? 1;
                newGroup.Latitude = group["lat"] ?? 0;
                newGroup.Longitude = group["long"] ?? 0;
                newGroup.HasJoined = true;
                try
                {
                    newGroup.HasJoined = group["isMember"];
                }
                catch { }

                result.Add(newGroup);
            }
            return result;
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
            catch (Exception e)
            {
                await page.DisplayAlert("Database Error", "An error involving our database occurred. Please try again later.", "Ok");
            }
            return false;
        }

        public static async Task<bool> DeleteGroup(int groupId)
        {
            String url = DELETE_GROUP_API + "?" +
                "user_id=" + LocalStorage.GetUserId() + "&" +
                "session_id=" + LocalStorage.GetSessionId() + "&" +
                "group_id=" + groupId;

            JsonValue result = await GetResponse(url);

            try
            {
                //TODO: Update local cache to remove the group identified by @param groupId from MyGroups list
                return result["status"] == SUCCESS;
            }
            catch (Exception e)
            {
                await page.DisplayAlert("Database Error", "An error involving our database occurred. Please try again later.", "Ok");
            }
            return false;
        }

        public static async Task<bool> JoinGroup(int groupId)
        {
            return await JoinGroup(groupId, LocalStorage.GetUserId(), LocalStorage.GetSessionId());
        }

        public static async Task<bool> JoinGroup(int groupId, int userId, int sessionId)
        {
            String url = JOIN_GROUP_API + "?" +
               "user_id=" + userId + "&" +
               "session_id=" + sessionId + "&" +
               "group_id=" + groupId;

            JsonValue result = await GetResponse(url);

            try
            {
                return result["status"] == SUCCESS;
            }
            catch (Exception e)
            {
                await page.DisplayAlert("Database Error", "An error involving our database occurred. Please try again later.", "Ok");
            }
            return false;
        }

        public static async Task<bool> isLoggedIn()
        {
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

        internal static async Task<bool> DeleteAccount()
        {
            string url = DELETE_ACCOUNT_API + "?" +
                "user_id=" + LocalStorage.GetUserId() + "&" +
                "session_id=" + LocalStorage.GetSessionId();

            JsonValue result = await GetResponse(url);

            try
            {
                return result["status"] == SUCCESS;
            }
            catch (Exception e)
            {
                await page.DisplayAlert("Database Error", "An error involving our database occurred. Please try again later.", "Ok");
            }
            return false;
        }

        public static async Task<bool> MergeGroups(Group parentGroup, Group requester)
        {
            int targetNumberOfPeople = Math.Max(parentGroup.MembersNumber + requester.MembersNumber,
                Math.Max(parentGroup.TargetNumberOfPeople, requester.TargetNumberOfPeople));

            parentGroup.TargetNumberOfPeople = targetNumberOfPeople;
            if (!await UpdateGroup(parentGroup))
            {
                return false;
            }

            // Add all members of the requester to the parent group
            foreach (User member in await getGroupMembers(requester.GroupId))
            {
                if (!await JoinGroup(parentGroup.GroupId, member.UserId, LocalStorage.GetSessionId()))
                {
                    return false;
                }
            }

            //Delete the requester's group
            return await DeleteGroup(requester.GroupId);
        }

        internal static async Task<bool> sendMessage(String messageToBeSent, int groupId)
        {
            string url = SEND_MESSAGE_API + "?" +
                "user_id=" + LocalStorage.GetUserId() + "&" +
                "session_id=" + LocalStorage.GetSessionId() + "&" +
                "group_id=" + groupId.ToString() + "&" +
                "message=" + messageToBeSent;

            JsonValue result = await GetResponse(url);

            try
            {
                return result["status"] == SUCCESS;
            }
            catch (Exception e)
            {
                await page.DisplayAlert("Database Error", "An error involving our database occurred. Please try again.", "Ok");
            }
            return false;
        }


        internal static async Task<JsonValue> GetResponse(string url)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(url);
            var json = await client.GetStringAsync(uri);
            var result = JsonValue.Parse(json);
            return result;
        }

    }
}
