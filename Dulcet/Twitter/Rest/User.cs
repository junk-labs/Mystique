using System.Collections.Generic;
using System.Linq;
using Dulcet.Twitter.Credential;
using Dulcet.Util;
using System;

namespace Dulcet.Twitter.Rest
{
    public static partial class Api
    {
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
            var users = new List<TwitterUser>();
            var nc = doc.Root.Element("next_cursor");
            if (nc != null)
                nextCursor = (long)nc.ParseLong();
            var pc = doc.Root.Element("previous_cursor");
            if (pc != null)
                prevCursor = (long)pc.ParseLong();
            System.Diagnostics.Debug.WriteLine("GetUser:::" + Environment.NewLine + doc.ToString());
            return doc.Root.Element("users").Elements().Select(TwitterUser.FromNode).Where(s => s != null);
        }

        /// <summary>
        /// Get users with use cursor params
        /// </summary>
        private static IEnumerable<TwitterUser> GetUsersAll(this CredentialProvider provider, string partialUri, IEnumerable<KeyValuePair<string, string>> para)
        {
            long n_cursor = -1;
            long c_cursor = -1;
            long p;
            var parac = para.ToArray();
            while (n_cursor != 0)
            {
                var pc = parac.ToList();
                pc.Add(new KeyValuePair<string, string>("cursor", c_cursor.ToString()));
                var users = provider.GetUsers(partialUri, pc, out p, out n_cursor);
                if (users != null)
                    foreach (var u in users)
                        yield return u;
                c_cursor = n_cursor;
            }
        }
        /// <summary>
        /// Get user with full params
        /// </summary>
        private static TwitterUser GetUser(this CredentialProvider provider, string partialUri, CredentialProvider.RequestMethod method, long? userId, string screenName)
        {
            var para = new List<KeyValuePair<string, string>>();
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
            return TwitterUser.FromNode(doc.Root);
        }

        /// <summary>
        /// Get user information
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userId">target user id</param>
        public static TwitterUser GetUser(this CredentialProvider provider, long userId)
        {
            return provider.GetUser("users/show.json", CredentialProvider.RequestMethod.GET, userId, null);
        }

        /// <summary>
        /// Get user by screen name
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="screenName">target user screen name</param>
        public static TwitterUser GetUserByScreenName(this CredentialProvider provider, string screenName)
        {
            return provider.GetUser("users/show.json", CredentialProvider.RequestMethod.GET, null, screenName);
        }

        /// <summary>
        /// Get users
        /// </summary>
        private static IEnumerable<TwitterUser> GetUsers(this CredentialProvider provider, string partialUri, IEnumerable<KeyValuePair<string, string>> para)
        {
            var doc = provider.RequestAPIv1(partialUri, CredentialProvider.RequestMethod.GET, para);
            if (doc == null)
                return null; // request returns error ?
            System.Diagnostics.Debug.WriteLine("GetUsers ::: " + Environment.NewLine + doc);
            return doc.Root.Element("users").Elements().Select(u => TwitterUser.FromNode(u)).Where(u => u != null);
        }

        /// <summary>
        /// ユーザー情報を取得します。
        /// </summary>
        public static IEnumerable<TwitterUser> LookupUsers(this CredentialProvider provider, string[] screenNames = null, long[] ids = null)
        {
            if (screenNames == null && ids == null)
                throw new ArgumentNullException("screenNameとid,両方をnullに設定することはできません。");
            if (screenNames != null)
                return provider.GetUsers("users/lookup.json", new[] { new KeyValuePair<string, string>("screen_name", String.Join(",", screenNames)) });
            else
                return provider.GetUsers("users/lookup.json", new[] { new KeyValuePair<string, string>("user_id", String.Join(",", ids.Select(l => l.ToString()))) });
        }

        /// <summary>
        /// Get users
        /// </summary>
        private static IEnumerable<long> GetUserIds(this CredentialProvider provider, string partialUri, IEnumerable<KeyValuePair<string, string>> para, out long prevCursor, out long nextCursor)
        {
            prevCursor = 0;
            nextCursor = 0;
            var doc = provider.RequestAPIv1(partialUri, CredentialProvider.RequestMethod.GET, para);
            if (doc == null)
                return null; // request returns error ?
            List<TwitterUser> users = new List<TwitterUser>();
            var nc = doc.Root.Element("next_cursor");
            if (nc != null)
                nextCursor = (long)nc.ParseLong();
            var pc = doc.Root.Element("previous_cursor");
            if (pc != null)
                prevCursor = (long)pc.ParseLong();
            if (doc.Root.Element("ids") != null)
                return doc.Root.Element("ids").Elements().Select(n => n.ParseLong()).Where(n => n != 0);
            else
                return doc.Root.Elements().Select(n => n.ParseLong()).Where(n => n != 0);
        }

        /// <summary>
        /// Get users with full params
        /// </summary>
        private static IEnumerable<long> GetUserIds(this CredentialProvider provider, string partialUri, long? userId, string screenName, long? cursor, out long prevCursor, out long nextCursor)
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
            return provider.GetUserIds(partialUri, para, out prevCursor, out nextCursor);
        }

        /// <summary>
        /// Get users with use cursor params
        /// </summary>
        private static IEnumerable<long> GetUserIdsAll(this CredentialProvider provider, string partialUri, long? userId, string screenName)
        {
            long n_cursor = -1;
            long c_cursor = -1;
            long p;
            while (n_cursor != 0)
            {
                var users = provider.GetUserIds(partialUri, userId, screenName, c_cursor, out p, out n_cursor);
                if (users != null)
                    foreach (var u in users)
                        yield return u;
                c_cursor = n_cursor;
            }
        }

        public static IEnumerable<long> GetFriendIds(this CredentialProvider provider, long? userId = null, string screenName = null)
        {
            return provider.GetUserIdsAll("friends/ids.json", userId, screenName);
        }

        public static IEnumerable<long> GetFollowerIds(this CredentialProvider provider, long? userId = null, string screenName = null)
        {
            return provider.GetUserIdsAll("followers/ids.json", userId, screenName);
        }

        public static IEnumerable<long> GetBlockingIds(this CredentialProvider provider)
        {
            return provider.GetUserIdsAll("blocks/ids.json", null, null);
        }
    }
}
