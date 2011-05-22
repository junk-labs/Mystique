using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dulcet.Twitter.Credential;

namespace Dulcet.Twitter.Rest
{
    public static partial class Api
    {
        /// <summary>
        /// Get twitter timeline
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="partialUri">partial uri</param>
        /// <param name="param">parameters</param>
        private static IEnumerable<TwitterStatus> GetTimeline(this CredentialProvider provider, string partialUri, IEnumerable<KeyValuePair<string, string>> param)
        {
            var doc = provider.RequestAPIv1(partialUri, CredentialProvider.RequestMethod.GET, param);
            if (doc == null)
                return null;
            List<TwitterStatus> statuses = new List<TwitterStatus>();
            HashSet<string> hashes = new HashSet<string>();
            return from n in doc.Descendants("status")
                   let s = TwitterStatus.FromNode(n)
                   where s != null
                   select s;
        }

        /// <summary>
        /// Get timeline with full parameters
        /// </summary>
        private static IEnumerable<TwitterStatus> GetTimeline(this CredentialProvider provider, string partialUri, long? sinceId, long? maxId, long? count, long? page, long? userId, string screenName)
        {
            List<KeyValuePair<string, string>> para = new List<KeyValuePair<string, string>>();
            if (sinceId != null && sinceId.HasValue)
                para.Add(new KeyValuePair<string, string>("since_id", sinceId.Value.ToString()));

            if (maxId != null && maxId.HasValue)
                para.Add(new KeyValuePair<string, string>("max_id", maxId.Value.ToString()));

            if (count != null)
                para.Add(new KeyValuePair<string, string>("count", count.ToString()));

            if (page != null)
                para.Add(new KeyValuePair<string, string>("page", page.ToString()));

            if (userId != null)
                para.Add(new KeyValuePair<string, string>("user_id", userId.ToString()));

            if (!String.IsNullOrEmpty(screenName))
                para.Add(new KeyValuePair<string, string>("screen_name", screenName));

            return provider.GetTimeline(partialUri, para);
        }

        /// <summary>
        /// Get public timeline<para />
        /// This result will caching while 60 seconds in Twitter server.
        /// </summary>
        /// <param name="provider">credential provider</param>
        public static IEnumerable<TwitterStatus> GetPublicTimeline(this CredentialProvider provider)
        {
            return provider.GetTimeline("statuses/public_timeline.xml", null);
        }

        /// <summary>
        /// Get home timeline (it contains following users' tweets)
        /// </summary>
        /// <param name="provider">credential provider</param>
        public static IEnumerable<TwitterStatus> GetHomeTimeline(this CredentialProvider provider)
        {
            return provider.GetTimeline("statuses/home_timeline.xml", null);
        }

        /// <summary>
        /// Get home timeline with full params (it contains following users' tweets)
        /// </summary>
        public static IEnumerable<TwitterStatus> GetHomeTimeline(this CredentialProvider provider, long? sinceId = null, long? maxId = null, long? count = null, long? page = null)
        {
            return provider.GetTimeline("statuses/home_timeline.xml", sinceId, maxId, count, page, null, null);
        }

        /// <summary>
        /// Get friends timeline
        /// </summary>
        /// <param name="provider">credential provider</param>
        public static IEnumerable<TwitterStatus> GetFriendsTimeline(this CredentialProvider provider)
        {
            return provider.GetTimeline("statuses/friends_timeline.xml", null);
        }

        /// <summary>
        /// Get friends timeline with full params
        /// </summary>
        public static IEnumerable<TwitterStatus> GetFriendsTimeline(this CredentialProvider provider, string id = null, long? sinceId = null, long? maxId = null, long? count = null, long? page = null)
        {
            List<KeyValuePair<string, string>> para = new List<KeyValuePair<string, string>>();
            if (sinceId != null && sinceId.HasValue)
                para.Add(new KeyValuePair<string, string>("since_id", sinceId.Value.ToString()));

            if (maxId != null && maxId.HasValue)
                para.Add(new KeyValuePair<string, string>("max_id", maxId.Value.ToString()));

            if (count != null)
                para.Add(new KeyValuePair<string, string>("count", count.ToString()));

            if (page != null)
                para.Add(new KeyValuePair<string, string>("page", page.ToString()));

            if (!String.IsNullOrEmpty(id))
                para.Add(new KeyValuePair<string, string>("id", id.ToString()));

            return provider.GetTimeline("statuses/friends_timeline.xml", para);
        }

        /// <summary>
        /// Get user timeline
        /// </summary>
        /// <param name="provider">credential provider</param>
        public static IEnumerable<TwitterStatus> GetUserTimeline(this CredentialProvider provider)
        {
            return provider.GetTimeline("statuses/user_timeline.xml", null);
        }

        /// <summary>
        /// Get user timeline with full params
        /// </summary>
        public static IEnumerable<TwitterStatus> GetUserTimeline(this CredentialProvider provider, string id = null, long? userId = null, string screenName = null, long? sinceId = null, long? maxId = null, long? count = null, long? page = null, bool? trim_user = null, bool? include_rts = null, bool? include_entities = null)
        {
            List<KeyValuePair<string, string>> para = new List<KeyValuePair<string, string>>();
            if (sinceId != null && sinceId.HasValue)
                para.Add(new KeyValuePair<string, string>("since_id", sinceId.Value.ToString()));

            if (maxId != null && maxId.HasValue)
                para.Add(new KeyValuePair<string, string>("max_id", maxId.Value.ToString()));

            if (count != null)
                para.Add(new KeyValuePair<string, string>("count", count.ToString()));

            if (page != null)
                para.Add(new KeyValuePair<string, string>("page", page.ToString()));

            if (userId != null)
                para.Add(new KeyValuePair<string, string>("user_id", userId.ToString()));

            if (!String.IsNullOrEmpty(id))
                para.Add(new KeyValuePair<string, string>("id", id.ToString()));

            if (!String.IsNullOrEmpty(screenName))
                para.Add(new KeyValuePair<string, string>("screen_name", screenName.ToString()));

            if (trim_user != null)
                para.Add(new KeyValuePair<string, string>("trim_user", "true"));

            if (include_rts != null)
                para.Add(new KeyValuePair<string, string>("include_rts", "true"));

            if (include_entities != null)
                para.Add(new KeyValuePair<string, string>("include_entities", "true"));

            return provider.GetTimeline("statuses/user_timeline.xml", para);
        }

        /// <summary>
        /// Get mentions
        /// </summary>
        /// <param name="provider">credential provider</param>
        public static IEnumerable<TwitterStatus> GetMentions(this CredentialProvider provider)
        {
            return provider.GetTimeline("statuses/mentions.xml", null);
        }

        /// <summary>
        /// Get mentions with full params
        /// </summary>
        public static IEnumerable<TwitterStatus> GetMentions(this CredentialProvider provider, long? sinceId = null, long? maxId = null, long? count = null, long? page = null)
        {
            return provider.GetTimeline("statuses/mentions.xml", sinceId, maxId, count, page, null, null);
        }

        /// <summary>
        /// Get favorite timeline
        /// </summary>
        /// <param name="provider">credential provider</param>
        public static IEnumerable<TwitterStatus> GetFavorites(this CredentialProvider provider)
        {
            return provider.GetTimeline("favorites.xml", null, null, null, null, null, null);
        }

        /// <summary>
        /// Get favorite timeline with full params
        /// </summary>
        public static IEnumerable<TwitterStatus> GetFavorites(this CredentialProvider provider, string id = null, long? page = null)
        {
            if (id == null)
                return provider.GetTimeline("favorites.xml", null, null, null, page, null, null);
            else
                return provider.GetTimeline("favorites/" + id + ".xml", null, null, null, page, null, null);
        }
    }
}
