using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml.Linq;
using Dulcet.Network;
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

        public static TwitterStatus UpdateWithMedia(this CredentialProvider provider, string body, string filePath,
                                                     long? inReplyToStatusId = null)
        {
            var url = Api.TwitterUri + "statuses/update_with_media.json";
            var req = Http.CreateRequest(new Uri(url), "POST",
                                         contentType: "multipart/form-data");
            var reg = typeof(OAuth).InvokeMember(
                "GetHeader",
                BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                provider,
                new object[] { url, CredentialProvider.RequestMethod.POST, null, null, false });
            req.Headers.Add("Authorization", "OAuth " + reg);

            // 10 sec
            req.Timeout = 100000;

            var sd = new List<SendData>();
            sd.Add(new SendData("status", body));
            if (inReplyToStatusId != null)
            {
                sd.Add(new SendData("in_reply_to_status_id", inReplyToStatusId.Value.ToString()));
            }
            sd.Add(new SendData("media[]", file: filePath));

            var doc = Http.WebUpload(req, sd, Encoding.UTF8, stream =>
            {
                using (var json = JsonReaderWriterFactory.CreateJsonReader(stream,
                                                                           System.Xml.XmlDictionaryReaderQuotas.Max))
                {
                    return XDocument.Load(json);
                }
            });
            if (doc.ThrownException != null)
                throw doc.ThrownException;
            if (!doc.Succeeded)
                return null;
            return TwitterStatus.FromNode(doc.Data.Root);
        }
    }
}
