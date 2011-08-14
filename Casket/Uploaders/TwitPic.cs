using Acuerdo.External.Uploader;
using Dulcet.ThirdParty;
using Dulcet.Twitter.Credential;

namespace Casket.Uploaders
{
    public class TwitPic : IUploader
    {
        private static string _appk = "f4e98ee376dc3e692342b6add361608d";


        public string UploadImage(OAuth credential, string path, string comment)
        {
            string url;
            if (!credential.UploadToTwitpic(_appk, comment, path, out url))
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
            get { return "TwitPic"; }
        }

        public bool IsResolvable(string url)
        {
            return url.StartsWith("http://twitpic.com/");
        }

        public string Resolve(string url)
        {
            if (IsResolvable(url))
            {
                return "http://twitpic.com/show/full/" + url.Substring(19);
            }
            else
            {
                return null;
            }
        }

    }
}
