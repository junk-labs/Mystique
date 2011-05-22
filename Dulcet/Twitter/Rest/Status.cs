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
        private static TwitterStatus GetStatus(this CredentialProvider provider, string partialUriFormat, CredentialProvider.RequestMethod method, long id)
        {
            string partialUri = string.Format(partialUriFormat, id);
            var doc = provider.RequestAPIv1(partialUri, method, null);
            if (doc == null)
                return null;
            return TwitterStatus.FromNode(doc.Element("status"));
        }

        /// <summary>
        /// Get status from id
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="id">user id</param>
        public static TwitterStatus GetStatus(this CredentialProvider provider, long id)
        {
            return provider.GetStatus("statuses/show/{0}.xml", CredentialProvider.RequestMethod.GET, id);
        }

        /// <summary>
        /// Update new status
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="body">body</param>
        /// <param name="inReplyToStatusId">tweet id which be replied this tweet</param>
        public static TwitterStatus UpdateStatus(this CredentialProvider provider, string body, long? inReplyToStatusId = null)
        {
            List<KeyValuePair<string, string>> para = new List<KeyValuePair<string, string>>();
            para.Add(new KeyValuePair<string, string>("status", HttpUtility.UrlEncodeStrict(body, Encoding.UTF8, true)));
            if (inReplyToStatusId != null && inReplyToStatusId.HasValue)
            {
                para.Add(new KeyValuePair<string, string>("in_reply_to_status_id", inReplyToStatusId.Value.ToString()));
            }
            var doc = provider.RequestAPIv1("statuses/update.xml", CredentialProvider.RequestMethod.POST, para);
            if (doc != null)
                return TwitterStatus.FromNode(doc.Element("status"));
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
            return provider.GetStatus("statuses/destroy/{0}.xml", CredentialProvider.RequestMethod.POST, id);
        }

    }
}
