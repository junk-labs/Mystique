using System.Collections.Generic;
using System.Linq;
using Dulcet.Twitter.Credential;
using Dulcet.Util;

namespace Dulcet.Twitter.Rest
{
    public static partial class Api
    {
        /// <summary>
        /// Get user with full params
        /// </summary>
        private static TwitterUser GetUser(this CredentialProvider provider, string partialUri, CredentialProvider.RequestMethod method, long? userId, string screenName)
        {
            List<KeyValuePair<string, string>> para = new List<KeyValuePair<string, string>>();
            if (userId != null)
            {
                para.Add(new KeyValuePair<string, string>("user_id", userId.ToString()));
            }
            if (screenName != null)
            {
                para.Add(new KeyValuePair<string, string>("screen_name", screenName));
            }
            var doc = provider.RequestAPIv1(partialUri, method, para);
            if (doc == null)
                return null;
            return TwitterUser.FromNode(doc.Element("user"));
        }

        /// <summary>
        /// Get user information
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userId">target user id</param>
        public static TwitterUser GetUser(this CredentialProvider provider, long userId)
        {
            return provider.GetUser("users/show.xml", CredentialProvider.RequestMethod.GET, userId, null);
        }

        /// <summary>
        /// Get user by screen name
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="screenName">target user screen name</param>
        public static TwitterUser GetUserByScreenName(this CredentialProvider provider, string screenName)
        {
            return provider.GetUser("users/show.xml", CredentialProvider.RequestMethod.GET, null, screenName);
        }

        /// <summary>
        /// Get users
        /// </summary>
        private static IEnumerable<TwitterUser> GetUsers(this CredentialProvider provider, string partialUri, IEnumerable<KeyValuePair<string, string>> para, out long prevCursor, out long nextCursor)
        {
            prevCursor = 0;
            nextCursor = 0;
            var doc = provider.RequestAPIv1(partialUri, CredentialProvider.RequestMethod.GET, para);
            if (doc == null)
                return null; // request returns error ?
            List<TwitterUser> users = new List<TwitterUser>();
            var ul = doc.Element("users_list");
            if (ul != null)
            {
                var nc = ul.Element("next_cursor");
                if (nc != null)
                    nextCursor = (long)nc.ParseLong();
                var pc = ul.Element("previous_cursor");
                if (pc != null)
                    prevCursor = (long)pc.ParseLong();
            }
            return from n in doc.Descendants("user")
                   let usr = TwitterUser.FromNode(n)
                   where usr != null
                   select usr;
        }

        /// <summary>
        /// Get users with full params
        /// </summary>
        private static IEnumerable<TwitterUser> GetUsers(this CredentialProvider provider, string partialUri, long? userId, string screenName, long? cursor, out long prevCursor, out long nextCursor)
        {
            List<KeyValuePair<string, string>> para = new List<KeyValuePair<string, string>>();
            if (userId != null)
            {
                para.Add(new KeyValuePair<string, string>("user_id", userId.ToString()));
            }
            if (screenName != null)
            {
                para.Add(new KeyValuePair<string, string>("screen_name", screenName));
            }
            if (cursor != null)
            {
                para.Add(new KeyValuePair<string, string>("cursor", cursor.ToString()));
            }
            return provider.GetUsers(partialUri, para, out prevCursor, out nextCursor);
        }

        /// <summary>
        /// Get users with use cursor params
        /// </summary>
        private static IEnumerable<TwitterUser> GetUsersAll(this CredentialProvider provider, string partialUri, long? userId, string screenName)
        {
            long n_cursor = -1;
            long c_cursor = -1;
            long p;
            while (n_cursor != 0)
            {
                var users = provider.GetUsers(partialUri, userId, screenName, c_cursor, out p, out n_cursor);
                if (users != null)
                    foreach (var u in users)
                        yield return u;
                c_cursor = n_cursor;
            }
        }

        /// <summary>
        /// Get friends all
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userId">target user id</param>
        public static IEnumerable<TwitterUser> GetFriendsAll(this CredentialProvider provider, long? userId = null)
        {
            return provider.GetUsersAll("statuses/friends.xml", userId, null);
        }

        /// <summary>
        /// Get friends all
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="screenName">target user's screen name</param>
        public static IEnumerable<TwitterUser> GetFriendsAllByScreenName(this CredentialProvider provider, string screenName)
        {
            return provider.GetUsersAll("statuses/friends.xml", null, screenName);
        }

        /// <summary>
        /// Get friends with full params
        /// </summary>
        public static IEnumerable<TwitterUser> GetFriends(this CredentialProvider provider, long? userId, string screenName, long? cursor, out long prevCursor, out long nextCursor)
        {
            if (cursor == null)
                cursor = -1;
            return provider.GetUsers("statuses/friends.xml", userId, screenName, cursor, out prevCursor, out nextCursor);
        }

        /// <summary>
        /// Get followers all
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userId">target user id</param>
        public static IEnumerable<TwitterUser> GetFollowersAll(this CredentialProvider provider, long? userId = null)
        {
            return provider.GetUsersAll("statuses/followers.xml", userId, null);
        }

        /// <summary>
        /// Get followers all
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="screenName">target user's screen name</param>
        public static IEnumerable<TwitterUser> GetFollowersAllByScreenName(this CredentialProvider provider, string screenName)
        {
            return provider.GetUsersAll("statuses/followers.xml", null, screenName);
        }

        /// <summary>
        /// Get followers with full params
        /// </summary>
        public static IEnumerable<TwitterUser> GetFollowers(this CredentialProvider provider, long? userId, string screenName, long? cursor, out long prevCursor, out long nextCursor)
        {
            return provider.GetUsers("statuses/followers.xml", userId, screenName, cursor, out prevCursor, out nextCursor);
        }
    }
}
