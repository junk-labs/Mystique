using Acuerdo.External.Uploader;

namespace Casket.Resolvers
{
    public class Twipple : IResolver
    {
        public bool IsResolvable(string url)
        {
            return url.StartsWith("http://p.twipple.jp/");
        }

        public string Resolve(string url)
        {
            if (IsResolvable(url))
                return "http://p.twipple.jp/show/orig/" + url.Substring(20);
            else
                return null;
        }
    }
}
