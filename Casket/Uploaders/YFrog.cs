using Acuerdo.External.Uploader;
using Dulcet.ThirdParty;
using Dulcet.Twitter.Credential;

namespace Casket.Uploaders
{
    public class YFrog : IUploader
    {
        private static string _appkey = "238DGHOTa6a8f8356246fb3d3e9c7dae65cb3970";

        public string UploadImage(OAuth credential, string path, string comment)
        {
            string url;
            if (!credential.UploadToYFrog(_appkey, path, out url))
            {
                return null;
            }
            else
            {
                return url;
            }
        }

        public string ServiceName
        {
            get { return "YFrog"; }
        }

        public bool IsResolvable(string url)
        {
            return url.StartsWith("http://yfrog.com/") || url.StartsWith("http://twitter.yfrog.com/");
        }

        public string Resolve(string url)
        {
            if (IsResolvable(url))
                return url.Replace("twitter.", "") + ":medium";
            else
                return null;
        }
    }
}
