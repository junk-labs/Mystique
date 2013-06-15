using Dulcet.Network;
using Dulcet.Twitter.Credential;
using Dulcet.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Dulcet.ThirdParty
{
    public static class TwippleApi
    {
        static string UploadApiUrl = "http://p.twipple.jp/api/upload2";

        /// <summary>
        /// Upload picture to Twipple
        /// </summary>
        public static bool UploadToTwipple(this OAuth provider, string mediaFilePath, out string url)
        {
            url = null;
            var doc = provider.UploadToTwipple(mediaFilePath);
            if (doc.Element("rsp").Attribute("stat").Value == "ok")
            {
                url = doc.Element("rsp").Element("mediaurl").ParseString();
                return true;
            }
            else if (doc.Element("rsp").Attribute("stat").Value == "fail")
            {
                var err = doc.Element("rsp").Element("err");
                throw new Exception(err.Attribute("code").Value + ": " + err.Attribute("msg").Value);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Upload picture to Twipple
        /// </summary>
        public static XDocument UploadToTwipple(this OAuth provider, string mediaFilePath)
        {
            var req = Http.CreateRequest(new Uri(UploadApiUrl), "POST", contentType: "application/x-www-form-urlencoded");

            // use OAuth Echo
            provider.MakeOAuthEchoRequest(ref req);

            var sd = new List<SendData>();
            sd.Add(new SendData("media", file: mediaFilePath));

            var doc = Http.WebUpload<XDocument>(req, sd, Encoding.UTF8, XDocument.Load);
            if (doc.ThrownException != null)
                throw doc.ThrownException;
            else if (!doc.Succeeded)
                return null;
            return doc.Data;
        }

    }
}
