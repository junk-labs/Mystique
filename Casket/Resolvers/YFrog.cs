using Acuerdo.External.Uploader;

namespace Casket.Resolvers
{
    public class YFrog : IResolver
    {
        public bool IsResolvable(string url)
        {
            return url.StartsWith("http://yfrog.com/");
        }

        public string Resolve(string url)
        {
            if (IsResolvable(url))
                return url + ":medium";
            else
                return null;
        }
    }
}
