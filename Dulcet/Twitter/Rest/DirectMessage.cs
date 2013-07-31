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
        /// Get direct messages
        /// </summary>
        private static IEnumerable<TwitterDirectMessage> GetDirectMessages(this CredentialProvider provider, string partialUri, IEnumerable<KeyValuePair<string, string>> param)
        {
            var doc = provider.RequestAPIv1(partialUri, CredentialProvider.RequestMethod.GET, param);
            if (doc == null)
                return null;
            List<TwitterStatus> statuses = new List<TwitterStatus>();
            HashSet<string> hashes = new HashSet<string>();
            return doc.Root.Elements().Select(n => TwitterDirectMessage.FromNode(n)).Where(d => d != null);
        }

        /// <summary>
        /// Get direct messages with full params
        /// </summary>
        private static IEnumerable<TwitterDirectMessage> GetDirectMessages(this CredentialProvider provider, string partialUri, long? sinceId, long? maxId, long? count, long? page)
        {
            List<KeyValuePair<string, string>> para = new List<KeyValuePair<string, string>>();
            if (sinceId != null && sinceId.HasValue)
            {
                para.Add(new KeyValuePair<string, string>("since_id", sinceId.Value.ToString()));
            }
            if (maxId != null && maxId.HasValue)
            {
                para.Add(new KeyValuePair<string, string>("max_id", maxId.Value.ToString()));
            }
            if (count != null && count.HasValue)
            {
                para.Add(new KeyValuePair<string, string>("count", count.Value.ToString()));
            }
            if (page != null && page.HasValue)
            {
                para.Add(new KeyValuePair<string, string>("page", page.Value.ToString()));
            }

            para.Add(new KeyValuePair<string, string>("include_entities", "true"));

            return provider.GetDirectMessages(partialUri, para);
        }

        /// <summary>
        /// Get direct messages
        /// </summary>
        /// <param name="provider">credential provider</param>
        public static IEnumerable<TwitterDirectMessage> GetDirectMessages(this CredentialProvider provider)
        {
            return provider.GetDirectMessages("direct_messages.json", null);
        }

        /// <summary>
        /// Get direct messages with full params
        /// </summary>
        public static IEnumerable<TwitterDirectMessage> GetDirectMessages(this CredentialProvider provider, long? sinceId = null, long? maxId = null, long? count = null, long? page = null, bool? includeEntities = null)
        {
            return provider.GetDirectMessages("direct_messages.json", sinceId, maxId, count, page);
        }

        /// <summary>
        /// Get direct messages you sent
        /// </summary>
        /// <param name="provider">credential provider</param>
        public static IEnumerable<TwitterDirectMessage> GetSentDirectMessages(this CredentialProvider provider)
        {
            return provider.GetDirectMessages("direct_messages/sent.json", null);
        }

        /// <summary>
        /// Get direct messages you sent with full params
        /// </summary>
        public static IEnumerable<TwitterDirectMessage> GetSentDirectMessages(this CredentialProvider provider, long? sinceId = null, long? maxId = null, long? count = null, long? page = null, bool? includeEntities = null)
        {
            return provider.GetDirectMessages("direct_messages/sent.json", sinceId, maxId, count, page);
        }

        /// <summary>
        /// Send new direct message
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="user">target user id or name</param>
        /// <param name="text">send body</param>
        public static TwitterDirectMessage SendDirectMessage(this CredentialProvider provider, string screenName, string text)
        {
            List<KeyValuePair<string, string>> para = new List<KeyValuePair<string, string>>();
            para.Add(new KeyValuePair<string, string>("text", HttpUtility.UrlEncodeStrict(text, Encoding.UTF8, true)));
            para.Add(new KeyValuePair<string, string>("screen_name", screenName));

            var xmlDoc = provider.RequestAPIv1("direct_messages/new.json", CredentialProvider.RequestMethod.POST, para);
            if (xmlDoc == null)
                return null;

            return TwitterDirectMessage.FromNode(xmlDoc.Root);
        }

        /// <summary>
        /// Delete a direct message which you sent
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="id">destroy id</param>
        public static TwitterDirectMessage DestroyDirectMessage(this CredentialProvider provider, long id)
        {
            List<KeyValuePair<string, string>> para = new List<KeyValuePair<string, string>>();
            para.Add(new KeyValuePair<string, string>("id", id.ToString()));

            var xmlDoc = provider.RequestAPIv1("direct_messages/destroy.json", CredentialProvider.RequestMethod.POST, para);
            if (xmlDoc == null)
                return null;

            return TwitterDirectMessage.FromNode(xmlDoc.Root);
        }
    }
}
