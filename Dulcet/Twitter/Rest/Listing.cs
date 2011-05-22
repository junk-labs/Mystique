using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dulcet.Twitter.Credential;
using Dulcet.Util;

namespace Dulcet.Twitter.Rest
{
    public static partial class Api
    {
        /// <summary>
        /// Get list statuses
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userScreenName">list owner user id</param>
        /// <param name="listId">list id</param>
        public static IEnumerable<TwitterStatus> GetListStatuses(this CredentialProvider provider, string userScreenName, string listId)
        {
            return provider.GetListStatuses(userScreenName, listId, null, null, null, null);
        }

        /// <summary>
        /// Get list statuses with full params
        /// </summary>
        public static IEnumerable<TwitterStatus> GetListStatuses(this CredentialProvider provider, string userScreenName, string listId, string sinceId = null, string maxId = null, long? perPage = null, long? page = null)
        {
            listId = listId.Replace("_", "-");
            var partialUri = userScreenName + "/lists/" + listId + "/statuses.xml";

            List<KeyValuePair<string, string>> para = new List<KeyValuePair<string, string>>();

            if (!String.IsNullOrEmpty(sinceId))
                para.Add(new KeyValuePair<string, string>("since_id", sinceId.ToString()));

            if (!String.IsNullOrEmpty(maxId))
                para.Add(new KeyValuePair<string, string>("max_id", maxId.ToString()));

            if (perPage != null)
                para.Add(new KeyValuePair<string, string>("per_page", perPage.ToString()));

            if (page != null)
                para.Add(new KeyValuePair<string, string>("page", page.ToString()));

            return provider.GetTimeline(partialUri, para);
        }

        /// <summary>
        /// Get list members
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userScreenName">list owner user's id</param>
        /// <param name="listId">list id</param>
        public static IEnumerable<TwitterUser> GetListMembersAll(this CredentialProvider provider, string userScreenName, string listId)
        {
            listId = listId.Replace("_", "-");
            return provider.GetUsersAll(userScreenName + "/" + listId + "/members.xml", null, null);
        }

        /// <summary>
        /// Get list members with full params
        /// </summary>
        public static IEnumerable<TwitterUser> GetListMembers(this CredentialProvider provider, string userScreenName, string listId, long? cursor, out long prevCursor, out long nextCursor)
        {
            if (cursor == null)
                cursor = -1;
            listId = listId.Replace("_", "-");
            return provider.GetUsers(userScreenName + "/" + listId + "/members.xml", null, null, cursor, out prevCursor, out nextCursor);
        }

        /// <summary>
        /// Get list subscribers with full params
        /// </summary>
        public static IEnumerable<TwitterUser> GetListSubscribers(this CredentialProvider provider, string userScreenName, string listId, long? cursor, out long prevCursor, out long nextCursor)
        {
            if (cursor == null)
                cursor = -1;
            listId = listId.Replace("_", "-");
            return provider.GetUsers(userScreenName + "/" + listId + "/subscribers.xml", null, null, cursor, out prevCursor, out nextCursor);
        }

        /// <summary>
        /// Get list subscribers
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userScreenName">list owner user's id</param>
        /// <param name="listId">list id</param>
        public static IEnumerable<TwitterUser> GetListSubscribersAll(this CredentialProvider provider, string userScreenName, string listId)
        {
            listId = listId.Replace("_", "-");
            return provider.GetUsersAll(userScreenName + "/" + listId + "/subscribers.xml", null, null);
        }

        /// <summary>
        /// Get list with full params
        /// </summary>
        private static IEnumerable<TwitterList> GetLists(this CredentialProvider provider, string partialUri, long? cursor, out long prevCursor, out long nextCursor)
        {
            List<KeyValuePair<string, string>> para = new List<KeyValuePair<string, string>>();
            if (cursor != null)
            {
                para.Add(new KeyValuePair<string, string>("cursor", cursor.ToString()));
            }

            prevCursor = 0;
            nextCursor = 0;

            var doc = provider.RequestAPIv1(partialUri, CredentialProvider.RequestMethod.GET, para);
            if (doc == null)
                return null;
            var ll = doc.Element("lists_list");
            if (ll != null)
            {
                var nc = ll.Element("next_cursor");
                if (nc != null)
                    nextCursor = (long)nc.ParseLong();
                var pc = ll.Element("previous_cursor");
                if (pc != null)
                    prevCursor = (long)pc.ParseLong();
            }


            return from n in doc.Descendants("list")
                   let l = TwitterList.FromNode(n)
                   where l != null
                   select l;
        }

        /// <summary>
        /// Get list enumerations all
        /// </summary>
        private static IEnumerable<TwitterList> GetListsAll(this CredentialProvider provider, string partialUri)
        {
            long n_cursor = -1;
            long c_cursor = -1;
            long p;
            while (n_cursor != 0)
            {
                var lists = provider.GetLists(partialUri, c_cursor, out p, out n_cursor);
                if (lists != null)
                    foreach (var u in lists)
                        yield return u;
                c_cursor = n_cursor;
            }
        }

        /// <summary>
        /// Get lists you following
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userScreenName">target user id</param>
        public static IEnumerable<TwitterList> GetFollowingListsAll(this CredentialProvider provider, string userScreenName)
        {
            foreach (var l in provider.GetUserListsAll(userScreenName))
                yield return l;
            foreach (var l in provider.GetSubscribedListsAll(userScreenName))
                yield return l;
        }

        /// <summary>
        /// Get lists someone created with full params
        /// </summary>
        public static IEnumerable<TwitterList> GetUserLists(this CredentialProvider provider, string userScreenName, long? cursor, out long prevCursor, out long nextCursor)
        {
            var partialUri = userScreenName + "/lists.xml";
            return provider.GetLists(partialUri, cursor, out prevCursor, out nextCursor);
        }

        /// <summary>
        /// Get all lists someone created
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userScreenName">target user id</param>
        public static IEnumerable<TwitterList> GetUserListsAll(this CredentialProvider provider, string userScreenName)
        {
            var partialUri = userScreenName + "/lists.xml";
            return provider.GetListsAll(partialUri);
        }

        /// <summary>
        /// Get lists which member contains someone with full params
        /// </summary>
        public static IEnumerable<TwitterList> GetMembershipLists(this CredentialProvider provider, string userScreenName, long? cursor, out long prevCursor, out long nextCursor)
        {
            var partialUri = userScreenName + "/lists/memberships.xml";
            return provider.GetLists(partialUri, cursor, out prevCursor, out nextCursor);
        }

        /// <summary>
        /// Get all lists which member contains someone
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userScreenName">target user id</param>
        public static IEnumerable<TwitterList> GetMembershipListsAll(this CredentialProvider provider, string userScreenName)
        {
            var partialUri = userScreenName + "/lists/memberships.xml";
            return provider.GetListsAll(partialUri);
        }

        /// <summary>
        /// Get lists someone subscribed with full params
        /// </summary>
        public static IEnumerable<TwitterList> GetSubscribedLists(this CredentialProvider provider, string userScreenName, long? cursor, out long prevCursor, out long nextCursor)
        {
            var partialUri = userScreenName + "/lists/subscriptions.xml";
            return provider.GetLists(partialUri, cursor, out prevCursor, out nextCursor);
        }

        /// <summary>
        /// Get lists all someone subscribed
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userScreenName">target user id</param>
        public static IEnumerable<TwitterList> GetSubscribedListsAll(this CredentialProvider provider, string userScreenName)
        {
            var partialUri = userScreenName + "/lists/subscriptions.xml";
            return provider.GetListsAll(partialUri);
        }

        /// <summary>
        /// Get single list data
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userScreenName">list owner user's id</param>
        /// <param name="listId">list is</param>
        public static TwitterList GetList(this CredentialProvider provider, string userScreenName, string listId)
        {
            var list = provider.RequestAPIv1(userScreenName + "/lists/" + listId + ".xml",
                 CredentialProvider.RequestMethod.GET, null).Element("list");
            if (list != null)
                return TwitterList.FromNode(list);
            else
                return null;
        }

        /// <summary>
        /// Create or update list
        /// </summary>
        private static TwitterList CreateOrUpdateList(this CredentialProvider provider, string id, string name, string description, bool? inPrivate)
        {
            var kvp = new List<KeyValuePair<string, string>>();
            if (id != null)
                kvp.Add(new KeyValuePair<string, string>("id", id));
            if (name != null)
                kvp.Add(new KeyValuePair<string, string>("name", HttpUtility.UrlEncodeStrict(name, Encoding.UTF8, true)));
            if (description != null)
                kvp.Add(new KeyValuePair<string, string>("description", HttpUtility.UrlEncodeStrict(description, Encoding.UTF8, true)));
            if (inPrivate != null)
                kvp.Add(new KeyValuePair<string, string>("mode", inPrivate.Value ? "private" : "public"));
            var list = provider.RequestAPIv1(
                "user/lists.xml",
                 CredentialProvider.RequestMethod.POST,
                 kvp).Element("list");
            if (list != null)
                return TwitterList.FromNode(list);
            else
                return null;
        }

        /// <summary>
        /// Create new list
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="name">list name</param>
        /// <param name="description">list description</param>
        /// <param name="inPrivate">private mode</param>
        public static TwitterList CreateList(this CredentialProvider provider, string name, string description, bool? inPrivate)
        {
            return provider.CreateOrUpdateList(null, name, description, inPrivate);
        }

        /// <summary>
        /// Update list information
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="id">list id</param>
        /// <param name="newName">list's new name</param>
        /// <param name="description">new description</param>
        /// <param name="inPrivate">private mode</param>
        public static TwitterList UpdateList(this CredentialProvider provider, string id, string newName, string description, bool? inPrivate)
        {
            return provider.CreateOrUpdateList(id, newName, description, inPrivate);
        }

        /// <summary>
        /// Delete list you created
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userScreenName">your id</param>
        /// <param name="listId">list id</param>
        public static TwitterList DeleteList(this CredentialProvider provider, string userScreenName, string listId)
        {
            var kvp = new[] { new KeyValuePair<string, string>("_method", "DELETE") };
            var list = provider.RequestAPIv1(
                 userScreenName + "/lists/" + listId + ".xml",
                  CredentialProvider.RequestMethod.POST,
                  kvp).Element("list");
            if (list != null)
                return TwitterList.FromNode(list);
            else
                return null;
        }

        /// <summary>
        /// Add user into your list
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="yourScreenName">your screen name</param>
        /// <param name="listId">list id</param>
        /// <param name="addUserScreenName">adding user id or screen name</param>
        public static TwitterList AddUserIntoList(this CredentialProvider provider, string yourScreenName, string listId, string addUserScreenName)
        {
            var kvp = new[] { new KeyValuePair<string, string>("id", addUserScreenName) };
            var list = provider.RequestAPIv1(
                yourScreenName + "/" + listId + ".xml",
                CredentialProvider.RequestMethod.POST,
                kvp).Element("list");
            if (list != null)
                return TwitterList.FromNode(list);
            else
                return null;
        }

        /// <summary>
        /// Delete user from your list
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="yourScreenName">your screen name</param>
        /// <param name="listId">list id</param>
        /// <param name="deleteUserScreenName">deleting user id</param>
        public static TwitterList DeleteUserFromList(this CredentialProvider provider, string yourScreenName, string listId, string deleteUserScreenName)
        {
            var kvp = new[] { new KeyValuePair<string, string>("id", deleteUserScreenName), new KeyValuePair<string, string>("_method", "DELETE") };
            var list = provider.RequestAPIv1(
                yourScreenName + "/" + listId + ".xml",
                CredentialProvider.RequestMethod.POST,
                kvp).Element("list");
            if (list != null)
                return TwitterList.FromNode(list);
            else
                return null;
        }

        /// <summary>
        /// Get user information in list<para />
        /// You can use this method for check someone existing in list.
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="user">list owner user id or screen name</param>
        /// <param name="listId">list id</param>
        /// <param name="queryUserScreenName">query user id</param>
        public static TwitterUser GetListMember(this CredentialProvider provider, string user, string listId, string queryUserScreenName)
        {
            var query = provider.RequestAPIv1(
                user + "/" + listId + "/members/" + queryUserScreenName + ".xml",
                CredentialProvider.RequestMethod.GET, null).Element("user");
            if (user != null)
                return TwitterUser.FromNode(query);
            else
                return null;
        }

        /// <summary>
        /// Subscribe list
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="screenName">list owner user screen name</param>
        /// <param name="listId">list id</param>
        public static bool SubscribeList(this CredentialProvider provider, string screenName, string listId)
        {
            var users = provider.RequestAPIv1(
                screenName + "/" + listId + "/subscribers.xml",
                CredentialProvider.RequestMethod.POST, null).Element("users");
            if (users == null)
                return false;
            else
                return true;
        }

        /// <summary>
        /// UnSubscribe list
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="screenName">list owner user screen name</param>
        /// <param name="listId">list id</param>
        public static bool UnsubscribeList(this CredentialProvider provider, string screenName, string listId)
        {
            var kvp = new[] { new KeyValuePair<string, string>("_method", "DELETE") };
            var users = provider.RequestAPIv1(
                screenName + "/" + listId + "/subscribers.xml",
                CredentialProvider.RequestMethod.POST, kvp).Element("users");
            if (users == null)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Get subscriber information in list<para />
        /// You can use this method for check someone subscribing a list.
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="screenName">list owner user id</param>
        /// <param name="listId">list id</param>
        /// <param name="queryUserScreenName">query user id</param>
        public static TwitterUser GetListSubscriber(this CredentialProvider provider, string screenName, string listId, string queryUserScreenName)
        {
            var query = provider.RequestAPIv1(
                screenName + "/" + listId + "/subscribers/" + queryUserScreenName + ".xml",
                CredentialProvider.RequestMethod.GET, null).Element("user");
            if (query != null)
                return TwitterUser.FromNode(query);
            else
                return null;
        }
    }
}
