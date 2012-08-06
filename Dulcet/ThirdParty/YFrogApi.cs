using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Dulcet.Network;
using Dulcet.Twitter.Credential;
using Dulcet.Util;

namespace Dulcet.ThirdParty
{
    public static class YFrogApi
    {
        static string UploadApiUrl = "http://yfrog.com/api/xauth_upload";

        /// <summary>
        /// Upload picture to YFrog
        /// </summary>
        public static bool UploadToYFrog(this OAuth provider, string apiKey, string mediaFilePath, out string url)
        {
            url = null;
            var doc = provider.UploadToYFrog(apiKey, mediaFilePath);
            if (doc == null || doc.Element("rsp") == null)
            {
                return false;
            }
            else
            {
                url = doc.Element("rsp").Element("mediaurl").ParseString();
                return true;
            }
        }

        /// <summary>
        /// Upload picture to TwitPic
        /// </summary>
        public static XDocument UploadToYFrog(this OAuth provider, string apiKey, string mediaFilePath)
        {
            var req = Http.CreateRequest(new Uri(UploadApiUrl), "POST", contentType: "application/x-www-form-urlencoded");

            // use OAuth Echo
            provider.MakeOAuthEchoRequest(ref req);

            List<SendData> sd = new List<SendData>();
            sd.Add(new SendData("key", apiKey));
            sd.Add(new SendData("media", file: mediaFilePath));

            var doc = Http.WebUpload<XDocument>(req, sd, Encoding.UTF8, (s) => XDocument.Load(s));
            if (doc.ThrownException != null)
                throw doc.ThrownException;
            if (doc.Succeeded == false)
                return null;
            return doc.Data;
        }

    }
}
