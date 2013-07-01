using System;
using Acuerdo.External.Uploader;
using Dulcet.Twitter;
using Dulcet.Twitter.Credential;
using Dulcet.Twitter.Rest;

namespace Casket.Uploaders
{
    public class TwitterOfficial : IPostDelegatingUploader
    {
        public bool IsResolvable(string url)
        {
            return false;
        }

        public string Resolve(string url)
        {
            throw new NotImplementedException();
        }

        public string UploadImage(OAuth credential, string path, string comment)
        {
            throw new NotImplementedException();
        }

        public string ServiceName { get { return "Twitter Official Image Uploader"; } }

        public TwitterStatus PostAndUpload(OAuth credential, string path, string body, long? inReplyTo)
        {
            return credential.UpdateWithMedia(body, path, inReplyTo);
        }
    }
}
