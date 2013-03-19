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
            var para = CreateParamList();
            if (userId != null)
                para.Add(new KeyValuePair<string, string>("user_id", userId.ToString()));
            if (!String.IsNullOrEmpty(screenName))
                para.Add(new KeyValuePair<string, string>("screen_name", screenName));
            if (follow)
                para.Add(new KeyValuePair<string, string>("follow", "true"));
            var ret = provider.RequestAPIv1("friendships/create.json", CredentialProvider.RequestMethod.POST, para);
            if (ret != null && ret.Root != null)
                return TwitterUser.FromNode(ret.Root);
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
            var ret = provider.RequestAPIv1("friendships/destroy.json", CredentialProvider.RequestMethod.POST, arg);
            if (ret != null && ret.Root != null)
                return TwitterUser.FromNode(ret.Root);
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
            var ret = provider.RequestAPIv1("friendship/exists.json", CredentialProvider.RequestMethod.GET, arg);
            if (ret.Element("friends") != null)
                return ret.Element("friends").ParseBool();
            else
                return false;
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
            var ret = provider.RequestAPIv1("blocks/create.json", CredentialProvider.RequestMethod.POST, arg);
            if (ret != null && ret.Root != null)
                return TwitterUser.FromNode(ret.Root);
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
            var ret = provider.RequestAPIv1("blocks/destroy.json", CredentialProvider.RequestMethod.POST, arg);
            if (ret != null && ret.Root != null)
                return TwitterUser.FromNode(ret.Root);
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
            var ret = provider.RequestAPIv1("blocks/exists.json", CredentialProvider.RequestMethod.POST, arg);
            if (ret != null && ret.Root != null)
                return TwitterUser.FromNode(ret.Root);
            else
                return null;
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
            var para = CreateParamList();
            if (userId != null)
                para.Add(new KeyValuePair<string, string>("user_id", userId.ToString()));
            if (!String.IsNullOrEmpty(screenName))
                para.Add(new KeyValuePair<string, string>("screen_name", screenName));
            var ret = provider.RequestAPIv1("users/report_spam.json", CredentialProvider.RequestMethod.POST, para);
            if (ret != null && ret.Root != null)
                return TwitterUser.FromNode(ret.Root);
            return null;
        }
    }
}
