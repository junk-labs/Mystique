using Acuerdo.External.Uploader;

namespace Casket.Resolvers
{
    public class Plixi : IResolver
    {
        public bool IsResolvable(string url)
        {
            return url.StartsWith("http://plixi.com/p/") || url.StartsWith("http://tweetphoto.com/") || url.StartsWith("http://lockerz.com/s/");
        }

        public string Resolve(string url)
        {
            if(IsResolvable(url))
                return "http://api.plixi.com/api/TPAPI.svc/imagefromurl?size=big&url=" + url;
            else
                return null;
        }
    }
}
