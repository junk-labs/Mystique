using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Dulcet.Network;
using Dulcet.Twitter.Credential;
using Dulcet.Util;

namespace Dulcet.ThirdParty
{
    /// <summary>
    /// TwitPic API Implementation
    /// </summary>
    public static class TwitPicApi
    {
        static string UploadApiUrl = "http://api.twitpic.com/2/upload.xml";
        static string MediaShowUrl = "http://api.twitpic.com/2/media/show.xml?id={0}";
        /// <summary>
        /// Upload picture to TwitPic
        /// </summary>
        public static bool UploadToTwitpic(this OAuth provider, string apiKey, string message, string mediaFilePath, out string url)
        {
            url = null;
            var doc = provider.UploadToTwitpic(apiKey, message, mediaFilePath);
            if (doc == null || doc.Element("image") == null)
            {
                return false;
            }
            else
            {
                url = doc.Element("image").Element("url").ParseString();
                return true;
            }
        }

        /// <summary>
        /// Upload picture to TwitPic
        /// </summary>
        public static XDocument UploadToTwitpic(this OAuth provider, string apiKey, string message, string mediaFilePath)
        {
            var req = Http.CreateRequest(new Uri(UploadApiUrl), "POST", contentType: "application/x-www-form-urlencoded");

            // use OAuth Echo
            provider.MakeOAuthEchoRequest(ref req);

            List<SendData> sd = new List<SendData>();
            sd.Add(new SendData("key", apiKey));
            sd.Add(new SendData("message", message));
            sd.Add(new SendData("media", file: mediaFilePath));

            var doc = Http.WebUpload<XDocument>(req, sd, Encoding.UTF8, XDocument.Load);
            if (doc.ThrownException != null)
                throw doc.ThrownException;
            if (doc.Succeeded == false)
                return null;
            return doc.Data;
        }

        /// <summary>
        /// Get detail XML of image data
        /// </summary>
        /// <param name="id">picture id</param>
        /// <returns>XML document</returns>
        public static XDocument GetDetail(string id)
        {
            var dat = Http.WebConnect<XDocument>(
                Http.CreateRequest(new Uri(String.Format(MediaShowUrl, id)), contentType: "application/x-www-form-urlencoded"),
                Http.StreamConverters.ReadXml);
            if (dat.ThrownException != null)
                throw dat.ThrownException;
            else if (!dat.Succeeded)
                return null;
            else
                return dat.Data;
        }
    }
}
