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
        /// Create friendship with someone
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="user">target user id or screen name</param>
        [Obsolete("Please use other overload.")]
        public static TwitterUser CreateFriendship(this CredentialProvider provider, string user)
        {
            var ret = provider.RequestAPIv1("friendships/create/" + user + ".xml", CredentialProvider.RequestMethod.POST, null);
            if (ret != null && ret.Element("user") != null)
                return TwitterUser.FromNode(ret.Element("user"));
            else
                return null;
        }

        /// <summary>
        /// Create friendship with someone
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userId">target user id</param>
        /// <param name="screenName">target user screen name</param>
        /// <param name="follow">send his tweet to specified device</param>
        /// <remarks>
        /// user id or user screeen name must set.
        /// </remarks>
        public static TwitterUser CreateFriendship(this CredentialProvider provider, long? userId = null, string screenName = null, bool follow = false)
        {
            if (userId == null && String.IsNullOrEmpty(screenName))
                throw new ArgumentException("User id or screen name is must set.");
            List<KeyValuePair<string, string>> arg = new List<KeyValuePair<string, string>>();
            if (userId != null)
                arg.Add(new KeyValuePair<string, string>("user_id", userId.ToString()));
            if (!String.IsNullOrEmpty(screenName))
                arg.Add(new KeyValuePair<string, string>("screen_name", screenName));
            if (follow)
                arg.Add(new KeyValuePair<string, string>("follow", "true"));
            var ret = provider.RequestAPIv1("friendships/create.xml", CredentialProvider.RequestMethod.POST, arg);
            if (ret != null && ret.Element("user") != null)
                return TwitterUser.FromNode(ret.Element("user"));
            else
                return null;
        }

        /// <summary>
        /// Destroy friendship with someone
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="user">target user id or screen name</param>
        [Obsolete("Please use other overload.")]
        public static TwitterUser DestroyFriendship(this CredentialProvider provider, string user)
        {
            var ret = provider.RequestAPIv1("friendships/destroy/" + user + ".xml", CredentialProvider.RequestMethod.POST, null);
            if (ret != null && ret.Element("user") != null)
                return TwitterUser.FromNode(ret.Element("user"));
            else
                return null;
        }

        /// <summary>
        /// Destroy friendship with someone
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userId">target user id</param>
        /// <param name="screenName">target user screen name</param>
        /// <remarks>
        /// user id or user screeen name must set.
        /// </remarks>
        public static TwitterUser DestroyFriendship(this CredentialProvider provider, long? userId = null, string screenName = null)
        {
            if (userId == null && String.IsNullOrEmpty(screenName))
                throw new ArgumentException("User id or screen name is must set.");
            List<KeyValuePair<string, string>> arg = new List<KeyValuePair<string, string>>();
            if (userId != null)
                arg.Add(new KeyValuePair<string, string>("user_id", userId.ToString()));
            if (!String.IsNullOrEmpty(screenName))
                arg.Add(new KeyValuePair<string, string>("screen_name", screenName));
            var ret = provider.RequestAPIv1("friendships/destroy.xml", CredentialProvider.RequestMethod.POST, arg);
            if (ret != null && ret.Element("user") != null)
                return TwitterUser.FromNode(ret.Element("user"));
            else
                return null;
        }

        /// <summary>
        /// Check exists friendship
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userA">user A</param>
        /// <param name="userB">user B</param>
        /// <returns>if user A or B is not existed, this method returns false.</returns>
        public static bool ExistsFriendship(this CredentialProvider provider, string userA, string userB)
        {
            List<KeyValuePair<string, string>> arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("user_a", userA));
            arg.Add(new KeyValuePair<string, string>("user_b", userB));
            var ret = provider.RequestAPIv1("friendship/exists.xml", CredentialProvider.RequestMethod.GET, arg);
            if (ret.Element("friends") != null)
                return ret.Element("friends").ParseBool();
            else
                return false;
        }

        /// <summary>
        /// Create block someone
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="user">target user id or screen name</param>
        public static TwitterUser CreateBlockUser(this CredentialProvider provider, string user)
        {
            var ret = provider.RequestAPIv1("blocks/create/" + user + ".xml", CredentialProvider.RequestMethod.POST, null);
            if (ret != null && ret.Element("user") != null)
                return TwitterUser.FromNode(ret.Element("user"));
            else
                return null;
        }

        /// <summary>
        /// Create block someone
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userId">block user id</param>
        /// <param name="screenName">block user screen name</param>
        /// <remarks>
        /// user id or user screeen name must set.
        /// </remarks>
        public static TwitterUser CreateBlockUser(this CredentialProvider provider, long? userId = null, string screenName = null)
        {
            if (userId == null && String.IsNullOrEmpty(screenName))
                throw new ArgumentException("User id or screen name is must set.");
            List<KeyValuePair<string, string>> arg = new List<KeyValuePair<string, string>>();
            if (userId != null)
                arg.Add(new KeyValuePair<string, string>("user_id", userId.ToString()));
            if (!String.IsNullOrEmpty(screenName))
                arg.Add(new KeyValuePair<string, string>("screen_name", screenName));
            var ret = provider.RequestAPIv1("blocks/create.xml", CredentialProvider.RequestMethod.POST, arg);
            if (ret != null && ret.Element("user") != null)
                return TwitterUser.FromNode(ret.Element("user"));
            else
                return null;
        }

        /// <summary>
        /// Destroy block someone
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="user">target user id or screen name</param>
        public static TwitterUser DestroyBlockUser(this CredentialProvider provider, string user)
        {
            var ret = provider.RequestAPIv1("blocks/destroy/" + user + ".xml", CredentialProvider.RequestMethod.POST, null);
            if (ret != null && ret.Element("user") != null)
                return TwitterUser.FromNode(ret.Element("user"));
            else
                return null;
        }

        /// <summary>
        /// Destroy block someone
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userId">block user id</param>
        /// <param name="screenName">block user screen name</param>
        /// <remarks>
        /// user id or user screeen name must set.
        /// </remarks>
        public static TwitterUser DestroyBlockUser(this CredentialProvider provider, long? userId = null, string screenName = null)
        {
            if (userId == null && String.IsNullOrEmpty(screenName))
                throw new ArgumentException("User id or screen name is must set.");
            List<KeyValuePair<string, string>> arg = new List<KeyValuePair<string, string>>();
            if (userId != null)
                arg.Add(new KeyValuePair<string, string>("user_id", userId.ToString()));
            if (!String.IsNullOrEmpty(screenName))
                arg.Add(new KeyValuePair<string, string>("screen_name", screenName));
            var ret = provider.RequestAPIv1("blocks/destroy.xml", CredentialProvider.RequestMethod.POST, arg);
            if (ret != null && ret.Element("user") != null)
                return TwitterUser.FromNode(ret.Element("user"));
            else
                return null;
        }

        /// <summary>
        /// Check blocking someone
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="user">target user id or screen name</param>
        public static TwitterUser ExistsBlockUser(this CredentialProvider provider, string user)
        {
            var ret = provider.RequestAPIv1("blocks/exists/" + user + ".xml", CredentialProvider.RequestMethod.POST, null);
            if (ret != null && ret.Element("user") != null)
                return TwitterUser.FromNode(ret.Element("user"));
            else
                return null;
        }

        /// <summary>
        /// Check blocking someone
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userId">block user id</param>
        /// <param name="screenName">block user screen name</param>
        /// <remarks>
        /// user id or user screeen name must set.
        /// </remarks>
        public static TwitterUser ExistsBlockUser(this CredentialProvider provider, long? userId = null, string screenName = null)
        {
            if (userId == null && String.IsNullOrEmpty(screenName))
                throw new ArgumentException("User id or screen name is must set.");
            List<KeyValuePair<string, string>> arg = new List<KeyValuePair<string, string>>();
            if (userId != null)
                arg.Add(new KeyValuePair<string, string>("user_id", userId.ToString()));
            if (!String.IsNullOrEmpty(screenName))
                arg.Add(new KeyValuePair<string, string>("screen_name", screenName));
            var ret = provider.RequestAPIv1("blocks/exists.xml", CredentialProvider.RequestMethod.POST, arg);
            if (ret != null && ret.Element("user") != null)
                return TwitterUser.FromNode(ret.Element("user"));
            else
                return null;
        }

        /// <summary>
        /// Get blocking user's list
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="page">page of blocking list(1-)</param>
        public static IEnumerable<TwitterUser> GetBlockingUsers(this CredentialProvider provider, int? page = null)
        {
            List<KeyValuePair<string, string>> arg = null;
            if (page != null)
            {
                arg = new List<KeyValuePair<string, string>>();
                arg.Add(new KeyValuePair<string, string>("page", page.Value.ToString()));
            }
            var ret = provider.RequestAPIv1("blocks/blocking.xml", CredentialProvider.RequestMethod.GET, arg);
            if (ret != null)
                return from n in ret.Descendants("user")
                       let usr = TwitterUser.FromNode(n)
                       where usr != null
                       select usr;
            else
                return null;
        }

        /// <summary>
        /// Get blocking user's list (all)
        /// </summary>
        /// <param name="provider">credential provider</param>
        public static IEnumerable<TwitterUser> GetBlockingUsersAll(this CredentialProvider provider)
        {
            return provider.GetBlockingUsers();
            // Page parameter is currently disabled.
            /*
            int page = 1;
            while(true)
            {
                var ret = GetBlockingUsers(provider, page);
                if (ret == null)
                    break;
                foreach(var u in ret)
                    yield return u;
                if (ret.Count() < 20)
                    break;
                page++;
            }
            */
        }

        /// <summary>
        /// Report spam and block someone
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userId">block user id</param>
        /// <param name="screenName">block user screen name</param>
        /// <remarks>
        /// user id or user screeen name must set.
        /// </remarks>
        public static TwitterUser ReportSpam(this CredentialProvider provider, long? userId = null, string screenName = null)
        {
            if (userId == null && String.IsNullOrEmpty(screenName))
                throw new ArgumentException("User id or screen name is must set.");
            List<KeyValuePair<string, string>> arg = new List<KeyValuePair<string, string>>();
            if (userId != null)
                arg.Add(new KeyValuePair<string, string>("user_id", userId.ToString()));
            if (!String.IsNullOrEmpty(screenName))
                arg.Add(new KeyValuePair<string, string>("screen_name", screenName));
            var ret = provider.RequestAPIv1("report_spam.xml", CredentialProvider.RequestMethod.POST, arg);
            if (ret != null && ret.Element("user") != null)
                return TwitterUser.FromNode(ret.Element("user"));
            else
                return null;
        }
    }
}
