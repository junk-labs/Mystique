using System;
using System.Collections.Generic;
using System.Linq;
using Dulcet.Twitter.Credential;
using Dulcet.Util;

namespace Dulcet.Twitter.Rest
{
    public static partial class Api
    {
        /// <summary>
        /// Get list statuses with full params
        /// </summary>
        public static IEnumerable<TwitterStatus> GetListStatuses(this CredentialProvider provider, string userScreenName, string listId, string sinceId = null, string maxId = null, long? perPage = null, long? page = null, bool? includeRts = null, bool? includeEntities = null)
        {
            listId = listId.Replace("_", "-");
            var partialUri = userScreenName + "/lists/" + listId + "/statuses.json";

            List<KeyValuePair<string, string>> para = new List<KeyValuePair<string, string>>();

            if (!String.IsNullOrEmpty(sinceId))
                para.Add(new KeyValuePair<string, string>("since_id", sinceId.ToString()));

            if (!String.IsNullOrEmpty(maxId))
                para.Add(new KeyValuePair<string, string>("max_id", maxId.ToString()));

            if (perPage != null)
                para.Add(new KeyValuePair<string, string>("per_page", perPage.ToString()));

            if (page != null)
                para.Add(new KeyValuePair<string, string>("page", page.ToString()));

            if (includeRts != null && includeRts.Value)
                para.Add(new KeyValuePair<string, string>("includeRts", "true"));

            if (includeEntities != null && includeEntities.Value)
                para.Add(new KeyValuePair<string, string>("includeEntities", "true"));

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
            return provider.GetUsersAll(userScreenName + "/" + listId + "/members.json", null, null);
        }

        /// <summary>
        /// Get list members with full params
        /// </summary>
        public static IEnumerable<TwitterUser> GetListMembers(this CredentialProvider provider, string userScreenName, string listId, long? cursor, out long prevCursor, out long nextCursor)
        {
            if (cursor == null)
                cursor = -1;
            listId = listId.Replace("_", "-");
            return provider.GetUsers(userScreenName + "/" + listId + "/members.json", null, null, cursor, out prevCursor, out nextCursor);
        }

        /// <summary>
        /// Get list subscribers with full params
        /// </summary>
        public static IEnumerable<TwitterUser> GetListSubscribers(this CredentialProvider provider, string userScreenName, string listId, long? cursor, out long prevCursor, out long nextCursor)
        {
            if (cursor == null)
                cursor = -1;
            listId = listId.Replace("_", "-");
            return provider.GetUsers(userScreenName + "/" + listId + "/subscribers.json", null, null, cursor, out prevCursor, out nextCursor);
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
            return provider.GetUsersAll(userScreenName + "/" + listId + "/subscribers.json", null, null);
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
            var nc = doc.Root.Element("next_cursor");
            if (nc != null)
                nextCursor = (long)nc.ParseLong();
            var pc = doc.Root.Element("previous_cursor");
            if (pc != null)
                prevCursor = (long)pc.ParseLong();

            System.Diagnostics.Debug.WriteLine("Lists :::" + Environment.NewLine + doc);

            return doc.Root.Element("lists").Elements().Select(n => TwitterList.FromNode(n)).Where(n => n != null);
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
            var partialUri = userScreenName + "/lists.json";
            return provider.GetLists(partialUri, cursor, out prevCursor, out nextCursor);
        }

        /// <summary>
        /// Get all lists someone created
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userScreenName">target user id</param>
        public static IEnumerable<TwitterList> GetUserListsAll(this CredentialProvider provider, string userScreenName)
        {
            var partialUri = userScreenName + "/lists.json";
            return provider.GetListsAll(partialUri);
        }

        /// <summary>
        /// Get lists which member contains someone with full params
        /// </summary>
        public static IEnumerable<TwitterList> GetMembershipLists(this CredentialProvider provider, string userScreenName, long? cursor, out long prevCursor, out long nextCursor)
        {
            var partialUri = userScreenName + "/lists/memberships.json";
            return provider.GetLists(partialUri, cursor, out prevCursor, out nextCursor);
        }

        /// <summary>
        /// Get all lists which member contains someone
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userScreenName">target user id</param>
        public static IEnumerable<TwitterList> GetMembershipListsAll(this CredentialProvider provider, string userScreenName)
        {
            var partialUri = userScreenName + "/lists/memberships.json";
            return provider.GetListsAll(partialUri);
        }

        /// <summary>
        /// Get lists someone subscribed with full params
        /// </summary>
        public static IEnumerable<TwitterList> GetSubscribedLists(this CredentialProvider provider, string userScreenName, long? cursor, out long prevCursor, out long nextCursor)
        {
            var partialUri = userScreenName + "/lists/subscriptions.json";
            return provider.GetLists(partialUri, cursor, out prevCursor, out nextCursor);
        }

        /// <summary>
        /// Get lists all someone subscribed
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userScreenName">target user id</param>
        public static IEnumerable<TwitterList> GetSubscribedListsAll(this CredentialProvider provider, string userScreenName)
        {
            var partialUri = userScreenName + "/lists/subscriptions.json";
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
            var list = provider.RequestAPIv1(userScreenName + "/lists/" + listId + ".json",
                 CredentialProvider.RequestMethod.GET, null).Root;
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
                user + "/" + listId + "/members/" + queryUserScreenName + ".json",
                CredentialProvider.RequestMethod.GET, null).Root;
            if (user != null)
                return TwitterUser.FromNode(query);
            else
                return null;
        }
    }
}
