using Acuerdo.External.Uploader;
using Dulcet.ThirdParty;
using Dulcet.Twitter.Credential;

namespace Casket.Uploaders
{
    public class Twipple : IUploader
    {
        public string UploadImage(OAuth credential, string path, string comment)
        {
            string url;
            if (!credential.UploadToTwipple(path, out url))
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
            get { return "ついっぷるフォト"; }
        }

        public bool IsResolvable(string url)
        {
            return url.StartsWith("http://p.twipple.jp/");
        }

        public string Resolve(string url)
        {
            if (IsResolvable(url))
            {
                return "http://p.twipple.jp/show/orig/" + url.Substring(20);
            }
            else
            {
                return null;
            }
        }
    }
}
