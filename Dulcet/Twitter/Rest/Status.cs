using System.Collections.Generic;
using System.Text;
using Dulcet.Twitter.Credential;
using Dulcet.Util;

namespace Dulcet.Twitter.Rest
{
    public static partial class Api
    {
        /// <summary>
        /// Get status
        /// </summary>
        private static TwitterStatus GetStatus(this CredentialProvider provider, string partialUri, CredentialProvider.RequestMethod method, IEnumerable<KeyValuePair<string, string>> para)
        {
            var doc = provider.RequestAPIv1(partialUri, method, para);
            if (doc == null)
                return null;
            return TwitterStatus.FromNode(doc.Root);
        }

        /// <summary>
        /// Get status from id
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="id">user id</param>
        public static TwitterStatus GetStatus(this CredentialProvider provider, long id)
        {
            return provider.GetStatus("statuses/show/" + id + ".json", CredentialProvider.RequestMethod.GET, CreateParamList());
        }

        /// <summary>
        /// Update new status
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="body">body</param>
        /// <param name="inReplyToStatusId">tweet id which be replied this tweet</param>
        public static TwitterStatus UpdateStatus(this CredentialProvider provider, string body, long? inReplyToStatusId = null)
        {
            var para = CreateParamList();
            para.Add(new KeyValuePair<string, string>("status", HttpUtility.UrlEncodeStrict(body, Encoding.UTF8, true)));
            if (inReplyToStatusId != null && inReplyToStatusId.HasValue)
                para.Add(new KeyValuePair<string, string>("in_reply_to_status_id", inReplyToStatusId.Value.ToString()));

            para.Add(new KeyValuePair<string, string>("include_entities", "true"));

            var doc = provider.RequestAPIv1("statuses/update.json", CredentialProvider.RequestMethod.POST, para);
            if (doc != null)
                return TwitterStatus.FromNode(doc.Root);
            else
                return null;
        }

        /// <summary>
        /// Delete your tweet
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="id">tweet id</param>
        public static TwitterStatus DestroyStatus(this CredentialProvider provider, long id)
        {
            return provider.GetStatus("statuses/destroy/" + id + ".json", CredentialProvider.RequestMethod.POST, CreateParamList());
        }
    }
}
